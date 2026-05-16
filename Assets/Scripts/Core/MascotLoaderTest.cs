using System.Collections.Generic;
using UnityEngine;

public class MascotLoaderTest : MonoBehaviour
{
    void Start()
    {
        List<MascotData> mascots = MascotLoader.LoadAll();

        foreach (MascotData m in mascots)
        {
            Debug.Log($"--- {m.Definition.mascotName} ({m.Definition.vtuberName}) ---");
            Debug.Log($"  Primary Activity: {m.Definition.primaryActivity}");
            Debug.Log($"  Evolution Profile: music={m.Definition.evolutionProfile.music} gaming={m.Definition.evolutionProfile.gaming} yapping={m.Definition.evolutionProfile.yapping} mischief={m.Definition.evolutionProfile.mischief} knowledge={m.Definition.evolutionProfile.knowledge}");
            Debug.Log($"  Animations loaded: {m.AnimationFrames.Count}");
            foreach (var anim in m.AnimationFrames)
                Debug.Log($"    {anim.Key}: {anim.Value.Length} frame(s)");
            Debug.Log($"  EggBG: {(m.EggBackground != null ? "loaded" : "missing")}");
            Debug.Log($"  Likeness: {(m.Likeness != null ? "loaded" : "missing")}");
            Debug.Log($"  Food: {(m.Food != null ? "loaded" : "missing")}");
        }
    }
}