using UnityEngine;

/// <summary>
/// Event trigger in open world
/// Integrates career-specific abilities:
/// - Doctor: can heal NPC health crises
/// - Firefighter: can save NPCs from death events
/// - Lawyer: can defend NPCs in crisis (better outcomes)
/// - Detective: can discover NPC secrets
/// - Journalist: can interview for hidden stories
/// - Writer: expanded dialogue options
/// </summary>
public class EventTrigger : MonoBehaviour, IInteractable
{
    [Header("Event Config")]
    public string EventId;
    public string EventTitle;
    [TextArea] public string EventDescription;
    public int GoldReward;
    public bool IsExitPoint;

    [Header("Death Risk")]
    public bool CanTriggerDeath;
    public float DeathChance = 0f;

    [Header("Career-Specific")]
    public bool IsHealthCrisis;     // Doctor can heal
    public bool IsDeathThreat;      // Firefighter can save
    public bool IsInjustice;        // Lawyer can defend
    public bool HasSecret;          // Detective/Journalist can discover
    public string SecretText;       // hidden info
    public int SecretGoldReward;

    [Header("Choices")]
    public EventChoice[] Choices;

    [Header("NPC Link")]
    public string LinkedNpcId;      // NPC this event is about

    public string InteractionPrompt => $"Press E: {EventTitle}";

    public void Interact()
    {
        Debug.Log($"[Event] {EventTitle}");
        var player = GameManager.Instance != null ? GameManager.Instance.Player : null;
        var career = player?.Dream?.ActiveCareer ?? DreamCareer.None;

        // === FIREFIGHTER: save from death ===
        if (CanTriggerDeath && IsDeathThreat && career == DreamCareer.Firefighter)
        {
            Debug.Log("[Event] Firefighter saves the day!");
            DeathChance = 0; // nullify death
            if (KarmaTracker.Instance != null)
                KarmaTracker.Instance.HelpedSomeone("Firefighter rescued someone from " + EventTitle);
            // Extra karma for firefighter
            if (player != null) player.KarmaValue += 3;
        }

        // === DOCTOR: heal health crisis ===
        if (IsHealthCrisis && career == DreamCareer.Doctor)
        {
            Debug.Log("[Event] Doctor heals the crisis!");
            CanTriggerDeath = false;
            if (KarmaTracker.Instance != null)
                KarmaTracker.Instance.HelpedSomeone("Doctor treated " + EventTitle);
            if (player != null) player.KarmaValue += 2;
            GoldReward += 50; // medical fee
        }

        // === DETECTIVE: discover secret ===
        if (HasSecret && (career == DreamCareer.Detective || career == DreamCareer.Journalist))
        {
            Debug.Log($"[Event] {career} discovered: {SecretText}");
            if (player != null) player.Gold += SecretGoldReward;
            if (KarmaTracker.Instance != null)
            {
                if (career == DreamCareer.Journalist && IsInjustice)
                    KarmaTracker.Instance.SelflessChoice("Exposed injustice: " + SecretText);
                else
                    KarmaTracker.Instance.NeutralAction("Discovered: " + SecretText);
            }
        }

        // === LAWYER: defend in crisis ===
        if (IsInjustice && career == DreamCareer.Lawyer)
        {
            Debug.Log("[Event] Lawyer defends!");
            GoldReward += 80;
            if (KarmaTracker.Instance != null)
                KarmaTracker.Instance.SelflessChoice("Defended justice in " + EventTitle);
        }

        // Death check
        if (CanTriggerDeath && Random.value < DeathChance)
        {
            int karmaTotal = 0;
            if (KarmaTracker.Instance != null) karmaTotal = KarmaTracker.Instance.SettleAndReset();
            var result = new GridWorldResult { GoldEarned = GoldReward, KarmaChange = karmaTotal, IsDead = true };
            GameManager.Instance.ExitGridWorld(result);
            return;
        }

        if (Choices != null && Choices.Length > 0)
        {
            MakeChoice(0); // TODO: show choice UI
        }

        if (IsExitPoint) ExitWorld();
    }

    public void MakeChoice(int index)
    {
        if (Choices == null || index >= Choices.Length) return;
        var choice = Choices[index];
        Debug.Log($"[Event] Choice: {choice.Text} -> {choice.NarrativeResult}");

        if (KarmaTracker.Instance != null)
        {
            switch (choice.ActionType)
            {
                case KarmaActionType.Help: KarmaTracker.Instance.HelpedSomeone(choice.NarrativeResult); break;
                case KarmaActionType.Harm: KarmaTracker.Instance.HarmedSomeone(choice.NarrativeResult); break;
                case KarmaActionType.Selfish: KarmaTracker.Instance.SelfishChoice(choice.NarrativeResult); break;
                case KarmaActionType.Selfless: KarmaTracker.Instance.SelflessChoice(choice.NarrativeResult); break;
                case KarmaActionType.Ignore: KarmaTracker.Instance.IgnoredSomeone(choice.NarrativeResult); break;
                default: KarmaTracker.Instance.NeutralAction(choice.NarrativeResult); break;
            }
        }

        // Social system integration
        if (!string.IsNullOrEmpty(LinkedNpcId))
        {
            var npc = GetComponentInParent<NPC>();
            if (npc != null) npc.OnDialogueChoice(choice.ActionType);
        }

        if (GameManager.Instance != null)
            GameManager.Instance.Player.Gold += choice.GoldChange;
    }

    void ExitWorld()
    {
        int karmaTotal = 0;
        if (KarmaTracker.Instance != null) karmaTotal = KarmaTracker.Instance.SettleAndReset();
        var result = new GridWorldResult { GoldEarned = GoldReward, KarmaChange = karmaTotal, IsDead = false };
        GameManager.Instance.ExitGridWorld(result);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            string extra = "";
            var career = GameManager.Instance?.Player?.Dream?.ActiveCareer ?? DreamCareer.None;
            if (IsHealthCrisis && career == DreamCareer.Doctor) extra = " [Doctor: Can Heal]";
            if (IsDeathThreat && career == DreamCareer.Firefighter) extra = " [Firefighter: Can Save]";
            if (HasSecret && (career == DreamCareer.Detective || career == DreamCareer.Journalist)) extra = " [Can Investigate]";
            if (IsInjustice && career == DreamCareer.Lawyer) extra = " [Lawyer: Can Defend]";
            Debug.Log($"[Event] Near: {InteractionPrompt}{extra}");
        }
    }
}

[System.Serializable]
public class EventChoice
{
    public string Text;
    public string NarrativeResult;
    public KarmaActionType ActionType;
    public int GoldChange;
}

public enum KarmaActionType
{
    Neutral, Help, Harm, Selfish, Selfless, Ignore
}