using UnityEngine;

/// <summary>
/// Stat Growth System - Tracks and triggers stat increases from various sources
/// 
/// STAT GROWTH SOURCES:
/// 
/// CHARISMA:
/// - Unlock an NPC (+1 every 5 unlocks)
/// - NPC approaches you (+1 every 3 approaches)
/// - Successfully invite NPC to your event (+1)
/// - Confess love and succeed (+2)
/// - Career: Businessman +1/world, Musician +1/world, Lawyer +1/world
/// 
/// RESILIENCE:
/// - Survive any hardship world event (+1 to +2)
/// - Survive death risk grid (+1)
/// - Get betrayed (+1)
/// - Lose gold (>200 at once) (+1)
/// - Career: Soldier +1/world, Firefighter +1/world, Farmer +1/world
/// 
/// WILLPOWER:
/// - Resist NPC manipulation (+1)
/// - Break free from toxic relationship (+2)
/// - Keep same dream across milestones (+1 each time)
/// - Refuse a bribe/temptation (+1)
/// - Career: Soldier +1/world, Detective +1/world
/// 
/// EMPATHY:
/// - Help an NPC in crisis (+1)
/// - Attend NPC funeral (+1)
/// - Lend money to NPC (+1)
/// - Have a close relationship (50+) with 3+ NPCs (+1)
/// - Career: Doctor +1/world, Teacher +1/world, Vet +1/world
/// 
/// LUCK:
/// - Hidden "karma echo": good karma occasionally boosts luck (+1 when karma > 10)
/// - Find hidden events (Scientist/Detective) (+1)
/// - Survive a high death probability grid (+1)
/// - Reincarnation with soul memories (+1 per reincarnation)
/// - Career: no direct boost (luck is fate, not skill)
/// </summary>
public class StatGrowth : MonoBehaviour
{
    public static StatGrowth Instance { get; private set; }

    // Counters for threshold-based growth
    private int npcUnlockCount = 0;
    private int npcApproachCount = 0;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    PlayerStats GetStats()
    {
        return GameManager.Instance?.Player?.Stats;
    }

    // ==================== CHARISMA ====================

    public void OnNPCUnlocked()
    {
        npcUnlockCount++;
        if (npcUnlockCount % 5 == 0) // every 5 unlocks
        {
            var s = GetStats(); if (s == null) return;
            s.Modify(StatType.Charisma, 1);
            Debug.Log("[StatGrowth] Charisma +1 (unlocked 5 NPCs)");
        }
    }

    public void OnNPCApproachedPlayer()
    {
        npcApproachCount++;
        if (npcApproachCount % 3 == 0) // every 3 approaches
        {
            var s = GetStats(); if (s == null) return;
            s.Modify(StatType.Charisma, 1);
            Debug.Log("[StatGrowth] Charisma +1 (3 NPCs approached you)");
        }
    }

    public void OnInviteAccepted()
    {
        var s = GetStats(); if (s == null) return;
        s.Modify(StatType.Charisma, 1);
        Debug.Log("[StatGrowth] Charisma +1 (invite accepted)");
    }

    public void OnConfessSuccess()
    {
        var s = GetStats(); if (s == null) return;
        s.Modify(StatType.Charisma, 2);
        Debug.Log("[StatGrowth] Charisma +2 (confession success)");
    }

    // ==================== RESILIENCE ====================

    public void OnSurvivedHardship()
    {
        var s = GetStats(); if (s == null) return;
        s.Modify(StatType.Resilience, 1);
        Debug.Log("[StatGrowth] Resilience +1 (survived hardship)");
    }

    public void OnSurvivedDeathRisk()
    {
        var s = GetStats(); if (s == null) return;
        s.Modify(StatType.Resilience, 1);
        Debug.Log("[StatGrowth] Resilience +1 (survived death risk)");
    }

    public void OnBetrayed()
    {
        var s = GetStats(); if (s == null) return;
        s.Modify(StatType.Resilience, 1);
        Debug.Log("[StatGrowth] Resilience +1 (betrayed)");
    }

    public void OnBigGoldLoss(int amount)
    {
        if (amount >= 200)
        {
            var s = GetStats(); if (s == null) return;
            s.Modify(StatType.Resilience, 1);
            Debug.Log("[StatGrowth] Resilience +1 (big financial loss)");
        }
    }

    // ==================== WILLPOWER ====================

    public void OnResistedManipulation()
    {
        var s = GetStats(); if (s == null) return;
        s.Modify(StatType.Willpower, 1);
        Debug.Log("[StatGrowth] Willpower +1 (resisted manipulation)");
    }

    public void OnBrokeFreeToxic()
    {
        var s = GetStats(); if (s == null) return;
        s.Modify(StatType.Willpower, 2);
        Debug.Log("[StatGrowth] Willpower +2 (broke free from toxic)");
    }

    public void OnKeptDream()
    {
        var s = GetStats(); if (s == null) return;
        s.Modify(StatType.Willpower, 1);
        Debug.Log("[StatGrowth] Willpower +1 (kept same dream)");
    }

    public void OnRefusedTemptation()
    {
        var s = GetStats(); if (s == null) return;
        s.Modify(StatType.Willpower, 1);
        Debug.Log("[StatGrowth] Willpower +1 (refused temptation)");
    }

    // ==================== EMPATHY ====================

    public void OnHelpedNPCCrisis()
    {
        var s = GetStats(); if (s == null) return;
        s.Modify(StatType.Empathy, 1);
        Debug.Log("[StatGrowth] Empathy +1 (helped NPC in crisis)");
    }

    public void OnAttendedFuneral()
    {
        var s = GetStats(); if (s == null) return;
        s.Modify(StatType.Empathy, 1);
        Debug.Log("[StatGrowth] Empathy +1 (attended funeral)");
    }

    public void OnLentMoney()
    {
        var s = GetStats(); if (s == null) return;
        s.Modify(StatType.Empathy, 1);
        Debug.Log("[StatGrowth] Empathy +1 (lent money)");
    }

    public void OnDeepRelationshipsMilestone(int closeCount)
    {
        if (closeCount >= 3)
        {
            var s = GetStats(); if (s == null) return;
            s.Modify(StatType.Empathy, 1);
            Debug.Log("[StatGrowth] Empathy +1 (3+ close relationships)");
        }
    }

    // ==================== LUCK ====================

    public void CheckKarmaEcho()
    {
        var p = GameManager.Instance?.Player;
        if (p == null) return;
        // Good karma occasionally boosts luck
        if (p.KarmaValue > 10 && Random.value < 0.2f) // 20% chance when karma > 10
        {
            p.Stats.Modify(StatType.Luck, 1);
            Debug.Log("[StatGrowth] Luck +1 (karma echo - good deeds rewarded)");
        }
    }

    public void OnFoundHiddenEvent()
    {
        var s = GetStats(); if (s == null) return;
        s.Modify(StatType.Luck, 1);
        Debug.Log("[StatGrowth] Luck +1 (found hidden event)");
    }

    public void OnSurvivedHighDeathProb(float prob)
    {
        if (prob >= 0.3f) // survived 30%+ death chance
        {
            var s = GetStats(); if (s == null) return;
            s.Modify(StatType.Luck, 1);
            Debug.Log("[StatGrowth] Luck +1 (survived high death probability)");
        }
    }

    public void OnReincarnation()
    {
        var s = GetStats(); if (s == null) return;
        s.Modify(StatType.Luck, 1);
        Debug.Log("[StatGrowth] Luck +1 (reincarnation with memories)");
    }

    // ==================== CAREER PASSIVE GROWTH ====================

    /// <summary>
    /// Called once per grid world entry, gives career-specific stat growth
    /// </summary>
    public void ApplyCareerPassive()
    {
        var p = GameManager.Instance?.Player;
        if (p?.Dream == null || p.Stats == null) return;

        switch (p.Dream.ActiveCareer)
        {
            case DreamCareer.Businessman:
            case DreamCareer.Musician:
            case DreamCareer.Lawyer:
                p.Stats.Modify(StatType.Charisma, 1);
                Debug.Log($"[StatGrowth] Career passive: Charisma +1 ({p.Dream.ActiveCareer})");
                break;

            case DreamCareer.Soldier:
                p.Stats.Modify(StatType.Resilience, 1);
                p.Stats.Modify(StatType.Willpower, 1);
                Debug.Log("[StatGrowth] Career passive: Resilience +1, Willpower +1 (Soldier)");
                break;

            case DreamCareer.Firefighter:
            case DreamCareer.Farmer:
                p.Stats.Modify(StatType.Resilience, 1);
                Debug.Log($"[StatGrowth] Career passive: Resilience +1 ({p.Dream.ActiveCareer})");
                break;

            case DreamCareer.Detective:
                p.Stats.Modify(StatType.Willpower, 1);
                Debug.Log("[StatGrowth] Career passive: Willpower +1 (Detective)");
                break;

            case DreamCareer.Doctor:
            case DreamCareer.Teacher:
            case DreamCareer.Vet:
                p.Stats.Modify(StatType.Empathy, 1);
                Debug.Log($"[StatGrowth] Career passive: Empathy +1 ({p.Dream.ActiveCareer})");
                break;

            case DreamCareer.Athlete:
                p.Stats.Modify(StatType.Resilience, 1);
                Debug.Log("[StatGrowth] Career passive: Resilience +1 (Athlete)");
                break;

            case DreamCareer.Writer:
                p.Stats.Modify(StatType.Empathy, 1);
                Debug.Log("[StatGrowth] Career passive: Empathy +1 (Writer)");
                break;

            case DreamCareer.Journalist:
                p.Stats.Modify(StatType.Charisma, 1);
                Debug.Log("[StatGrowth] Career passive: Charisma +1 (Journalist)");
                break;
        }
    }

    public void ResetForNewLife()
    {
        npcUnlockCount = 0;
        npcApproachCount = 0;
    }
}