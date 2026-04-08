using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// NPC Relationship / Affinity system
/// Tracks intimacy with NPCs across grid worlds
/// High affinity NPCs follow player to next grid world (not shadow)
/// NPCs age, can get sick, can die
/// </summary>
public class RelationshipManager : MonoBehaviour
{
    public static RelationshipManager Instance { get; private set; }

    // All known NPC relationships
    public List<NPCRelationship> Relationships = new List<NPCRelationship>();

    // Threshold for NPC to follow to next world
    public int FollowThreshold = 30;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    /// <summary>
    /// Get or create relationship with an NPC
    /// </summary>
    public NPCRelationship GetRelationship(string npcId)
    {
        foreach (var r in Relationships)
            if (r.NpcId == npcId) return r;

        var newRel = new NPCRelationship { NpcId = npcId };
        Relationships.Add(newRel);
        return newRel;
    }

    /// <summary>
    /// Change affinity with an NPC (from dialogue choices, actions, etc.)
    /// </summary>
    public void ChangeAffinity(string npcId, int amount, string reason = "")
    {
        var rel = GetRelationship(npcId);
        rel.Affinity = Mathf.Clamp(rel.Affinity + amount, -100, 100);
        rel.InteractionCount++;
        Debug.Log($"[Relationship] {npcId}: affinity {(amount>0?"+":"")}{amount} = {rel.Affinity} ({reason})");
    }

    /// <summary>
    /// Get list of NPCs that should follow to next grid world
    /// </summary>
    public List<NPCRelationship> GetFollowingNPCs()
    {
        var following = new List<NPCRelationship>();
        foreach (var r in Relationships)
        {
            if (r.Affinity >= FollowThreshold && r.IsAlive && !r.HasLeft)
                following.Add(r);
        }
        return following;
    }

    /// <summary>
    /// Check if a specific NPC should appear as revealed (not shadow)
    /// </summary>
    public bool IsNPCRevealed(string npcId)
    {
        var rel = GetRelationship(npcId);
        return rel.Affinity >= FollowThreshold || rel.InteractionCount > 0;
    }

    /// <summary>
    /// Age all NPCs by one year. Some may get sick or die at old ages.
    /// </summary>
    public void AgeAllNPCs(int playerAge)
    {
        foreach (var r in Relationships)
        {
            if (!r.IsAlive) continue;
            r.NpcAge++;

            // NPCs can die of old age (random chance increases with age)
            if (r.NpcAge > 70)
            {
                float deathChance = (r.NpcAge - 70) * 0.02f;
                if (UnityEngine.Random.value < deathChance)
                {
                    r.IsAlive = false;
                    Debug.Log($"[Relationship] {r.NpcId} has passed away at age {r.NpcAge}");
                }
            }

            // NPCs can get sick
            if (r.NpcAge > 50 && !r.IsSick)
            {
                if (UnityEngine.Random.value < 0.03f)
                {
                    r.IsSick = true;
                    Debug.Log($"[Relationship] {r.NpcId} got sick");
                }
            }
        }
    }

    /// <summary>
    /// Mark an NPC as having left the player's life
    /// </summary>
    public void NPCLeaves(string npcId, string reason)
    {
        var rel = GetRelationship(npcId);
        rel.HasLeft = true;
        rel.LeftReason = reason;
        Debug.Log($"[Relationship] {npcId} left: {reason}");
    }
}

[Serializable]
public class NPCRelationship
{
    public string NpcId;
    public string NpcName;
    public int Affinity = 0;            // -100 to 100
    public int InteractionCount = 0;
    public int NpcAge = 0;
    public bool IsAlive = true;
    public bool IsSick = false;
    public bool HasLeft = false;
    public string LeftReason = "";
    public string Role = "";            // friend, mentor, lover, rival, family

    public string GetRelationshipLevel()
    {
        if (Affinity >= 80) return "Soulmate";
        if (Affinity >= 50) return "Close Friend";
        if (Affinity >= 30) return "Friend";
        if (Affinity >= 10) return "Acquaintance";
        if (Affinity >= -10) return "Stranger";
        if (Affinity >= -30) return "Disliked";
        return "Enemy";
    }
}