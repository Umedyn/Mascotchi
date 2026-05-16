using System.Collections;
using UnityEngine;

public class CreatureAnimator : MonoBehaviour
{
    [Header("References")]
    public SpriteRenderer spriteRenderer;

    [Tooltip("Seconds per frame.")]
    public float frameRate = 0.2f;

    private MascotData _currentData;
    private string     _currentAnimation;
    private Coroutine  _animCoroutine;

    public void SetMascotData(MascotData data)
    {
        _currentData = data;
        if (_currentAnimation != null)
            PlayAnimation(_currentAnimation);
    }

    public void PlayAnimation(string animName)
    {
        if (_currentData == null)
        {
            Debug.LogWarning("[CreatureAnimator] No MascotData set.");
            return;
        }

        if (!_currentData.AnimationFrames.ContainsKey(animName))
        {
            Debug.LogWarning($"[CreatureAnimator] Animation '{animName}' not found. Falling back to Idle.");
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
            spriteRenderer.sprite = frames[index];
            index = (index + 1) % frames.Length;
            yield return new WaitForSeconds(frameRate);
        }
    }
}