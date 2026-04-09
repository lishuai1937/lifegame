using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Social System v2 - Reworked energy model
/// 
/// NEW DESIGN:
/// - Talking to any NPC is FREE (no energy cost)
/// - Talking to a shadow NPC: they stay as shadow, can chat but no deep bond
/// - ENERGY is spent to UNLOCK an NPC (reveal true form + add to contacts + can reappear)
/// - Some NPCs auto-unlock for free:
///   - NPCs who are attracted to you (based on your Charisma vs their personality)
///   - Family members
///   - NPCs assigned as important by the scene
/// - Unlocked NPCs can appear in FUTURE grid worlds (recurring characters)
/// - Unlocked NPCs are in your phone contacts
/// 
/// Energy per grid world (how many NPCs you can unlock):
/// - Childhood: 8
/// - Youth: 6
/// - Young: 5
/// - Prime: 4
/// - Middle: 3
/// - Elder: 2
/// + Career modifier (Teacher +3, Businessman -2, etc.)
/// </summary>
public class SocialSystem : MonoBehaviour
{
    public static SocialSystem Instance { get; private set; }

    [Header("Unlock Energy")]
    public int MaxUnlockEnergy = 5;
    public int CurrentUnlockEnergy;

    [Header("Data")]
    public Dictionary<string, Relationship> AllRelationships = new Dictionary<string, Relationship>();
    public List<string> UnlockedNpcIds = new List<string>(); // NPCs with revealed true form
    public List<string> RecurringNpcIds = new List<string>(); // NPCs that reappear across worlds

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    /// <summary>
    /// Enter world - set unlock energy based on age + career
    /// </summary>
    public void EnterWorld(int playerAge)
    {
        if (playerAge <= 12) MaxUnlockEnergy = 8;
        else if (playerAge <= 17) MaxUnlockEnergy = 6;
        else if (playerAge <= 30) MaxUnlockEnergy = 5;
        else if (playerAge <= 50) MaxUnlockEnergy = 4;
        else if (playerAge <= 65) MaxUnlockEnergy = 3;
        else MaxUnlockEnergy = 2;

        // Career modifier
        if (GameManager.Instance != null && GameManager.Instance.Player.Dream != null)
        {
            int mod = GameManager.Instance.Player.Dream.GetEnergyModifier();
            MaxUnlockEnergy = Mathf.Max(1, MaxUnlockEnergy + mod);
        }

        CurrentUnlockEnergy = MaxUnlockEnergy;
        Debug.Log($"[Social] World entered age {playerAge}, unlock energy: {MaxUnlockEnergy}");
    }

    /// <summary>
    /// Register NPC (called on spawn)
    /// </summary>
    public void RegisterNPC(NPCProfile profile)
    {
        if (!AllRelationships.ContainsKey(profile.Id))
        {
            AllRelationships[profile.Id] = new Relationship
            {
                NpcId = profile.Id, NpcName = profile.Name, Profile = profile
            };
        }
    }

    /// <summary>
    /// Chat with NPC - FREE, no energy cost
    /// Can chat with shadow NPCs, builds small closeness
    /// </summary>
    public string ChatWith(string npcId, SocialAction action)
    {
        if (!AllRelationships.ContainsKey(npcId)) return null;
        var rel = AllRelationships[npcId];
        string result = rel.Interact(action);

        // Career bonuses still apply
        ApplyCareerBonuses(rel, action);

        // Karma tracking
        TrackKarma(action, result);

        Debug.Log($"[Social] Chat {action} -> {rel.NpcName}: {rel.Stage} ({rel.Closeness})");
        return result;
    }

    /// <summary>
    /// Unlock NPC - costs 1 energy, reveals true form, adds to contacts
    /// Returns false if no energy left
    /// </summary>
    public bool UnlockNPC(string npcId)
    {
        if (CurrentUnlockEnergy <= 0)
        {
            Debug.Log("[Social] No unlock energy left");
            return false;
        }
        if (UnlockedNpcIds.Contains(npcId)) return true; // already unlocked

        CurrentUnlockEnergy--;
        UnlockedNpcIds.Add(npcId);

        // Add to phone contacts
        if (PhoneSystem.Instance != null && AllRelationships.ContainsKey(npcId))
        {
            var rel = AllRelationships[npcId];
            if (rel.Profile != null)
                PhoneSystem.Instance.AddContact(rel.Profile);
        }

        // Mark as recurring (can appear in future worlds)
        if (!RecurringNpcIds.Contains(npcId))
            RecurringNpcIds.Add(npcId);

        Debug.Log($"[Social] Unlocked NPC: {npcId}, energy left: {CurrentUnlockEnergy}");
        return true;
    }

    /// <summary>
    /// Check if NPC wants to approach player (auto-unlock, free)
    /// Based on: NPC extroversion, player charisma, NPC role
    /// </summary>
    public bool CheckNPCApproachesPlayer(NPCProfile npc, PlayerStats playerStats)
    {
        if (npc == null) return false;
        if (UnlockedNpcIds.Contains(npc.Id)) return false; // already unlocked

        // Family always approaches
        if (npc.Role == NPCRole.Family) return true;

        // Romantic interest: extroverted + player has charisma
        if (npc.Role == NPCRole.Romantic && npc.Introversion < 5)
            return true;

        // Extroverted NPCs with high kindness approach on their own
        if (npc.Introversion < 3 && npc.Kindness > 7)
            return true;

        // Player charisma attracts NPCs
        if (playerStats != null && playerStats.Charisma > 12 && npc.Introversion < 6)
        {
            // High charisma = more NPCs approach you
            return Random.value < 0.3f; // 30% chance
        }

        return false;
    }

    /// <summary>
    /// Auto-unlock NPC (free, no energy cost) - for NPCs who approach player
    /// </summary>
    public void AutoUnlockNPC(string npcId)
    {
        if (UnlockedNpcIds.Contains(npcId)) return;
        UnlockedNpcIds.Add(npcId);

        if (!RecurringNpcIds.Contains(npcId))
            RecurringNpcIds.Add(npcId);

        if (PhoneSystem.Instance != null && AllRelationships.ContainsKey(npcId))
        {
            var rel = AllRelationships[npcId];
            if (rel.Profile != null)
                PhoneSystem.Instance.AddContact(rel.Profile);
        }

        Debug.Log($"[Social] NPC auto-unlocked (approached player): {npcId}");
    }

    /// <summary>
    /// Get list of recurring NPCs that should appear in a new grid world
    /// Filters by: still alive, similar life trajectory, close relationship
    /// </summary>
    public List<string> GetRecurringNPCsForWorld(int playerAge)
    {
        var result = new List<string>();
        foreach (var id in RecurringNpcIds)
        {
            if (!AllRelationships.ContainsKey(id)) continue;
            var rel = AllRelationships[id];

            // Close friends/lovers always reappear
            if (rel.Closeness >= 30)
            {
                result.Add(id);
                continue;
            }

            // Acquaintances have a chance to reappear
            if (rel.Closeness >= 10 && Random.value < 0.4f)
            {
                result.Add(id);
            }
        }
        return result;
    }

    public bool IsUnlocked(string npcId) => UnlockedNpcIds.Contains(npcId);

    public Relationship GetRelationship(string npcId)
    {
        return AllRelationships.ContainsKey(npcId) ? AllRelationships[npcId] : null;
    }

    public List<Relationship> GetFriends() => AllRelationships.Values.Where(r => r.Closeness >= 20).ToList();
    public List<Relationship> GetCloseRelationships() => AllRelationships.Values.Where(r => r.Closeness >= 50).ToList();

    public string GetLifeSummary()
    {
        int unlocked = UnlockedNpcIds.Count;
        int friends = AllRelationships.Values.Count(r => r.Closeness >= 20);
        int close = AllRelationships.Values.Count(r => r.Closeness >= 50);
        int enemies = AllRelationships.Values.Count(r => r.Closeness <= -40);
        if (close >= 5) return $"A life rich in deep connections. {close} soulmates, {friends} friends out of {unlocked} people you truly knew.";
        if (friends >= 10) return $"Well-liked by many. {friends} friends across a lifetime.";
        if (unlocked <= 10) return $"A quiet life. You only truly knew {unlocked} people.";
        return $"{unlocked} people revealed their true selves to you. {friends} became friends.";
    }

    public List<string> GetSoulMemories()
    {
        var memories = new List<string>();
        foreach (var r in AllRelationships.Values)
        {
            if (r.Closeness >= 50) memories.Add($"A deep bond with {r.NpcName}");
            else if (r.Closeness <= -40) memories.Add($"An unresolved conflict with {r.NpcName}");
        }
        return memories;
    }

    void ApplyCareerBonuses(Relationship rel, SocialAction action)
    {
        if (GameManager.Instance == null) return;
        var dream = GameManager.Instance.Player.Dream;
        if (dream == null) return;

        if (dream.ActiveCareer == DreamCareer.Musician && action == SocialAction.Chat)
            rel.Closeness = Mathf.Clamp(rel.Closeness + 1, -100, 100);
        if (dream.ActiveCareer == DreamCareer.Chef && action == SocialAction.Gift)
            rel.Closeness = Mathf.Clamp(rel.Closeness + 3, -100, 100);
        if (dream.ActiveCareer == DreamCareer.Vet && rel.Profile != null && rel.Profile.Kindness > 5)
            rel.Closeness = Mathf.Clamp(rel.Closeness + 1, -100, 100);
    }

    void TrackKarma(SocialAction action, string result)
    {
        if (KarmaTracker.Instance == null) return;
        switch (action)
        {
            case SocialAction.Help: KarmaTracker.Instance.HelpedSomeone(result); break;
            case SocialAction.Betray: KarmaTracker.Instance.HarmedSomeone(result); break;
            case SocialAction.Ignore: KarmaTracker.Instance.IgnoredSomeone(result); break;
            case SocialAction.Gift: KarmaTracker.Instance.SelflessChoice(result); break;
            default: KarmaTracker.Instance.NeutralAction(result); break;
        }
    }

    public void ResetForNewLife()
    {
        AllRelationships.Clear();
        UnlockedNpcIds.Clear();
        RecurringNpcIds.Clear();
        CurrentUnlockEnergy = 0;
    }
}