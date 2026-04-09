using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// NPC Event Manager - checks NPC life events when player advances age
/// Close NPCs invite the player to their milestones
/// Player can attend (costs energy, affects relationship) or skip
/// </summary>
public class NPCEventManager : MonoBehaviour
{
    public static NPCEventManager Instance { get; private set; }

    // All NPC lifelines tracked
    public Dictionary<string, NPCLifeline> AllLifelines = new Dictionary<string, NPCLifeline>();

    // Minimum closeness to receive invitations
    public int InviteThreshold = 20; // Friend level

    // Events pending player response
    public List<PendingInvite> PendingInvites = new List<PendingInvite>();

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    /// <summary>
    /// Register an NPC's lifeline (called when NPC is first met)
    /// </summary>
    public void RegisterLifeline(NPCProfile profile, int playerCurrentAge)
    {
        if (AllLifelines.ContainsKey(profile.Id)) return;
        var lifeline = NPCLifeline.Generate(profile, playerCurrentAge);
        AllLifelines[profile.Id] = lifeline;
    }

    /// <summary>
    /// Called when player advances to a new age
    /// Checks all tracked NPCs for events at this age
    /// Returns list of invitations from close NPCs
    /// </summary>
    public List<PendingInvite> CheckEventsAtAge(int playerAge)
    {
        PendingInvites.Clear();

        foreach (var kvp in AllLifelines)
        {
            var lifeline = kvp.Value;
            if (!lifeline.IsAlive) continue;

            // Calculate NPC's current age based on player age
            int npcAge = playerAge - lifeline.BirthYear + lifeline.CurrentAge;
            lifeline.CurrentAge = npcAge;

            // Check if NPC died
            if (npcAge >= lifeline.DeathAge)
            {
                lifeline.IsAlive = false;
            }

            // Get events at this NPC age
            var events = lifeline.GetEventsAtAge(npcAge);
            foreach (var evt in events)
            {
                if (!evt.CanInvitePlayer) continue;

                // Check closeness
                if (SocialSystem.Instance != null)
                {
                    var rel = SocialSystem.Instance.GetRelationship(kvp.Key);
                    if (rel != null && rel.Closeness >= InviteThreshold)
                    {
                        PendingInvites.Add(new PendingInvite
                        {
                            NpcId = kvp.Key,
                            NpcName = lifeline.NpcName,
                            Event = evt,
                            Closeness = rel.Closeness
                        });
                    }
                }
            }
        }

        // Sort by closeness (closest friends first)
        PendingInvites.Sort((a, b) => b.Closeness.CompareTo(a.Closeness));

        if (PendingInvites.Count > 0)
            Debug.Log($"[NPCEvents] {PendingInvites.Count} invitations at age {playerAge}");

        return PendingInvites;
    }

    /// <summary>
    /// Player attends an NPC event
    /// </summary>
    public string AttendEvent(PendingInvite invite)
    {
        invite.Event.PlayerAttended = true;
        string result = "";

        // Attending boosts relationship
        if (SocialSystem.Instance != null)
        {
            SocialSystem.Instance.InteractWith(invite.NpcId, SocialAction.Help);
        }

        switch (invite.Event.Type)
        {
            case NPCEventType.Birthday:
                result = $"You attended {invite.NpcName}'s birthday. They were really happy to see you.";
                break;
            case NPCEventType.Marriage:
                result = $"You attended {invite.NpcName}'s wedding. A beautiful ceremony.";
                // Player meets spouse
                var lifeline = AllLifelines[invite.NpcId];
                if (!string.IsNullOrEmpty(lifeline.SpouseId))
                    result += $" You met their partner.";
                break;
            case NPCEventType.ChildBorn:
                result = $"{invite.NpcName} had a baby! You visited them at the hospital.";
                break;
            case NPCEventType.Death:
                result = $"You attended {invite.NpcName}'s funeral. You'll miss them.";
                if (KarmaTracker.Instance != null)
                    KarmaTracker.Instance.SelflessChoice("Attended funeral of " + invite.NpcName);
                break;
            case NPCEventType.FinancialTrouble:
                result = $"{invite.NpcName} is in financial trouble and asked for your help.";
                break;
            case NPCEventType.HealthCrisis:
                result = $"{invite.NpcName} is seriously ill. You visited them.";
                break;
            case NPCEventType.LifeDilemma:
                result = $"{invite.NpcName} is facing a tough decision and wants your advice.";
                break;
            default:
                result = $"You spent time with {invite.NpcName}.";
                break;
        }

        Debug.Log($"[NPCEvents] Attended: {invite.NpcName}'s {invite.Event.Type}");
        return result;
    }

    /// <summary>
    /// Player helps NPC with their crisis (costs gold)
    /// </summary>
    public string HelpWithCrisis(PendingInvite invite, int goldAmount)
    {
        if (GameManager.Instance == null) return "";
        var player = GameManager.Instance.Player;

        if (player.Gold < goldAmount)
            return "You don't have enough gold to help.";

        player.Gold -= goldAmount;

        // Big relationship boost
        if (SocialSystem.Instance != null)
        {
            SocialSystem.Instance.InteractWith(invite.NpcId, SocialAction.Help);
            SocialSystem.Instance.InteractWith(invite.NpcId, SocialAction.Gift); // double boost
        }

        if (KarmaTracker.Instance != null)
            KarmaTracker.Instance.HelpedSomeone("Helped " + invite.NpcName + " with " + invite.Event.Title);

        return $"You gave {goldAmount} gold to help {invite.NpcName}. They were deeply grateful.";
    }

    /// <summary>
    /// Player skips an NPC event
    /// </summary>
    public string SkipEvent(PendingInvite invite)
    {
        invite.Event.PlayerAttended = false;

        // Relationship takes a hit (bigger hit for important events)
        if (SocialSystem.Instance != null)
        {
            SocialSystem.Instance.InteractWith(invite.NpcId, SocialAction.Ignore);
        }

        switch (invite.Event.Type)
        {
            case NPCEventType.Marriage:
                return $"You didn't attend {invite.NpcName}'s wedding. They noticed.";
            case NPCEventType.Death:
                return $"You didn't attend {invite.NpcName}'s funeral...";
            case NPCEventType.FinancialTrouble:
                return $"{invite.NpcName} struggled alone with their financial problems.";
            default:
                return $"You were too busy for {invite.NpcName}.";
        }
    }

    /// <summary>
    /// Get NPC's family members as new NPCProfiles
    /// Called when player meets an NPC's family
    /// </summary>
    public NPCProfile GetOrCreateFamilyMember(string npcId, string relation)
    {
        if (!AllLifelines.ContainsKey(npcId)) return null;
        var lifeline = AllLifelines[npcId];

        string familyId = npcId + "_" + relation;
        string familyName = lifeline.NpcName + "'s " + relation;

        NPCRole role = relation == "spouse" ? NPCRole.Family :
                       relation == "child" ? NPCRole.Child :
                       relation == "father" || relation == "mother" ? NPCRole.Family :
                       NPCRole.Stranger;

        return NPCProfile.GenerateRandom(familyName, role, lifeline.CurrentAge);
    }

    /// <summary>
    /// Reset for reincarnation
    /// </summary>
    public void ResetForNewLife()
    {
        AllLifelines.Clear();
        PendingInvites.Clear();
    }
}

[System.Serializable]
public class PendingInvite
{
    public string NpcId;
    public string NpcName;
    public NPCLifeEvent Event;
    public int Closeness;
}