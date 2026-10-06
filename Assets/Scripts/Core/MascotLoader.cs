using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class MascotLoader
{
    private static readonly string[] ValidActivities =
        { "Karaoke", "Gaming", "Streaming", "Marbles", "Coding" };

    // Drop-in folder for mascot packages added after the game is built.
    // PC (Windows): next to Mascotchi.exe
    // Mac: next to Mascotchi.app
    // Android: Android/data/<package name>/files/Mascots/
    // Editor: project root, beside the Assets folder
    public static string ExternalMascotsPath
    {
        get
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            return Path.Combine(Application.persistentDataPath, "Mascots");
#elif UNITY_STANDALONE_OSX && !UNITY_EDITOR
            return Path.Combine(Directory.GetParent(Directory.GetParent(Application.dataPath).FullName).FullName, "Mascots");
#else
            return Path.Combine(Directory.GetParent(Application.dataPath).FullName, "Mascots");
#endif
        }
    }

    public static List<MascotData> LoadAll()
    {
        var results = new List<MascotData>();

        // --- Built-in mascots (packed into the build from Resources/Mascots/) ---
        TextAsset[] allDefinitions = Resources.LoadAll<TextAsset>("Mascots");

        if (allDefinitions.Length == 0)
            Debug.LogWarning("[MascotLoader] No definition files found under Resources/Mascots/.");

        foreach (TextAsset asset in allDefinitions)
        {
            if (!asset.name.EndsWith("_Definition"))
                continue;

            string mascotName = asset.name.Replace("_Definition", "");
            ProcessMascot(mascotName, asset.text, results);
        }

        // --- Drop-in mascots (loose files on disk) ---
        LoadExternal(results);

        Debug.Log($"[MascotLoader] Finished. {results.Count} mascot(s) registered.");
        return results;
    }

    private static void LoadExternal(List<MascotData> results)
    {
        string root = ExternalMascotsPath;

        try
        {
            // Creates the folder on first launch so players can find it.
            Directory.CreateDirectory(root);
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[MascotLoader] Could not access drop-in folder '{root}'. ({e.Message})");
            return;
        }

        Debug.Log($"[MascotLoader] Scanning drop-in folder: {root}");

        foreach (string dir in Directory.GetDirectories(root))
        {
            string mascotName = Path.GetFileName(dir);

            if (mascotName == "Blob" || results.Exists(m => m.Definition.mascotName == mascotName))
            {
                Debug.LogWarning($"[MascotLoader] {mascotName}: a mascot with this name is already loaded. Drop-in copy skipped.");
                continue;
            }

            string defPath = Path.Combine(dir, $"{mascotName}_Definition.json");
            if (!File.Exists(defPath))
            {
                Debug.LogWarning($"[MascotLoader] {mascotName}: no {mascotName}_Definition.json found, skipping.");
                continue;
            }

            string json;
            try
            {
                json = File.ReadAllText(defPath);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[MascotLoader] {mascotName}: could not read definition file, skipping. ({e.Message})");
                continue;
            }

            string spriteDir = Path.Combine(dir, "Sprites");
            ProcessMascot(mascotName, json, results, null, () => LoadSpritesFromFolder(spriteDir, mascotName));
        }
    }

    private static Sprite[] LoadSpritesFromFolder(string folder, string mascotName)
    {
        var sprites = new List<Sprite>();
        if (!Directory.Exists(folder))
            return sprites.ToArray();

        foreach (string file in Directory.GetFiles(folder))
        {
            // Case-insensitive check so .PNG also works on Android.
            if (Path.GetExtension(file).ToLowerInvariant() != ".png")
                continue;

            byte[] bytes;
            try
            {
                bytes = File.ReadAllBytes(file);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[MascotLoader] {mascotName}: could not read '{Path.GetFileName(file)}'. ({e.Message})");
                continue;
            }

            var tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            if (!tex.LoadImage(bytes))
            {
                Debug.LogWarning($"[MascotLoader] {mascotName}: '{Path.GetFileName(file)}' is not a valid PNG, ignoring.");
                continue;
            }

            // Match the built-in import settings so pixel art stays crisp.
            tex.filterMode = FilterMode.Point;
            tex.wrapMode   = TextureWrapMode.Clamp;

            string spriteName = Path.GetFileNameWithoutExtension(file);
            tex.name = spriteName;

            Sprite sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100f);
            sprite.name = spriteName;
            sprites.Add(sprite);
        }

        return sprites.ToArray();
    }

    private static void ProcessMascot(string mascotName, string json, List<MascotData> results,
                                      string spritePathOverride = null, Func<Sprite[]> spriteLoader = null)
    {
        // --- Parse JSON ---
        MascotDefinition def;
        try
        {
            def = JsonUtility.FromJson<MascotDefinition>(json);
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[MascotLoader] {mascotName}: malformed JSON, skipping. ({e.Message})");
            // TODO: Replace with UI popup warning when UIManager exists.
            return;
        }

        // --- Validate definition ---
        if (def == null || string.IsNullOrEmpty(def.mascotName))
        {
            Debug.LogWarning($"[MascotLoader] {mascotName}: mascotName field is empty, skipping.");
            return;
        }

        if (def.mascotName != mascotName)
        {
            Debug.LogWarning($"[MascotLoader] {mascotName}: mascotName in JSON ('{def.mascotName}') does not match folder name, skipping.");
            return;
        }

        if (Array.IndexOf(ValidActivities, def.primaryActivity) < 0)
        {
            Debug.LogWarning($"[MascotLoader] {mascotName}: primaryActivity '{def.primaryActivity}' is not valid, skipping.");
            return;
        }

        if (def.evolutionProfile == null)
        {
            Debug.LogWarning($"[MascotLoader] {mascotName}: evolutionProfile is missing, skipping.");
            return;
        }

        // --- Load sprites ---
        // Built-in packages load from Resources; drop-in packages pass their own loader.
        string spritePath = spritePathOverride ?? $"Mascots/{mascotName}/Sprites";
        Sprite[] allSprites = spriteLoader != null ? spriteLoader() : Resources.LoadAll<Sprite>(spritePath);

        if (allSprites == null || allSprites.Length == 0)
        {
            Debug.LogWarning($"[MascotLoader] {mascotName}: no sprites found in its Sprites folder, skipping.");
            return;
        }

        // --- Organise sprites by animation name and frame number ---
        var rawFrames = new Dictionary<string, List<(int frame, Sprite sprite)>>();
        Sprite eggBg = null;
        Sprite likeness = null;
        Sprite food = null;
        Sprite stinger = null;

        foreach (Sprite sprite in allSprites)
        {
            string[] parts = sprite.name.Replace(".png", "").Split('_');
            // parts[0] is always mascotName

            if (parts.Length == 2)
            {
                // Single sprites: MascotName_EggBG, _Likeness, _Food, _Stinger
                switch (parts[1])
                {
                    case "EggBG":    eggBg    = sprite; break;
                    case "Likeness": likeness = sprite; break;
                    case "Food":     food     = sprite; break;
                    case "Stinger":  stinger  = sprite; break;
                    default:
                        Debug.LogWarning($"[MascotLoader] {mascotName}: unrecognised single sprite '{sprite.name}', ignoring.");
                        break;
                }
            }
            else if (parts.Length == 3)
            {
                // Animated frames: MascotName_AnimationName_FrameNumber
                string animName = parts[1];
                if (!int.TryParse(parts[2], out int frameNum))
                {
                    Debug.LogWarning($"[MascotLoader] {mascotName}: could not parse frame number from '{sprite.name}', ignoring sprite.");
                    continue;
                }

                if (!rawFrames.ContainsKey(animName))
                    rawFrames[animName] = new List<(int, Sprite)>();

                rawFrames[animName].Add((frameNum, sprite));
            }
            else
            {
                Debug.LogWarning($"[MascotLoader] {mascotName}: unexpected sprite name format '{sprite.name}', ignoring.");
            }
        }

        // Idle is the only animation required for a valid package
        if (!rawFrames.ContainsKey("Idle"))
        {
            Debug.LogWarning($"[MascotLoader] {mascotName}: no Idle animation found, skipping.");
            return;
        }

        // Sort each animation's frames by frame number and convert to arrays
        var animationFrames = new Dictionary<string, Sprite[]>();
        foreach (KeyValuePair<string, List<(int frame, Sprite sprite)>> kvp in rawFrames)
        {
            kvp.Value.Sort((a, b) => a.frame.CompareTo(b.frame));
            var sorted = new Sprite[kvp.Value.Count];
            for (int i = 0; i < kvp.Value.Count; i++)
                sorted[i] = kvp.Value[i].sprite;
            animationFrames[kvp.Key] = sorted;
        }

        // --- Register ---
        results.Add(new MascotData
        {
            Definition      = def,
            AnimationFrames = animationFrames,
            EggBackground   = eggBg,
            Likeness        = likeness,
            Food            = food,
            Stinger         = stinger
        });

        Debug.Log($"[MascotLoader] Registered '{mascotName}' — {animationFrames.Count} animation(s), {allSprites.Length} sprite(s) total.");
    }

    public static MascotData LoadBlob()
    {
        TextAsset defAsset = Resources.Load<TextAsset>("Creature/Blob/Blob_Definition");
        if (defAsset == null)
        {
            Debug.LogError("[MascotLoader] Blob definition not found at Resources/Creature/Blob/Blob_Definition.");
            return null;
        }

        var results = new List<MascotData>();
        ProcessMascot("Blob", defAsset.text, results, "Creature/Blob/Sprites");

        if (results.Count == 0)
        {
            Debug.LogError("[MascotLoader] Blob failed to load.");
            return null;
        }

        Debug.Log("[MascotLoader] Blob loaded.");
        return results[0];
    }
}
