using System;

/// <summary>
/// Player save data - all persistent state
/// </summary>
[Serializable]
public class PlayerData
{
    // Basic info
    public string PlayerName = "Traveler";
    public int Gender = 0; // 0=male 1=female

    // Life progress
    public int CurrentAge = 0;
    public WorldRealm CurrentRealm = WorldRealm.Mortal;
    public int ReincarnationCount = 0;

    // Economy
    public int Gold = 0;

    // Hidden karma (player never sees the number)
    public int KarmaValue = 0;

    // Charm (from owned assets - affects NPC interactions)
    public int Charm = 0;

    // Dice
    public DiceSpeed CurrentDiceSpeed = DiceSpeed.Slow;
    public int NextSpeedChoiceAge = 20;

    // Family (generated at age 6)
    public FamilyBackground Family;

    // Life path
    public string CurrentLifePath = "default";

    // Items
    public string[] UnlockedItems = Array.Empty<string>();
    public string[] Achievements = Array.Empty<string>();

    // Soul memories (from previous lives)
    public string[] SoulMemories = Array.Empty<string>();

    // Regret pill: which grids have been "regretted" (cannot re-enter)
    public int[] RegrettedGrids = Array.Empty<int>();

    public AgePhase GetAgePhase()
    {
        if (CurrentAge <= 12) return AgePhase.Childhood;
        if (CurrentAge <= 17) return AgePhase.Youth;
        if (CurrentAge <= 30) return AgePhase.Young;
        if (CurrentAge <= 50) return AgePhase.Prime;
        if (CurrentAge <= 65) return AgePhase.Middle;
        return AgePhase.Elder;
    }
}

[Serializable]
public class FamilyBackground
{
    public int WealthLevel = 1;
    public int FamilySize = 3;
    public string FamilyTrait = "";
    public int InitialGold = 0;
}