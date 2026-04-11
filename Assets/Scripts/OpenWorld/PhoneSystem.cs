using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Phone System - In-game phone for contacting known NPCs
/// 
/// DESIGN:
/// - Available after age 12 (gets first phone)
/// - Contact list = all NPCs player has spoken to (revealed)
/// - Actions: Call, Text, Borrow Money, Lend Money, Invite to Event
/// - Each action costs 1 social energy (except texting costs 0)
/// - Can be used from board layer OR open world
/// - NPC response depends on closeness + personality
/// - Phone unlocks at 12, smartphone upgrade at 18 (more features)
/// </summary>
public class PhoneSystem : MonoBehaviour
{
    public static PhoneSystem Instance { get; private set; }

    public bool IsUnlocked = false;         // unlocked at age 12
    public bool HasSmartphone = false;      // upgraded at age 18
    public List<PhoneContact> Contacts = new List<PhoneContact>();

    // Cooldown: can't spam same NPC
    private Dictionary<string, float> lastContactTime = new Dictionary<string, float>();
    public float ContactCooldown = 30f;     // seconds between contacting same NPC

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    /// <summary>
    /// Check and unlock phone based on age
    /// </summary>
    public void CheckUnlock(int age)
    {
        if (age >= 12 && !IsUnlocked)
        {
            IsUnlocked = true;
            Debug.Log("[Phone] Phone unlocked!");
        }
        if (age >= 18 && !HasSmartphone)
        {
            HasSmartphone = true;
            Debug.Log("[Phone] Smartphone upgrade!");
        }
    }

    /// <summary>
    /// Add NPC to contacts (called when player first talks to an NPC)
    /// </summary>
    public void AddContact(NPCProfile profile)
    {
        if (!IsUnlocked) return;
        if (Contacts.Any(c => c.NpcId == profile.Id)) return;

        Contacts.Add(new PhoneContact
        {
            NpcId = profile.Id,
            NpcName = profile.Name,
            Role = profile.Role,
            AddedAtAge = GameManager.Instance != null ? GameManager.Instance.Player.CurrentAge : 0
        });
        Debug.Log($"[Phone] New contact: {profile.Name}");
    }

    /// <summary>
    /// Call an NPC - costs 1 energy, boosts closeness
    /// </summary>
    public PhoneResult Call(string npcId)
    {
        if (!IsUnlocked) return new PhoneResult(false, "You don't have a phone yet.");
        if (!CanContact(npcId)) return new PhoneResult(false, "You called them recently. Give it some time.");

        var rel = SocialSystem.Instance?.GetRelationship(npcId);
        if (rel == null) return new PhoneResult(false, "Unknown contact.");

        // Costs energy
        if (SocialSystem.Instance.CurrentUnlockEnergy <= 0)
            return new PhoneResult(false, "You're too tired to call anyone.");
        SocialSystem.Instance.CurrentUnlockEnergy--;

        MarkContacted(npcId);

        // Response based on closeness + personality
        string response;
        int closenessGain;

        if (rel.Closeness >= 50)
        {
            response = $"{rel.NpcName} picks up immediately. \"Hey! So good to hear from you!\"";
            closenessGain = 3;
        }
        else if (rel.Closeness >= 20)
        {
            response = $"{rel.NpcName} answers. \"Oh hi! What's up?\"";
            closenessGain = 2;
        }
        else if (rel.Closeness >= 0)
        {
            response = $"{rel.NpcName} answers after a few rings. \"Hello?\"";
            closenessGain = 1;
        }
        else
        {
            response = $"{rel.NpcName} doesn't pick up.";
            closenessGain = 0;
        }

        // Introverts less likely to enjoy calls
        if (rel.Profile != null && rel.Profile.Introversion > 7)
            closenessGain = Mathf.Max(0, closenessGain - 1);

        rel.Closeness = Mathf.Clamp(rel.Closeness + closenessGain, -100, 100);
        return new PhoneResult(true, response);
    }

    /// <summary>
    /// Text an NPC - free (no energy cost), tiny closeness boost
    /// Only available with smartphone (age 18+)
    /// </summary>
    public PhoneResult TextMessage(string npcId, string message)
    {
        if (!HasSmartphone) return new PhoneResult(false, "You need a smartphone to text.");
        if (!CanContact(npcId)) return new PhoneResult(false, "You just texted them.");

        var rel = SocialSystem.Instance?.GetRelationship(npcId);
        if (rel == null) return new PhoneResult(false, "Unknown contact.");

        MarkContacted(npcId);

        string response;
        if (rel.Closeness >= 30)
        {
            response = $"{rel.NpcName} replies quickly: \"\ud83d\ude0a\"";
            rel.Closeness = Mathf.Clamp(rel.Closeness + 1, -100, 100);
        }
        else if (rel.Closeness >= 0)
        {
            response = $"{rel.NpcName} replies: \"OK\"";
        }
        else
        {
            response = $"{rel.NpcName} left you on read.";
        }

        return new PhoneResult(true, response);
    }

    /// <summary>
    /// Borrow money from NPC - depends on closeness and their wealth
    /// </summary>
    public PhoneResult BorrowMoney(string npcId, int amount)
    {
        if (!IsUnlocked) return new PhoneResult(false, "No phone.");

        var rel = SocialSystem.Instance?.GetRelationship(npcId);
        if (rel == null) return new PhoneResult(false, "Unknown contact.");

        // Need at least Friend level
        if (rel.Closeness < 20)
            return new PhoneResult(false, $"{rel.NpcName} says: \"We're not that close...\"");

        // Check NPC personality
        bool agrees = true;
        string response;

        if (rel.Profile != null)
        {
            // Generous (kind) NPCs lend easier
            if (rel.Profile.Kindness < 4 && amount > 100)
            {
                agrees = false;
                response = $"{rel.NpcName} says: \"Sorry, I can't afford that.\"";
            }
            else if (rel.Closeness < 50 && amount > 200)
            {
                agrees = false;
                response = $"{rel.NpcName} says: \"That's a lot... I don't think I can.\"";
            }
            else
            {
                response = $"{rel.NpcName} says: \"Sure, I'll transfer it to you.\"";
            }
        }
        else
        {
            response = $"{rel.NpcName} agrees to lend you {amount} gold.";
        }

        if (agrees)
        {
            if (GameManager.Instance != null)
                GameManager.Instance.Player.Gold += amount;
            // Borrowing slightly reduces closeness (debt tension)
            rel.Closeness = Mathf.Clamp(rel.Closeness - 2, -100, 100);
            return new PhoneResult(true, response);
        }

        return new PhoneResult(false, response);
    }

    /// <summary>
    /// Lend money to NPC - boosts closeness significantly
    /// </summary>
    public PhoneResult LendMoney(string npcId, int amount)
    {
        if (!IsUnlocked) return new PhoneResult(false, "No phone.");
        if (GameManager.Instance == null) return new PhoneResult(false, "Error.");

        var player = GameManager.Instance.Player;
        if (player.Gold < amount)
            return new PhoneResult(false, "You don't have enough gold.");

        var rel = SocialSystem.Instance?.GetRelationship(npcId);
        if (rel == null) return new PhoneResult(false, "Unknown contact.");

        player.Gold -= amount;
        int boost = amount >= 200 ? 8 : amount >= 100 ? 5 : 3;
        rel.Closeness = Mathf.Clamp(rel.Closeness + boost, -100, 100);

        if (KarmaTracker.Instance != null)
            KarmaTracker.Instance.SelflessChoice("Lent money to " + rel.NpcName);

        return new PhoneResult(true, $"You sent {amount} gold to {rel.NpcName}. They're very grateful.");
    }

    /// <summary>
    /// Invite NPC to your event (wedding, birthday, etc.)
    /// </summary>
    public PhoneResult InviteToEvent(string npcId, string eventName)
    {
        if (!IsUnlocked) return new PhoneResult(false, "No phone.");

        var rel = SocialSystem.Instance?.GetRelationship(npcId);
        if (rel == null) return new PhoneResult(false, "Unknown contact.");

        string response;
        bool accepts;

        if (rel.Closeness >= 50)
        {
            accepts = true;
            response = $"{rel.NpcName}: \"Of course I'll be there! Wouldn't miss it!\"";
            rel.Closeness = Mathf.Clamp(rel.Closeness + 2, -100, 100);
        }
        else if (rel.Closeness >= 20)
        {
            accepts = Random.value > 0.2f; // 80% chance
            response = accepts
                ? $"{rel.NpcName}: \"Sure, I'll try to make it!\""
                : $"{rel.NpcName}: \"Sorry, I have plans that day...\"";
        }
        else if (rel.Closeness >= 0)
        {
            accepts = Random.value > 0.6f; // 40% chance
            response = accepts
                ? $"{rel.NpcName}: \"Oh, okay. I'll come.\""
                : $"{rel.NpcName}: \"I'm busy, sorry.\"";
        }
        else
        {
            accepts = false;
            response = $"{rel.NpcName} declined.";
        }

        // Introverts less likely to attend
        if (accepts && rel.Profile != null && rel.Profile.Introversion > 8)
        {
            if (Random.value > 0.5f)
            {
                accepts = false;
                response = $"{rel.NpcName}: \"I'm not really a party person... sorry.\"";
            }
        }

        return new PhoneResult(accepts, response);
    }

    /// <summary>
    /// Get sorted contact list (closest first)
    /// </summary>
    public List<PhoneContact> GetContactList()
    {
        // Sort by closeness
        var sorted = new List<PhoneContact>(Contacts);
        sorted.Sort((a, b) =>
        {
            var ra = SocialSystem.Instance?.GetRelationship(a.NpcId);
            var rb = SocialSystem.Instance?.GetRelationship(b.NpcId);
            int ca = ra != null ? ra.Closeness : 0;
            int cb = rb != null ? rb.Closeness : 0;
            return cb.CompareTo(ca);
        });
        return sorted;
    }

    bool CanContact(string npcId)
    {
        if (!lastContactTime.ContainsKey(npcId)) return true;
        return Time.time - lastContactTime[npcId] >= ContactCooldown;
    }

    void MarkContacted(string npcId)
    {
        lastContactTime[npcId] = Time.time;
    }

    public void ResetForNewLife()
    {
        IsUnlocked = false;
        HasSmartphone = false;
        Contacts.Clear();
        lastContactTime.Clear();
    }
}

[System.Serializable]
public class PhoneContact
{
    public string NpcId;
    public string NpcName;
    public NPCRole Role;
    public int AddedAtAge;
}

public class PhoneResult
{
    public bool Success;
    public string Message;
    public PhoneResult(bool s, string m) { Success = s; Message = m; }
}