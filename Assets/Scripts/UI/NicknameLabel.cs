using TMPro;
using UnityEngine;

[RequireComponent(typeof(TextMeshProUGUI))]
public class NicknameLabel : MonoBehaviour
{
    [Tooltip("The creature's Image RectTransform (the one CreatureAnimator drives).")]
    public RectTransform creature;

    [Tooltip("Gap in pixels between the top of the creature and the label.")]
    public float padding = 4f;

    private TextMeshProUGUI _text;
    private RectTransform   _rect;

    void Awake()
    {
        _text = GetComponent<TextMeshProUGUI>();
        _rect = (RectTransform)transform;
    }

    void LateUpdate()
    {
        GameManager gm = GameManager.Instance;
        if (gm == null || gm.CurrentSave == null || creature == null) return;

        string label = gm.CurrentSave.nickname;
        if (string.IsNullOrWhiteSpace(label))
            label = gm.CurrentSave.isEvolved ? gm.CurrentSave.activeMascotId : "Blob";
        if (_text.text != label) _text.text = label;

        Vector3 top = creature.TransformPoint(new Vector3(creature.rect.center.x, creature.rect.yMax, 0f));
        _rect.position = top;
        _rect.anchoredPosition += new Vector2(0f, padding);
    }
}