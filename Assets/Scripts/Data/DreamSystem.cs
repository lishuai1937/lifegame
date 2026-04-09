using System;
using System.Collections.Generic;

/// <summary>
/// Dream/Career System
/// 
/// DESIGN:
/// - 6 years old: First dream (childhood innocent version)
/// - 18 years old: Before gaokao, ask again (can keep or change)
/// - 22 years old: After graduation, ask again
/// - 30 years old: Final ask
/// - If dream stays the same from 6 to 30: "Unwavering Heart" achievement + big bonuses
/// - Each career has unique gameplay bonuses
/// - Changing dreams is not punishment, gives "Adaptability" bonus instead
/// </summary>
[Serializable]
public class DreamSystem
{
    // Dream history (age -> chosen dream)
    public int DreamAt6 = -1;
    public int DreamAt18 = -1;
    public int DreamAt22 = -1;
    public int DreamAt30 = -1;

    // Active career (set at 22 or 30)
    public DreamCareer ActiveCareer = DreamCareer.None;

    // Track how many times dream changed
    public int TimesChanged = 0;

    /// <summary>
    /// All available dreams/careers with bonuses
    /// </summary>
    public static readonly CareerInfo[] AllCareers = new CareerInfo[]
    {
        // === Childhood favorites ===
        new CareerInfo(DreamCareer.Astronaut, "Astronaut", "Explore the stars",
            "Death probability -15% (trained for danger), unlock space dream scene at age 90"),
        new CareerInfo(DreamCareer.Doctor, "Doctor", "Save lives",
            "Death probability -20% for self, can heal NPC health crises, +karma per heal"),
        new CareerInfo(DreamCareer.Teacher, "Teacher", "Educate the next generation",
            "Social energy +3, NPC children you teach gain better traits"),
        new CareerInfo(DreamCareer.Artist, "Artist", "Create beauty",
            "Dream scenes are richer, unlock special CG at milestones, +karma from art events"),
        new CareerInfo(DreamCareer.Athlete, "Athlete", "Push physical limits",
            "Gold +80% before 40, death probability +10% after 60 (injuries), sprint speed +50%"),
        new CareerInfo(DreamCareer.Scientist, "Scientist", "Discover truth",
            "Each grid world has 1 hidden event revealed, +gold from discoveries"),
        new CareerInfo(DreamCareer.Businessman, "Businessman", "Build an empire",
            "Gold income +60%, but social energy -2 (too busy)"),

        // === More realistic ===
        new CareerInfo(DreamCareer.Chef, "Chef", "Feed the world",
            "Gold +30%, NPC closeness from Gift action doubled"),
        new CareerInfo(DreamCareer.Musician, "Musician", "Touch hearts with music",
            "NPC closeness from Chat +50%, unlock music events, special BGM"),
        new CareerInfo(DreamCareer.Writer, "Writer", "Tell stories",
            "Memoir at 85 gives +5 karma, dialogue options expanded, soul memories +2 on reincarnation"),
        new CareerInfo(DreamCareer.Programmer, "Programmer", "Build the future",
            "Gold +50%, social energy -1 (introverted work), hidden tech events"),
        new CareerInfo(DreamCareer.Lawyer, "Lawyer", "Fight for justice",
            "Can defend NPCs in crisis events (better outcomes), gold +40%"),
        new CareerInfo(DreamCareer.Soldier, "Soldier", "Protect the people",
            "Death probability +5% (danger), karma +2 per grid world, NPC respect +"),
        new CareerInfo(DreamCareer.Farmer, "Farmer", "Live off the land",
            "Countryside scenes enhanced, gold steady but low, health bonus (death prob -10%)"),
        new CareerInfo(DreamCareer.Pilot, "Pilot", "See the world from above",
            "Travel events give double gold, unlock sky scenes, death prob +3%"),
        new CareerInfo(DreamCareer.Detective, "Detective", "Uncover the truth",
            "Can discover NPC secrets, hidden events revealed, +karma for solving cases"),
        new CareerInfo(DreamCareer.Firefighter, "Firefighter", "Run into danger",
            "Can save NPCs from death events, karma +3 per save, death prob +5%"),
        new CareerInfo(DreamCareer.Vet, "Vet", "Care for animals",
            "Kindness-based NPCs gain +closeness faster, unlock animal companion"),
        new CareerInfo(DreamCareer.Architect, "Architect", "Shape the world",
            "City scenes enhanced with your designs, gold +35%, unlock building events"),
        new CareerInfo(DreamCareer.Journalist, "Journalist", "Tell the truth",
            "Can interview NPCs for hidden stories, +karma for exposing injustice"),
        new CareerInfo(DreamCareer.Superhero, "Superhero", "Save everyone",
            "Childhood-only dream. If kept to 30: legendary achievement, all bonuses halved but karma x2"),

        // === Free choice ===
        new CareerInfo(DreamCareer.Free, "Undecided", "I don't know yet",
            "No bonus, no penalty. Maximum freedom. Adaptability bonus if chosen at 30.")
    };

    /// <summary>
    /// Set dream at a milestone age
    /// </summary>
    public void SetDream(int age, DreamCareer career)
    {
        switch (age)
        {
            case 6: DreamAt6 = (int)career; break;
            case 18:
                DreamAt18 = (int)career;
                if (DreamAt6 >= 0 && DreamAt6 != (int)career) TimesChanged++;
                break;
            case 22:
                DreamAt22 = (int)career;
                if (DreamAt18 >= 0 && DreamAt18 != (int)career) TimesChanged++;
                ActiveCareer = career;
                break;
            case 30:
                DreamAt30 = (int)career;
                if (DreamAt22 >= 0 && DreamAt22 != (int)career) TimesChanged++;
                ActiveCareer = career;
                break;
        }
    }

    /// <summary>
    /// Check if dream never changed (6 == 18 == 22 == 30)
    /// </summary>
    public bool IsUnwaveringHeart()
    {
        if (DreamAt6 < 0 || DreamAt18 < 0 || DreamAt22 < 0 || DreamAt30 < 0) return false;
        return DreamAt6 == DreamAt18 && DreamAt18 == DreamAt22 && DreamAt22 == DreamAt30;
    }

    /// <summary>
    /// Get bonuses description for current career
    /// </summary>
    public string GetActiveBonusDescription()
    {
        if (ActiveCareer == DreamCareer.None) return "No career chosen yet.";
        foreach (var c in AllCareers)
            if (c.Career == ActiveCareer) return c.BonusDescription;
        return "";
    }

    /// <summary>
    /// Get death probability modifier based on career
    /// </summary>
    public float GetDeathProbModifier()
    {
        switch (ActiveCareer)
        {
            case DreamCareer.Doctor: return -0.20f;
            case DreamCareer.Astronaut: return -0.15f;
            case DreamCareer.Farmer: return -0.10f;
            case DreamCareer.Athlete: return 0.10f; // after 60
            case DreamCareer.Soldier: return 0.05f;
            case DreamCareer.Pilot: return 0.03f;
            case DreamCareer.Firefighter: return 0.05f;
            default: return 0f;
        }
    }

    /// <summary>
    /// Get gold income multiplier
    /// </summary>
    public float GetGoldMultiplier()
    {
        switch (ActiveCareer)
        {
            case DreamCareer.Businessman: return 1.6f;
            case DreamCareer.Programmer: return 1.5f;
            case DreamCareer.Athlete: return 1.8f; // before 40
            case DreamCareer.Lawyer: return 1.4f;
            case DreamCareer.Architect: return 1.35f;
            case DreamCareer.Chef: return 1.3f;
            case DreamCareer.Pilot: return 1.3f;
            default: return 1.0f;
        }
    }

    /// <summary>
    /// Get social energy modifier
    /// </summary>
    public int GetEnergyModifier()
    {
        switch (ActiveCareer)
        {
            case DreamCareer.Teacher: return 3;
            case DreamCareer.Musician: return 1;
            case DreamCareer.Businessman: return -2;
            case DreamCareer.Programmer: return -1;
            default: return 0;
        }
    }
}

[Serializable]
public class CareerInfo
{
    public DreamCareer Career;
    public string Name;
    public string ChildhoodDescription; // what a 6-year-old would say
    public string BonusDescription;

    public CareerInfo(DreamCareer c, string n, string desc, string bonus)
    { Career = c; Name = n; ChildhoodDescription = desc; BonusDescription = bonus; }
}

public enum DreamCareer
{
    None = -1,
    Astronaut = 0,
    Doctor,
    Teacher,
    Artist,
    Athlete,
    Scientist,
    Businessman,
    Chef,
    Musician,
    Writer,
    Programmer,
    Lawyer,
    Soldier,
    Farmer,
    Pilot,
    Detective,
    Firefighter,
    Vet,
    Architect,
    Journalist,
    Superhero,
    Free        // undecided
}