using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Tracks player actions inside a grid world and calculates karma
/// Karma is HIDDEN from the player - they only see consequences, not numbers
/// 
/// Actions are logged during open world gameplay, karma is calculated on exit
/// </summary>
public class KarmaTracker : MonoBehaviour
{
    public static KarmaTracker Instance { get; private set; }

    // Action log for current grid world visit
    private List<KarmaAction> currentActions = new List<KarmaAction>();

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    /// <summary>
    /// Log an action the player took (called by EventTrigger, NPC interactions, etc.)
    /// The player does NOT see the karma value - only the narrative result
    /// </summary>
    public void LogAction(string actionId, string description, int hiddenKarma)
    {
        currentActions.Add(new KarmaAction
        {
            ActionId = actionId,
            Description = description,
            KarmaValue = hiddenKarma,
            Timestamp = Time.time
        });
        Debug.Log($"[KarmaTracker] Action: {description} (hidden karma: {hiddenKarma})");
    }

    // ==================== Common Actions ====================

    /// <summary>Help someone - positive karma, player just sees "You helped them"</summary>
    public void HelpedSomeone(string context)
    {
        int karma = Random.Range(1, 4); // 1-3, slightly random
        LogAction("help", context, karma);
    }

    /// <summary>Hurt/steal/betray - negative karma</summary>
    public void HarmedSomeone(string context)
    {
        int karma = Random.Range(-3, -1); // -3 to -1
        LogAction("harm", context, karma);
    }

    /// <summary>Made a selfish choice - mild negative</summary>
    public void SelfishChoice(string context)
    {
        int karma = Random.Range(-2, 0); // -2 to -1, or 0
        LogAction("selfish", context, karma);
    }

    /// <summary>Made a selfless choice - mild positive</summary>
    public void SelflessChoice(string context)
    {
        int karma = Random.Range(1, 3);
        LogAction("selfless", context, karma);
    }

    /// <summary>Neutral action - tiny random swing</summary>
    public void NeutralAction(string context)
    {
        int karma = Random.Range(-1, 2); // -1, 0, or 1
        LogAction("neutral", context, karma);
    }

    /// <summary>Ignored someone in need</summary>
    public void IgnoredSomeone(string context)
    {
        int karma = Random.Range(-2, 0);
        LogAction("ignore", context, karma);
    }

    // ==================== Settlement ====================

    /// <summary>
    /// Calculate total karma from this grid world visit
    /// Called when player exits the open world
    /// </summary>
    public int SettleAndReset()
    {
        int total = 0;
        foreach (var a in currentActions)
            total += a.KarmaValue;

        Debug.Log($"[KarmaTracker] World exit - {currentActions.Count} actions, net karma: {total}");
        currentActions.Clear();
        return total;
    }

    /// <summary>
    /// Get action count (for UI hints like "You made X choices")
    /// </summary>
    public int GetActionCount() => currentActions.Count;

    /// <summary>
    /// Clear without settling (e.g. on death)
    /// </summary>
    public void Reset() => currentActions.Clear();
}

[System.Serializable]
public class KarmaAction
{
    public string ActionId;
    public string Description;
    public int KarmaValue;      // hidden from player
    public float Timestamp;
}