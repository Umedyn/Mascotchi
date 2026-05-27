using UnityEngine;
using UnityEngine.UI;

public class NeedsDisplay : MonoBehaviour
{
    [Header("Stat Bars")]
    public Slider hungerBar;
    public Slider stressBar;
    public Slider hygieneBar;

    [Header("Attribute Fill Images")]
    public Image musicBar;
    public Image gamingBar;
    public Image yappingBar;
    public Image mischievBar;
    public Image knowledgeBar;

    public void Refresh(CreatureBase creature, EvolutionTracker tracker)
    {
        hungerBar.value  = creature.Hunger  / 100f;
        stressBar.value  = creature.Stress  / 100f;
        hygieneBar.value = creature.Hygiene / 100f;

        float highest = Mathf.Max(tracker.Music, tracker.Gaming, tracker.Yapping,
                                tracker.Mischief, tracker.Knowledge);

        if (highest <= 0f)
        {
            musicBar.fillAmount = gamingBar.fillAmount = yappingBar.fillAmount
                = mischievBar.fillAmount = knowledgeBar.fillAmount = 0f;
            return;
        }

        musicBar.fillAmount     = tracker.Music     / highest;
        gamingBar.fillAmount    = tracker.Gaming    / highest;
        yappingBar.fillAmount   = tracker.Yapping   / highest;
        mischievBar.fillAmount  = tracker.Mischief  / highest;
        knowledgeBar.fillAmount = tracker.Knowledge / highest;
    }
}