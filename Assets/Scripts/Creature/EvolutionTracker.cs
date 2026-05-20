using System;
using System.Collections.Generic;
using UnityEngine;

public class EvolutionTracker : MonoBehaviour
{
    [Header("Attribute Increments")]
    public float activityIncrementAmount = 10f;

    [Header("Passive Mischief")]
    [Tooltip("Mischief gained per tick while Hygiene is below the low threshold.")]
    public float mischievFromLowHygiene = 2f;

    [Header("Neglect Thresholds")]
    public float lowStatThreshold      = 30f;
    public float lowStressStatThreshold = 30f;

    [Header("Profile Evaluation")]
    [Tooltip("Score bonus per neglect tick during profile evaluation. Tune during playtesting.")]
    public float neglectWeight = 0.5f;

    public float Music     { get; private set; }
    public float Gaming    { get; private set; }
    public float Yapping   { get; private set; }
    public float Mischief  { get; private set; }
    public float Knowledge { get; private set; }

    public int LowHygieneTicks { get; private set; }
    public int LowHungerTicks  { get; private set; }
    public int HighStressTicks { get; private set; }

    // Tiebreak: in-memory only. Resets with each new game, which is correct.
    private readonly Dictionary<string, DateTime> _firstIncrementTime = new Dictionary<string, DateTime>();

    public void LoadFrom(SaveData data)
    {
        Music     = data.attrMusic;
        Gaming    = data.attrGaming;
        Yapping   = data.attrYapping;
        Mischief  = data.attrMischief;
        Knowledge = data.attrKnowledge;

        LowHygieneTicks = data.lowHygieneTicks;
        LowHungerTicks  = data.lowHungerTicks;
        HighStressTicks = data.highStressTicks;
    }

    public void ApplyActionAttributes(ActionType action)
    {
        switch (action)
        {
            case ActionType.Karaoke:   IncrementAttribute("Music",     activityIncrementAmount); break;
            case ActionType.Gaming:    IncrementAttribute("Gaming",    activityIncrementAmount); break;
            case ActionType.Streaming: IncrementAttribute("Yapping",   activityIncrementAmount); break;
            case ActionType.Marbles:   IncrementAttribute("Mischief",  activityIncrementAmount); break;
            case ActionType.Coding:    IncrementAttribute("Knowledge", activityIncrementAmount); break;
            // Feed, Rest, Clean do not build attributes.
        }
    }

    public void TrackNeglect(float hunger, float stress, float hygiene)
    {
        if (hygiene < lowStatThreshold)
            LowHygieneTicks++;
        if (hunger < lowStatThreshold)
            LowHungerTicks++;
        if (stress < lowStressStatThreshold)
            HighStressTicks++;
    }

    public MascotData EvaluateProfile(List<MascotData> mascots)
    {
        if (mascots == null || mascots.Count == 0)
        {
            Debug.LogError("[EvolutionTracker] No mascots available to evaluate.");
            return null;
        }

        MascotData best      = null;
        float      bestScore = float.MinValue;
        DateTime   bestTime  = DateTime.MaxValue;

        foreach (MascotData mascot in mascots)
        {
            float    score     = ScoreMascot(mascot);
            DateTime firstBuilt = GetEarliestRelevantTimestamp(mascot);

            bool isBetter = score > bestScore
                         || (Mathf.Approximately(score, bestScore) && firstBuilt < bestTime);

            if (isBetter)
            {
                best      = mascot;
                bestScore = score;
                bestTime  = firstBuilt;
            }
        }

        Debug.Log($"[EvolutionTracker] Winner: {best?.Definition.mascotName} (score: {bestScore:F1})");
        return best;
    }

    private float ScoreMascot(MascotData mascot)
    {
        var p = mascot.Definition.evolutionProfile;
        float score = p.music     * Music
                    + p.gaming    * Gaming
                    + p.yapping   * Yapping
                    + p.mischief  * Mischief
                    + p.knowledge * Knowledge;

        var t = mascot.Definition.statTendencies;
        if (t.prefersLowHygiene) score += LowHygieneTicks * neglectWeight;
        if (t.prefersLowHunger)  score += LowHungerTicks  * neglectWeight;
        if (t.prefersHighStress) score += HighStressTicks  * neglectWeight;

        return score;
    }

    // Tiebreak: find the attribute with the highest weight for this mascot.
    // The mascot whose top attribute was built first wins.
    private DateTime GetEarliestRelevantTimestamp(MascotData mascot)
    {
        var p = mascot.Definition.evolutionProfile;

        string topAttr   = "Music";
        float  topWeight = p.music;

        if (p.gaming    > topWeight) { topAttr = "Gaming";    topWeight = p.gaming;    }
        if (p.yapping   > topWeight) { topAttr = "Yapping";   topWeight = p.yapping;   }
        if (p.mischief  > topWeight) { topAttr = "Mischief";  topWeight = p.mischief;  }
        if (p.knowledge > topWeight) { topAttr = "Knowledge"; }

        return _firstIncrementTime.TryGetValue(topAttr, out DateTime t) ? t : DateTime.MaxValue;
    }

    private void IncrementAttribute(string attr, float amount)
    {
        if (!_firstIncrementTime.ContainsKey(attr))
            _firstIncrementTime[attr] = DateTime.UtcNow;

        switch (attr)
        {
            case "Music":     Music     += amount; break;
            case "Gaming":    Gaming    += amount; break;
            case "Yapping":   Yapping   += amount; break;
            case "Mischief":  Mischief  += amount; break;
            case "Knowledge": Knowledge += amount; break;
            default: Debug.LogWarning($"[EvolutionTracker] Unknown attribute: {attr}"); break;
        }
    }
}