using System;
using System.Collections.Generic;
using UnityEngine;

public static class MascotLoader
{
    private static readonly string[] ValidActivities =
        { "Karaoke", "Gaming", "Streaming", "Marbles", "Coding" };

    public static List<MascotData> LoadAll()
    {
        var results = new List<MascotData>();

        // Resources.LoadAll recurses into subfolders.
        // JSON files in a Resources folder load as TextAsset.
        TextAsset[] allDefinitions = Resources.LoadAll<TextAsset>("Mascots");

        if (allDefinitions.Length == 0)
        {
            Debug.LogWarning("[MascotLoader] No definition files found under Resources/Mascots/.");
            return results;
        }

        foreach (TextAsset asset in allDefinitions)
        {
            if (!asset.name.EndsWith("_Definition"))
                continue;

            string mascotName = asset.name.Replace("_Definition", "");
            ProcessMascot(mascotName, asset.text, results);
        }

        Debug.Log($"[MascotLoader] Finished. {results.Count} mascot(s) registered.");
        return results;
    }

    private static void ProcessMascot(string mascotName, string json, List<MascotData> results)
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
        if (string.IsNullOrEmpty(def.mascotName))
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
        string spritePath = $"Mascots/{mascotName}/Sprites";
        Sprite[] allSprites = Resources.LoadAll<Sprite>(spritePath);

        if (allSprites == null || allSprites.Length == 0)
        {
            Debug.LogWarning($"[MascotLoader] {mascotName}: no sprites found at Resources/{spritePath}, skipping.");
            return;
        }

        // --- Organise sprites by animation name and frame number ---
        var rawFrames = new Dictionary<string, List<(int frame, Sprite sprite)>>();
        Sprite eggBg = null;
        Sprite likeness = null;
        Sprite food = null;

        foreach (Sprite sprite in allSprites)
        {
            string[] parts = sprite.name.Replace(".png", "").Split('_');
            // parts[0] is always mascotName

            if (parts.Length == 2)
            {
                // Single sprites: MascotName_EggBG, _Likeness, _Food
                switch (parts[1])
                {
                    case "EggBG":   eggBg    = sprite; break;
                    case "Likeness": likeness = sprite; break;
                    case "Food":    food     = sprite; break;
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
            Definition     = def,
            AnimationFrames = animationFrames,
            EggBackground  = eggBg,
            Likeness       = likeness,
            Food           = food
        });

        Debug.Log($"[MascotLoader] Registered '{mascotName}' — {animationFrames.Count} animation(s), {allSprites.Length} sprite(s) total.");
    }
}