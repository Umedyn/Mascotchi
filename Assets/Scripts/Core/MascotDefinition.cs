using System;

[Serializable]
public class EvolutionProfile
{
    public float music;
    public float gaming;
    public float yapping;
    public float mischief;
    public float knowledge;
}

[Serializable]
public class StatTendencies
{
    public bool prefersLowHygiene;
    public bool prefersLowHunger;
    public bool prefersHighStress;
}

[Serializable]
public class MascotDefinition
{
    public string mascotName;
    public string vtuberName;
    public string primaryActivity;
    public EvolutionProfile evolutionProfile;
    public StatTendencies statTendencies;
}