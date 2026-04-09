using System;
using UnityEngine;

/// <summary>
/// Reputation System - How society sees you
/// Affects: NPC initial attitude, job opportunities, event options
/// Player can see a vague description but not the number
/// </summary>
[Serializable]
public class ReputationSystem
{
    // -100 (infamous) to +100 (legendary)
    public int Reputation = 0;

    public void Modify(int delta)
    {
        Reputation = Mathf.Clamp(Reputation + delta, -100, 100);
    }

    /// <summary>
    /// NPC initial closeness bonus based on reputation
    /// </summary>
    public int GetInitialClosenessBonus()
    {
        if (Reputation > 50) return 10;     // famous, people like you
        if (Reputation > 20) return 5;
        if (Reputation > 0) return 1;
        if (Reputation > -20) return 0;
        if (Reputation > -50) return -5;    // bad rep, people avoid you
        return -10;                          // infamous
    }

    /// <summary>
    /// Description for UI (vague, not numeric)
    /// </summary>
    public string GetDescription()
    {
        if (Reputation > 70) return "A living legend. Everyone knows your name.";
        if (Reputation > 40) return "Well-respected in the community.";
        if (Reputation > 15) return "People speak well of you.";
        if (Reputation > -15) return "An ordinary person. Nothing special.";
        if (Reputation > -40) return "Some people whisper behind your back.";
        if (Reputation > -70) return "Your name carries a bad reputation.";
        return "Infamous. People cross the street to avoid you.";
    }

    // Sources of reputation change:
    public void OnHelpedCommunity() { Modify(3); }
    public void OnPublicAchievement() { Modify(5); }    // career milestone, award
    public void OnScandal() { Modify(-10); }
    public void OnCriminalAct() { Modify(-15); }
    public void OnCharity(int amount) { Modify(amount >= 500 ? 5 : 2); }
    public void OnBusinessSuccess() { Modify(3); }
    public void OnBusinessFailure() { Modify(-2); }
    public void OnPublicSpeech() { Modify(2); }
    public void OnBetrayalExposed() { Modify(-8); }
}