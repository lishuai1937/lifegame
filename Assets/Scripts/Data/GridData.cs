using System;

/// <summary>
/// Single grid cell data
/// </summary>
[Serializable]
public class GridData
{
    public int Age;
    public GridType Type;
    public string Title;
    public string Description;
    public AgePhase Phase;
    public string SceneId;
    public bool HasDeathRisk;
    public float DeathProbability;
    public int BaseGoldReward;
    // KarmaImpact removed from grid data - karma is driven by player actions inside the open world
}

/// <summary>
/// Result returned when exiting a grid world
/// Karma changes come from in-world actions, not predefined
/// </summary>
[Serializable]
public class GridWorldResult
{
    public int GoldEarned;
    public int KarmaChange;      // determined by player actions inside the world
    public bool IsDead;
    public string[] ItemsGained;
    public string Achievement;
}

[Serializable]
public class GridDataCollection
{
    public GridData[] Grids;
}