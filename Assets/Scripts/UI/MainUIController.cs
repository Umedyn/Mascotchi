using UnityEngine;

public class MainUIController : MonoBehaviour
{
    public static MainUIController Instance { get; private set; }

    [Header("References")]
    public NeedsDisplay     needsDisplay;
    public CreatureBase     creatureBase;
    public EvolutionTracker evolutionTracker;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        GameManager.Instance.timeManager.OnTick += RefreshDisplay;
        RefreshDisplay();
    }

    public void RefreshDisplay() =>
        needsDisplay.Refresh(creatureBase, evolutionTracker);
}