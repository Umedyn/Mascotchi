using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EvolutionReveal : MonoBehaviour
{
    [Header("Flash")]
    public Image flashOverlay;
    public float flashInDuration  = 0.4f;
    public float flashHoldDuration = 0.8f;
    public float flashOutDuration = 0.8f;

    [Header("Name Text")]
    public TextMeshProUGUI evolutionNameText;
    public float nameFadeInDuration = 0.5f;
    public float nameHoldDuration   = 2.0f;
    public float nameFadeOutDuration = 0.5f;

    [Header("Egg Visuals")]
    public Image eggBackground;
    public Image eggLikeness;

    [Header("Stinger")]
    public Image stingerOverlay;
    public float stingerFadeInDuration  = 0.4f;
    public float stingerHoldDuration    = 2.0f;
    public float stingerFadeOutDuration = 0.4f;

    public void PlayReveal(MascotData winner, Action onSwapSprite, Action onComplete)
    {
        StartCoroutine(RevealSequence(winner, onSwapSprite, onComplete));
    }

    public void ShowEggVisuals(MascotData mascot)
    {
        if (mascot.EggBackground != null)
        {
            eggBackground.sprite = mascot.EggBackground;
            SetAlpha(eggBackground, 1f);
        }
        if (mascot.Likeness != null)
        {
            eggLikeness.sprite = mascot.Likeness;
            SetAlpha(eggLikeness, 1f);
        }
    }

    public void ShowPreEvolutionEggVisuals(MascotData blobData)
    {
        ShowEggVisuals(blobData);
    }

    private IEnumerator RevealSequence(MascotData winner, Action onSwapSprite, Action onComplete)
    {
        // Flash in
        yield return StartCoroutine(FadeImage(flashOverlay, 0f, 1f, flashInDuration));

        // Swap sprite while fully white
        onSwapSprite?.Invoke();

        // Apply egg visuals
        ShowEggVisuals(winner);

        yield return new WaitForSeconds(flashHoldDuration);

        // Flash out
        yield return StartCoroutine(FadeImage(flashOverlay, 1f, 0f, flashOutDuration));

        // Stinger (optional — skipped if no sprite provided)
        if (winner.Stinger != null)
        {
            stingerOverlay.sprite = winner.Stinger;
            yield return StartCoroutine(FadeImage(stingerOverlay, 0f, 1f, stingerFadeInDuration));
            yield return new WaitForSeconds(stingerHoldDuration);
            yield return StartCoroutine(FadeImage(stingerOverlay, 1f, 0f, stingerFadeOutDuration));
        }

        // Show mascot name
        evolutionNameText.text = winner.Definition.mascotName;
        yield return StartCoroutine(FadeText(evolutionNameText, 0f, 1f, nameFadeInDuration));
        yield return new WaitForSeconds(nameHoldDuration);
        yield return StartCoroutine(FadeText(evolutionNameText, 1f, 0f, nameFadeOutDuration));

        onComplete?.Invoke();
    }

    private IEnumerator FadeImage(Image image, float from, float to, float duration)
    {
        float elapsed = 0f;
        SetAlpha(image, from);
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            SetAlpha(image, Mathf.Lerp(from, to, elapsed / duration));
            yield return null;
        }
        SetAlpha(image, to);
    }

    private IEnumerator FadeText(TextMeshProUGUI text, float from, float to, float duration)
    {
        float elapsed = 0f;
        SetTextAlpha(text, from);
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            SetTextAlpha(text, Mathf.Lerp(from, to, elapsed / duration));
            yield return null;
        }
        SetTextAlpha(text, to);
    }

    private void SetAlpha(Image image, float alpha)
    {
        Color c = image.color;
        c.a = alpha;
        image.color = c;
    }

    private void SetTextAlpha(TextMeshProUGUI text, float alpha)
    {
        Color c = text.color;
        c.a = alpha;
        text.color = c;
    }
}