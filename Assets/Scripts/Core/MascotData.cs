using System.Collections.Generic;
using UnityEngine;

public class MascotData
{
    public MascotDefinition Definition;
    public Dictionary<string, Sprite[]> AnimationFrames; // key: e.g. "Idle", "Feed"
    public Sprite EggBackground;
    public Sprite Likeness;
    public Sprite Food;
}