using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Systems")]
    public TimeManager     timeManager;
    public CreatureBase    creatureBase;
    public EvolutionTracker evolutionTracker;
    public CreatureAnimator creatureAnimator;

    [Header("Progression")]
    [Tooltip("Action quota for the Blob stage. Adolescent quota is 2x this. Keep low for testing.")]
    public int blobQuota = 10;

    public List<MascotData> LoadedMascots { get; private set; }
    public SaveData CurrentSave           { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start() => Boot();

    private void Boot()
    {
        LoadedMascots = MascotLoader.LoadAll();

        CurrentSave = SaveSystem.SaveExists()
            ? SaveSystem.Load()
            : InitializeNewSave();

        if (!SaveSystem.SaveExists())
            SaveSystem.Save(CurrentSave);

        LoadCreatureState();

        timeManager.Initialize(CurrentSave.lastSessionTimestamp);
        timeManager.OnTick          += HandleTick;
        timeManager.OnCatchUpTicks  += HandleCatchUp;

        Debug.Log($"[GameManager] Boot complete. Stage: {CurrentSave.growthStage}, Nickname: '{CurrentSave.nickname}'");
    }

    private SaveData InitializeNewSave()
    {
        Debug.Log("[GameManager] No save found — initializing new game.");
        return new SaveData
        {
            hunger = 100f, stress = 100f, hygiene = 100f,
            growthStage = GrowthStage.Blob,
            nickname = "Blob"
        };
    }

    private void LoadCreatureState()
    {
        creatureBase.LoadFrom(CurrentSave);
        evolutionTracker.LoadFrom(CurrentSave);

        if (CurrentSave.isEvolved)
        {
            MascotData active = LoadedMascots.Find(m => m.Definition.mascotName == CurrentSave.activeMascotId);
            if (active != null) { creatureAnimator.SetMascotData(active); creatureAnimator.PlayAnimation("Idle"); }
            else Debug.LogWarning($"[GameManager] Active mascot '{CurrentSave.activeMascotId}' not found.");
        }
        else if (LoadedMascots.Count > 0)
        {
            // Pre-evolution: use first registered mascot as placeholder sprite source.
            creatureAnimator.SetMascotData(LoadedMascots[0]);
            creatureAnimator.PlayAnimation("Idle");
        }
    }

    public void PerformAction(ActionType action)
    {
        if (CurrentSave.growthStage == GrowthStage.Evolved) return;

        creatureBase.ApplyActionStats(action);
        evolutionTracker.ApplyActionAttributes(action);
        CurrentSave.actionCount++;

        if (CurrentSave.growthStage == GrowthStage.Blob && CurrentSave.actionCount >= blobQuota)
        {
            CurrentSave.growthStage = GrowthStage.Adolescent;
            CurrentSave.actionCount = 0;
            Debug.Log("[GameManager] Advanced to Adolescent.");
        }
        else if (CurrentSave.growthStage == GrowthStage.Adolescent && CurrentSave.actionCount >= blobQuota * 2)
        {
            TriggerEvolution();
            return;
        }

        SyncAndSave();
    }

    private void TriggerEvolution()
    {
        MascotData winner = evolutionTracker.EvaluateProfile(LoadedMascots);
        if (winner == null) { Debug.LogError("[GameManager] Evolution returned null. Aborting."); return; }

        CurrentSave.growthStage    = GrowthStage.Evolved;
        CurrentSave.isEvolved      = true;
        CurrentSave.evolvedMascotId = winner.Definition.mascotName;
        CurrentSave.activeMascotId  = winner.Definition.mascotName;

        if (!CurrentSave.unlockedMascots.Contains(winner.Definition.mascotName))
            CurrentSave.unlockedMascots.Add(winner.Definition.mascotName);

        // TODO Phase 6: replace with full evolution reveal sequence.
        creatureAnimator.SetMascotData(winner);
        creatureAnimator.PlayAnimation("Idle");

        SyncAndSave();
        Debug.Log($"[GameManager] Evolution complete. Winner: {winner.Definition.mascotName}");
    }

    private void HandleTick()
    {
        if (CurrentSave.growthStage == GrowthStage.Evolved) return;
        creatureBase.ApplyTick();
        evolutionTracker.TrackNeglect(creatureBase.Hunger, creatureBase.Stress, creatureBase.Hygiene);
        SyncAndSave();
    }

    private void HandleCatchUp(int count)
    {
        if (CurrentSave.growthStage == GrowthStage.Evolved) return;
        for (int i = 0; i < count; i++)
        {
            creatureBase.ApplyTick();
            evolutionTracker.TrackNeglect(creatureBase.Hunger, creatureBase.Stress, creatureBase.Hygiene);
        }
        SyncAndSave();
        Debug.Log($"[GameManager] Catch-up done. Hunger: {creatureBase.Hunger:F1} Stress: {creatureBase.Stress:F1} Hygiene: {creatureBase.Hygiene:F1}");
    }

    private void SyncAndSave()
    {
        CurrentSave.hunger  = creatureBase.Hunger;
        CurrentSave.stress  = creatureBase.Stress;
        CurrentSave.hygiene = creatureBase.Hygiene;

        CurrentSave.attrMusic     = evolutionTracker.Music;
        CurrentSave.attrGaming    = evolutionTracker.Gaming;
        CurrentSave.attrYapping   = evolutionTracker.Yapping;
        CurrentSave.attrMischief  = evolutionTracker.Mischief;
        CurrentSave.attrKnowledge = evolutionTracker.Knowledge;

        CurrentSave.lowHygieneTicks = evolutionTracker.LowHygieneTicks;
        CurrentSave.lowHungerTicks  = evolutionTracker.LowHungerTicks;
        CurrentSave.highStressTicks = evolutionTracker.HighStressTicks;

        SaveSystem.Save(CurrentSave);
    }

    void OnApplicationQuit() { if (CurrentSave != null) SyncAndSave(); }
}