using UnityEngine;

/// <summary>
/// NPC Influence System - NPCs can manipulate the player, and vice versa
/// 
/// DESIGN:
/// - Some NPCs are "manipulators" (high ambition + low kindness)
/// - They try to control the player: make you spend money, change plans, isolate you
/// - Player's Willpower determines if they resist
/// - If player has high Charisma, they can influence certain NPCs instead
/// - This creates a push-pull dynamic that feels like real social pressure
/// 
/// Being manipulated and breaking free = Willpower bonus + achievement
/// Successfully influencing NPCs = Charisma bonus but karma cost
/// </summary>
public class NPCInfluenceSystem : MonoBehaviour
{
    public static NPCInfluenceSystem Instance { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    /// <summary>
    /// Check if an NPC tries to manipulate the player
    /// Called during NPC interaction
    /// </summary>
    public ManipulationResult CheckNPCManipulation(NPCProfile npc, PlayerStats stats, Relationship rel)
    {
        if (npc == null || stats == null) return null;

        // Only manipulative NPCs try this (high ambition + low kindness + high temper)
        bool isManipulator = npc.Ambition > 7 && npc.Kindness < 4;
        if (!isManipulator) return null;

        // Only works if player has some relationship with them
        if (rel == null || rel.Closeness < 5) return null;

        // Manipulation attempt
        int manipPower = npc.Ambition + npc.Temper - npc.Kindness; // 0-20ish
        int resistance = stats.Willpower + stats.Empathy / 2;       // 0-30ish

        if (resistance >= manipPower)
        {
            // Player resists
            stats.Modify(StatType.Willpower, 1);
            return new ManipulationResult
            {
                WasManipulated = false,
                NpcName = npc.Name,
                Description = $"{npc.Name} tried to pressure you into something. You saw through it and said no.",
                WillpowerGain = 1
            };
        }
        else
        {
            // Player gets manipulated
            int goldLoss = Random.Range(20, 100);
            return new ManipulationResult
            {
                WasManipulated = true,
                NpcName = npc.Name,
                Description = $"{npc.Name} convinced you to do something you didn't want to. You spent {goldLoss} gold and feel uneasy about it.",
                GoldLoss = goldLoss,
                EmotionalImpact = "Why did you agree to that?"
            };
        }
    }

    /// <summary>
    /// Player tries to influence/persuade an NPC
    /// Requires high Charisma
    /// </summary>
    public InfluenceResult TryInfluenceNPC(NPCProfile npc, PlayerStats stats, Relationship rel, string request)
    {
        if (npc == null || stats == null) return new InfluenceResult(false, "Can't influence this person.");

        int charismaPower = stats.Charisma;
        int npcResistance = npc.Ambition + npc.Introversion / 2;

        // Closeness helps
        int closenessBonus = rel != null ? rel.Closeness / 10 : 0;
        int totalPower = charismaPower + closenessBonus;

        if (totalPower >= npcResistance)
        {
            // Success
            stats.Modify(StatType.Charisma, 1);

            // But using influence on others has a karma cost
            if (KarmaTracker.Instance != null)
                KarmaTracker.Instance.SelfishChoice("Persuaded " + npc.Name + " to " + request);

            return new InfluenceResult(true,
                $"{npc.Name} agrees to your request. They seem a bit reluctant but go along with it.")
            {
                CharismaGain = 1
            };
        }
        else
        {
            // Fail
            if (rel != null) rel.Closeness -= 3; // they don't like being pushed

            return new InfluenceResult(false,
                $"{npc.Name} refuses. \"Don't try to push me around.\"")
            {
                ClosenessLoss = 3
            };
        }
    }

    /// <summary>
    /// Check if player is being "led by the nose" by an NPC
    /// Happens when closeness is very high but NPC has manipulative traits
    /// Player might not even realize it
    /// </summary>
    public string CheckPassiveInfluence(NPCProfile npc, Relationship rel, PlayerStats stats)
    {
        if (npc == null || rel == null || stats == null) return null;

        // Only if very close AND NPC is manipulative
        if (rel.Closeness < 60) return null;
        if (npc.Ambition <= 5 || npc.Kindness >= 5) return null;

        // High empathy players notice
        if (stats.Empathy > 12)
        {
            stats.Modify(StatType.Willpower, 1);
            return $"You notice that {npc.Name} has been subtly steering your decisions. Something feels off.";
        }

        // Otherwise player doesn't notice, just loses gold/energy
        return null; // silent manipulation, player doesn't see it
    }
}

[System.Serializable]
public class ManipulationResult
{
    public bool WasManipulated;
    public string NpcName;
    public string Description;
    public string EmotionalImpact;
    public int GoldLoss;
    public int WillpowerGain;
}

public class InfluenceResult
{
    public bool Success;
    public string Message;
    public int CharismaGain;
    public int ClosenessLoss;
    public InfluenceResult(bool s, string m) { Success = s; Message = m; }
}