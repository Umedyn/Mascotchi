using System;
using System.Globalization;
using UnityEngine;

public class TimeManager : MonoBehaviour
{
    [Tooltip("Seconds per tick. Keep low during development; set to 60 for production.")]
    public float tickIntervalSeconds = 5f;

    [Tooltip("Maximum ticks applied after a long absence. Configurable per GDD.")]
    public int maxAwayTicks = 8;

    public event Action OnTick;
    public event Action<int> OnCatchUpTicks;

    private float _timer;

    public void Initialize(string lastTimestamp)
    {
        int missed = CalculateMissedTicks(lastTimestamp);
        if (missed > 0)
        {
            Debug.Log($"[TimeManager] Applying {missed} catch-up tick(s).");
            OnCatchUpTicks?.Invoke(missed);
        }
        _timer = 0f;
    }

    void Update()
    {
        _timer += Time.deltaTime;
        if (_timer >= tickIntervalSeconds)
        {
            _timer -= tickIntervalSeconds;
            OnTick?.Invoke();
        }
    }

    private int CalculateMissedTicks(string lastTimestamp)
    {
        if (string.IsNullOrEmpty(lastTimestamp)) return 0;

        if (!DateTime.TryParse(lastTimestamp, null, DateTimeStyles.RoundtripKind, out DateTime last))
        {
            Debug.LogWarning("[TimeManager] Could not parse lastSessionTimestamp.");
            return 0;
        }

        int ticks = Mathf.FloorToInt((float)(DateTime.UtcNow - last).TotalSeconds / tickIntervalSeconds);
        return Mathf.Min(ticks, maxAwayTicks);
    }
}