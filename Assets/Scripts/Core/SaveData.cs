using System;
using System.Collections.Generic;

[Serializable]
public enum GrowthStage { Blob, Adolescent, Evolved }

[Serializable]
public class SaveData
{
    public float hunger = 100f;
    public float stress = 100f;
    public float hygiene = 100f;

    public float attrMusic;
    public float attrGaming;
    public float attrYapping;
    public float attrMischief;
    public float attrKnowledge;

    public int actionCount;
    public GrowthStage growthStage = GrowthStage.Blob;
    public string lastSessionTimestamp;

    public bool isEvolved;
    public string evolvedMascotId;
    public string activeMascotId;
    public List<string> unlockedMascots = new List<string>();
    public string nickname;
    public int lowHygieneTicks;
    public int lowHungerTicks;
    public int highStressTicks;
}