using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CreatureAnimator : MonoBehaviour
{
    [Header("References")]
    public Image creatureImage;

    [Header("Idle Movement")]
    public float idleNudgeAmount  = 10f;
    public float idleNudgeChance  = 0.75f;
    public float idleNudgeBoundary = 30f;

    [Header("Food")]
    public Image foodImage;

    private Vector2 _basePosition;

    [Tooltip("Seconds per frame.")]
    public float frameRate = 0.2f;

    private MascotData _currentData;
    private string     _currentAnimation;
    private Coroutine  _animCoroutine;
    private RectTransform rectTransform;

    public void SetMascotData(MascotData data)
    {
        _currentData = data;
        if (_currentAnimation != null)
            PlayAnimation(_currentAnimation);
    }

    public void PlayAnimation(string animName)
    {
        if (_currentData == null) { Debug.LogWarning("[CreatureAnimator] No MascotData set."); return; }

        if (!_currentData.AnimationFrames.ContainsKey(animName))
        {
            Debug.LogWarning($"[CreatureAnimator] '{animName}' not found, falling back to Idle.");
            animName = "Idle";
            if (!_currentData.AnimationFrames.ContainsKey(animName)) return;
        }

        if (_animCoroutine != null) StopCoroutine(_animCoroutine);
        _currentAnimation = animName;
        _animCoroutine    = StartCoroutine(AnimateLoop(animName));
    }

    private IEnumerator AnimateLoop(string animName)
    {
        Sprite[] frames = _currentData.AnimationFrames[animName];
        int index = 0;
        while (true)
        {
            creatureImage.sprite = frames[index];
            index++;
            if (index >= frames.Length)
            {
                index = 0;
                if (animName == "Idle")
                    NudgePosition();
            }
            yield return new WaitForSeconds(frameRate);
        }
    }

    public void SetStage(GrowthStage stage)
    {
        transform.localScale = stage == GrowthStage.Adolescent || stage == GrowthStage.Evolved
            ? Vector3.one
            : new Vector3(0.5f, 0.5f, 1f);
    }

    private void NudgePosition()
    {
        if (Random.value > idleNudgeChance) return;

        float direction = Random.value > 0.5f ? 1f : -1f;
        Vector2 current = rectTransform != null
            ? rectTransform.anchoredPosition
            : (Vector2)transform.localPosition;

        float newX = Mathf.Clamp(current.x + (idleNudgeAmount * direction),
            -idleNudgeBoundary, idleNudgeBoundary);

        if (rectTransform != null)
            rectTransform.anchoredPosition = new Vector2(newX, current.y);
        else
            transform.localPosition = new Vector3(newX, transform.localPosition.y, 0f);
    }

    public void ResetPosition()
    {
        if (rectTransform != null)
            rectTransform.anchoredPosition = new Vector2(0f, rectTransform.anchoredPosition.y);
        else
            transform.localPosition = new Vector3(0f, transform.localPosition.y, 0f);
    }

    public void PlayOnce(string animName, System.Action onComplete)
    {
        if (_currentData == null) return;

        if (!_currentData.AnimationFrames.ContainsKey(animName))
        {
            Debug.LogWarning($"[CreatureAnimator] '{animName}' not found, skipping.");
            onComplete?.Invoke();
            return;
        }

        if (_animCoroutine != null) StopCoroutine(_animCoroutine);
        _animCoroutine = StartCoroutine(AnimateOnce(animName, onComplete));
    }

    private IEnumerator AnimateOnce(string animName, System.Action onComplete)
    {
        Sprite[] frames = _currentData.AnimationFrames[animName];
        for (int i = 0; i < frames.Length; i++)
        {
            creatureImage.sprite = frames[i];
            yield return new WaitForSeconds(frameRate);
        }
        onComplete?.Invoke();
    }

    public void ShowFood(System.Action onComplete)
    {
        if (foodImage == null) { onComplete?.Invoke(); return; }

        Sprite foodSprite = _currentData?.Food;
        if (foodSprite == null) { onComplete?.Invoke(); return; }

        foodImage.sprite = foodSprite;
        foodImage.gameObject.SetActive(true);
        StartCoroutine(HideFoodAfterDelay(1f, onComplete));
    }

    private IEnumerator HideFoodAfterDelay(float delay, System.Action onComplete)
    {
        yield return new WaitForSeconds(delay);
        foodImage.gameObject.SetActive(false);
        onComplete?.Invoke();
    }

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        _basePosition = rectTransform != null
            ? rectTransform.anchoredPosition
            : (Vector2)transform.localPosition;
    }
}