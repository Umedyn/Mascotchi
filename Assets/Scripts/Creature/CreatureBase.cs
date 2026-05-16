using UnityEngine;

public class CreatureBase : MonoBehaviour
{
    [Header("Decay Per Tick")]
    public float hungerDecayPerTick  = 10f;
    public float stressDecayPerTick  = 8f;
    public float hygieneDecayPerTick = 5f;

    [Tooltip("Multiplier on stress decay when Hunger reaches 0.")]
    public float stressDecayHungerMultiplier  = 2f;

    [Tooltip("Multiplier on hygiene decay when Stress reaches 0.")]
    public float hygieneDecayStressMultiplier = 2f;

    [Header("Action Restore Amounts")]
    public float feedRestoreAmount   = 30f;
    public float restRestoreAmount   = 30f;
    public float cleanRestoreAmount  = 30f;
    public float activityStressCost  = 15f;

    public float Hunger  { get; private set; }
    public float Stress  { get; private set; }
    public float Hygiene { get; private set; }

    public void LoadFrom(SaveData data)
    {
        Hunger  = data.hunger;
        Stress  = data.stress;
        Hygiene = data.hygiene;
    }

    public void ApplyTick()
    {
        float effectiveStressDecay  = Hunger <= 0f
            ? stressDecayPerTick  * stressDecayHungerMultiplier
            : stressDecayPerTick;

        float effectiveHygieneDecay = Stress <= 0f
            ? hygieneDecayPerTick * hygieneDecayStressMultiplier
            : hygieneDecayPerTick;

        Hunger  = Mathf.Max(0f, Hunger  - hungerDecayPerTick);
        Stress  = Mathf.Max(0f, Stress  - effectiveStressDecay);
        Hygiene = Mathf.Max(0f, Hygiene - effectiveHygieneDecay);
    }

    public void ApplyActionStats(ActionType action)
    {
        switch (action)
        {
            case ActionType.Feed:
                Hunger  = Mathf.Min(100f, Hunger  + feedRestoreAmount);
                break;
            case ActionType.Rest:
                Stress  = Mathf.Min(100f, Stress  + restRestoreAmount);
                break;
            case ActionType.Clean:
                Hygiene = Mathf.Min(100f, Hygiene + cleanRestoreAmount);
                break;
            default:
                // All activities cost Stress.
                // TODO Phase 7: reduce cost for primary activity post-evolution.
                Stress = Mathf.Max(0f, Stress - activityStressCost);
                break;
        }
    }
}