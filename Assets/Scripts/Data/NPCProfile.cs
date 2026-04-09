using System;

/// <summary>
/// NPC identity and personality data
/// Every NPC in the world has unique traits that affect how relationships develop
/// </summary>
[Serializable]
public class NPCProfile
{
    public string Id;               // unique id (persistent across grid worlds if recurring)
    public string Name;
    public int Gender;              // 0=male 1=female
    public int Age;                 // NPC's age in current grid world

    // Personality traits (0-10 scale)
    public int Kindness;            // affects willingness to help, forgive
    public int Ambition;            // career-driven, competitive
    public int Humor;               // lighthearted, jokes
    public int Loyalty;             // sticks with you through hard times
    public int Temper;              // easily angered, confrontational
    public int Introversion;        // 0=extrovert 10=introvert

    // Social role
    public NPCRole Role;            // family, classmate, colleague, stranger, etc.

    /// <summary>
    /// Generate a random personality
    /// </summary>
    public static NPCProfile GenerateRandom(string name, NPCRole role, int age)
    {
        var r = new Random(name.GetHashCode() + age);
        return new NPCProfile
        {
            Id = Guid.NewGuid().ToString().Substring(0, 8),
            Name = name,
            Gender = r.Next(2),
            Age = age,
            Kindness = r.Next(0, 11),
            Ambition = r.Next(0, 11),
            Humor = r.Next(0, 11),
            Loyalty = r.Next(0, 11),
            Temper = r.Next(0, 11),
            Introversion = r.Next(0, 11),
            Role = role
        };
    }
}

public enum NPCRole
{
    Family,         // parent, sibling, child, spouse
    Classmate,      // school/university
    Colleague,      // work
    Neighbor,       // lives nearby
    Stranger,       // random encounter
    Authority,      // teacher, boss, doctor
    Romantic,       // love interest
    Rival,          // competitor, antagonist
    Elder,          // wise old person
    Child           // young person
}