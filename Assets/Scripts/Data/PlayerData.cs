using System;

/// <summary>
/// Player save data
/// </summary>
[Serializable]
public class PlayerData
{
    public string PlayerName = "Traveler";
    public int Gender = 0;

    // Life progress
    public int CurrentAge = 0;
    public WorldRealm CurrentRealm = WorldRealm.Mortal;
    public int ReincarnationCount = 0;

    // Economy
    public int Gold = 0;

    // Hidden karma (player never sees the number)
    public int KarmaValue = 0;

    // Dice
    public DiceSpeed CurrentDiceSpeed = DiceSpeed.Slow;
    public int NextSpeedChoiceAge = 20;

    // Family background (generated at age 6)
    public FamilyBackground Family;

    // Dream/Career system
    public DreamSystem Dream = new DreamSystem();

    // Hidden stats (player doesn't see numbers, only feels effects)
    public PlayerStats Stats = new PlayerStats();

    // Assets (housing, vehicle, business, investments, collectibles)
    public AssetSystem Assets = new AssetSystem();

    // Health (physical + mental, diseases, injuries)
    public HealthSystem Health = new HealthSystem();

    // Education & Skills
    public SkillSystem Skills = new SkillSystem();

    // Reputation (how society sees you)
    public ReputationSystem Reputation = new ReputationSystem();

    // Diary (auto-recorded life events)
    public DiarySystem Diary = new DiarySystem();

    // Last Wishes (bucket list, unlocks at 70)
    public LastWishSystem LastWishes = new LastWishSystem();

    // Housing (floor plan + furniture slots)
    public HousingSystem Housing = new HousingSystem();

    // Achievements & memories
    public string[] UnlockedItems = Array.Empty<string>();
    public string[] Achievements = Array.Empty<string>();
    public string[] SoulMemories = Array.Empty<string>();

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