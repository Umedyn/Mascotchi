using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RosterGallery : MonoBehaviour
{
    [Header("References")]
    public Transform     contentParent;
    public GameObject    entryPrefab;

    public void Refresh()
    {
        foreach (Transform child in contentParent)
            Destroy(child.gameObject);

        List<MascotData> allMascots = GameManager.Instance.LoadedMascots;
        List<string>     unlocked   = GameManager.Instance.CurrentSave.unlockedMascots;

        foreach (MascotData mascot in allMascots)
        {
            GameObject entry   = Instantiate(entryPrefab, contentParent);
            Image      img     = entry.transform.Find("MascotImage").GetComponent<Image>();
            TextMeshProUGUI mascotName  = entry.transform.Find("MascotName").GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI vtuberName  = entry.transform.Find("VtuberName").GetComponent<TextMeshProUGUI>();

            bool isUnlocked = unlocked.Contains(mascot.Definition.mascotName);

            if (mascot.AnimationFrames.ContainsKey("Idle"))
                img.sprite = mascot.AnimationFrames["Idle"][0];

            if (isUnlocked)
            {
                // Full colour
                img.color = Color.white;
                mascotName.text = mascot.Definition.mascotName;
                vtuberName.text = mascot.Definition.vtuberName;
            }
            else
            {
                // Black silhouette — preserve alpha, zero out RGB
                img.color = new Color(0f, 0f, 0f, 1f);
                mascotName.text = "";
                vtuberName.text = "";
            }
        }
    }
}