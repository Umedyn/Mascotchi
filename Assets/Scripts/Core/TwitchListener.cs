using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net.Security;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

// Reads the active mascot's Twitch chat anonymously and plays streamer commands.
// Read-only: never posts, never logs in, never loads video. Every chat line that is
// not a valid command from the streamer is discarded immediately and never stored.
public class TwitchListener : MonoBehaviour
{
    [Header("References")]
    public ActionPanel actionPanel;

    [Header("Streamer Commands")]
    [Tooltip("Seconds between accepted streamer commands. Separate from the player's cooldown.")]
    public float streamerCooldown = 60f;

    [Tooltip("Turn off to disable the listener entirely.")]
    public bool listenerEnabled = true;

    [Tooltip("TESTING ONLY. If set, listens to this channel instead of the active mascot's twitchChannel. Must be empty in release builds.")]
    public string testChannelOverride = "";

    private const string Host          = "irc.chat.twitch.tv";
    private const int    Port          = 6697;                 // TLS
    private const string CommandPrefix = "!mascotchi ";
    private const int    ReadTimeoutMs = 6 * 60 * 1000;        // Twitch pings about every 5 minutes
    private const float  ChannelCheckInterval = 1f;

    private static readonly Dictionary<string, ActionType> Commands = new Dictionary<string, ActionType>
    {
        { "feed",      ActionType.Feed },
        { "rest",      ActionType.Rest },
        { "clean",     ActionType.Clean },
        { "karaoke",   ActionType.Karaoke },
        { "gaming",    ActionType.Gaming },
        { "streaming", ActionType.Streaming },
        { "marbles",   ActionType.Marbles },
        { "coding",    ActionType.Coding },
    };

    private readonly ConcurrentQueue<ActionType> _pending = new ConcurrentQueue<ActionType>();
    private readonly object _clientLock = new object();
    private TcpClient     _client;
    private volatile int  _generation;   // bumped on every stop; old threads see the change and exit
    private string        _channel = ""; // channel the current connection is for ("" = not listening)
    private float         _cooldownRemaining;
    private float         _channelCheckTimer;
    private bool          _paused;

    void Update()
    {
        if (_cooldownRemaining > 0f) _cooldownRemaining -= Time.unscaledDeltaTime;

        _channelCheckTimer -= Time.unscaledDeltaTime;
        if (_channelCheckTimer <= 0f)
        {
            _channelCheckTimer = ChannelCheckInterval;
            string desired = _paused ? "" : GetDesiredChannel();
            if (desired != _channel)
            {
                StopListening();
                if (desired != "") StartListening(desired);
            }
        }

        while (_pending.TryDequeue(out ActionType action))
            HandleCommand(action);
    }

    // ---------- Main thread ----------

    private void HandleCommand(ActionType action)
    {
        if (_cooldownRemaining > 0f)
        {
            Debug.Log($"[TwitchListener] Streamer command '{action}' ignored: streamer cooldown.");
            return;
        }
        if (!IsIdle())
        {
            Debug.Log($"[TwitchListener] Streamer command '{action}' ignored: creature not idle.");
            return;
        }

        _cooldownRemaining = streamerCooldown;
        Debug.Log($"[TwitchListener] Streamer command '{action}' accepted.");
        actionPanel.RunStreamerAction(action);
    }

    private bool IsIdle()
    {
        GameManager gm = GameManager.Instance;
        if (gm == null || gm.CurrentSave == null || !gm.CurrentSave.isEvolved) return false;
        if (actionPanel == null || actionPanel.IsBusy) return false;
        if (gm.evolutionReveal != null && gm.evolutionReveal.IsPlaying) return false;
        if (PanelManager.Instance == null || !PanelManager.Instance.IsMainPanelActive()) return false;
        return true;
    }

    private string GetDesiredChannel()
    {
        if (!listenerEnabled) return "";
        if (!string.IsNullOrWhiteSpace(testChannelOverride)) return NormalizeChannel(testChannelOverride);

        GameManager gm = GameManager.Instance;
        if (gm == null || gm.CurrentSave == null || !gm.CurrentSave.isEvolved || gm.LoadedMascots == null) return "";

        MascotData active = gm.LoadedMascots.Find(m => m.Definition.mascotName == gm.CurrentSave.activeMascotId);
        if (active == null) return "";
        return NormalizeChannel(active.Definition.twitchChannel);
    }

    // Accepts "Name", "#name" or a twitch.tv link; returns the bare lowercase login.
    private static string NormalizeChannel(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return "";
        string s = raw.Trim().ToLowerInvariant();
        int slash = s.LastIndexOf('/');
        if (slash >= 0) s = s.Substring(slash + 1);

        StringBuilder sb = new StringBuilder(s.Length);
        foreach (char c in s)
            if ((c >= 'a' && c <= 'z') || (c >= '0' && c <= '9') || c == '_') sb.Append(c);
        return sb.ToString();
    }

    private void StartListening(string channel)
    {
        _channel = channel;
        int gen = _generation;
        Thread thread = new Thread(() => Run(channel, gen)) { IsBackground = true, Name = "TwitchListener" };
        thread.Start();
    }

    private void StopListening()
    {
        _generation++;
        lock (_clientLock)
        {
            _client?.Close();   // unblocks the reader thread
            _client = null;
        }
        _channel = "";
        while (_pending.TryDequeue(out _)) { }   // missed commands are lost
    }

    void OnApplicationPause(bool paused)
    {
        // Android backgrounding: drop the connection, reconnect when the app returns.
        _paused = paused;
        if (paused) StopListening();
        else _channelCheckTimer = 0f;
    }

    void OnDestroy()          => StopListening();
    void OnApplicationQuit()  => StopListening();

    // ---------- Background thread ----------

    private void Run(string channel, int gen)
    {
        System.Random rng = new System.Random();
        int retryDelay = 2;

        while (gen == _generation)
        {
            try
            {
                using (TcpClient client = new TcpClient())
                {
                    lock (_clientLock)
                    {
                        if (gen != _generation) return;
                        _client = client;
                    }

                    client.Connect(Host, Port);
                    using (SslStream ssl = new SslStream(client.GetStream(), false))
                    {
                        ssl.AuthenticateAsClient(Host);
                        ssl.ReadTimeout = ReadTimeoutMs;

                        StreamReader reader = new StreamReader(ssl, Encoding.UTF8);
                        StreamWriter writer = new StreamWriter(ssl, new UTF8Encoding(false)) { NewLine = "\r\n", AutoFlush = true };

                        // Anonymous read-only login: no account, no password.
                        writer.WriteLine("NICK justinfan" + rng.Next(10000, 99999));
                        writer.WriteLine("JOIN #" + channel);
                        Debug.Log($"[TwitchListener] Listening to #{channel}.");
                        retryDelay = 2;

                        string line;
                        while (gen == _generation && (line = reader.ReadLine()) != null)
                        {
                            if (line.StartsWith("PING"))
                            {
                                writer.WriteLine("PONG" + line.Substring(4));
                                continue;
                            }
                            if (line.StartsWith(":tmi.twitch.tv RECONNECT")) break;

                            TryParseCommand(line, channel);
                        }
                    }
                }
            }
            catch (Exception)
            {
                // Dropped connection, timeout, no internet, or closed by StopListening.
            }
            finally
            {
                lock (_clientLock)
                {
                    if (gen == _generation) _client = null;
                }
            }

            if (gen != _generation) return;

            // Wait before reconnecting, checking often so a stop is noticed quickly.
            for (int waited = 0; waited < retryDelay * 10 && gen == _generation; waited++)
                Thread.Sleep(100);
            retryDelay = Math.Min(retryDelay * 2, 60);
        }
    }

    // Expected form: ":login!login@login.tmi.twitch.tv PRIVMSG #channel :message"
    private void TryParseCommand(string line, string channel)
    {
        if (line.Length == 0 || line[0] != ':') return;

        int space = line.IndexOf(' ');
        int bang  = line.IndexOf('!');
        if (bang < 2 || space < 0 || bang > space) return;

        string sender = line.Substring(1, bang - 1);
        if (!string.Equals(sender, channel, StringComparison.OrdinalIgnoreCase)) return;   // streamer only

        string marker = " PRIVMSG #" + channel + " :";
        int at = line.IndexOf(marker, StringComparison.OrdinalIgnoreCase);
        if (at < 0) return;

        string text = CleanMessage(line.Substring(at + marker.Length));
        if (!text.StartsWith(CommandPrefix)) return;

        if (Commands.TryGetValue(text.Substring(CommandPrefix.Length), out ActionType action))
            _pending.Enqueue(action);
    }

    // Strips invisible characters some chat clients add to repeated messages,
    // collapses whitespace, and lowercases, so the exact-match check is reliable.
    private static string CleanMessage(string text)
    {
        StringBuilder sb = new StringBuilder(text.Length);
        foreach (char c in text)
        {
            if (char.IsSurrogate(c)) continue;
            UnicodeCategory cat = CharUnicodeInfo.GetUnicodeCategory(c);
            if (cat == UnicodeCategory.Format || cat == UnicodeCategory.NonSpacingMark || cat == UnicodeCategory.Control) continue;
            sb.Append(char.IsWhiteSpace(c) ? ' ' : c);
        }
        string[] parts = sb.ToString().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        return string.Join(" ", parts).ToLowerInvariant();
    }
}
