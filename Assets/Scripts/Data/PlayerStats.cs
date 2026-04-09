using System;
using UnityEngine;

/// <summary>
/// Player Stats - Hidden attributes that affect NPC interactions and world events
/// Player doesn't see exact numbers, only feels the effects
/// </summary>
[Serializable]
public class PlayerStats
{
    // === CORE STATS (hidden from player) ===
    public int Charisma = 5;        // 0-20: ability to influence NPCs
    public int Resilience = 5;      // 0-20: ability to endure hardship
    public int Willpower = 5;       // 0-20: resistance to NPC manipulation
    public int Empathy = 5;         // 0-20: ability to sense NPC emotions
    public int Luck = 5;            // 0-20: random event bias

    /// <summary>
    /// Modify stat (clamped 0-20)
    /// </summary>
    public void Modify(StatType stat, int delta)
    {
        switch (stat)
        {
            case StatType.Charisma: Charisma = Mathf.Clamp(Charisma + delta, 0, 20); break;
            case StatType.Resilience: Resilience = Mathf.Clamp(Resilience + delta, 0, 20); break;
            case StatType.Willpower: Willpower = Mathf.Clamp(Willpower + delta, 0, 20); break;
            case StatType.Empathy: Empathy = Mathf.Clamp(Empathy + delta, 0, 20); break;
            case StatType.Luck: Luck = Mathf.Clamp(Luck + delta, 0, 20); break;
        }
    }

    public int Get(StatType stat)
    {
        switch (stat)
        {
            case StatType.Charisma: return Charisma;
            case StatType.Resilience: return Resilience;
            case StatType.Willpower: return Willpower;
            case StatType.Empathy: return Empathy;
            case StatType.Luck: return Luck;
            default: return 0;
        }
    }
}

public enum StatType { Charisma, Resilience, Willpower, Empathy, Luck }