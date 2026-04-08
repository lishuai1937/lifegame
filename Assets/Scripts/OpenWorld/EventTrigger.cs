using UnityEngine;

/// <summary>
/// Event trigger in open world
/// Player actions here feed into KarmaTracker (hidden karma)
/// Player only sees narrative text, never karma numbers
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

    [Header("Choices (optional)")]
    public EventChoice[] Choices;

    public string InteractionPrompt => $"Press E: {EventTitle}";

    public void Interact()
    {
        Debug.Log($"[EventTrigger] {EventTitle}");

        // Death check
        if (CanTriggerDeath && Random.value < DeathChance)
        {
            // Settle karma before death
            int karmaTotal = 0;
            if (KarmaTracker.Instance != null)
                karmaTotal = KarmaTracker.Instance.SettleAndReset();

            var result = new GridWorldResult
            {
                GoldEarned = GoldReward,
                KarmaChange = karmaTotal,
                IsDead = true
            };
            GameManager.Instance.ExitGridWorld(result);
            return;
        }

        if (Choices != null && Choices.Length > 0)
        {
            // TODO: Show choice UI, for now auto-pick first choice
            MakeChoice(0);
        }

        // Exit point
        if (IsExitPoint)
        {
            ExitWorld();
        }
    }

    /// <summary>
    /// Player makes a choice - karma is tracked but NOT shown
    /// </summary>
    public void MakeChoice(int index)
    {
        if (Choices == null || index >= Choices.Length) return;
        var choice = Choices[index];

        Debug.Log($"[EventTrigger] Choice: {choice.Text} -> {choice.NarrativeResult}");

        // Log to karma tracker - player only sees the narrative, not the karma value
        if (KarmaTracker.Instance != null)
        {
            switch (choice.ActionType)
            {
                case KarmaActionType.Help:
                    KarmaTracker.Instance.HelpedSomeone(choice.NarrativeResult);
                    break;
                case KarmaActionType.Harm:
                    KarmaTracker.Instance.HarmedSomeone(choice.NarrativeResult);
                    break;
                case KarmaActionType.Selfish:
                    KarmaTracker.Instance.SelfishChoice(choice.NarrativeResult);
                    break;
                case KarmaActionType.Selfless:
                    KarmaTracker.Instance.SelflessChoice(choice.NarrativeResult);
                    break;
                case KarmaActionType.Ignore:
                    KarmaTracker.Instance.IgnoredSomeone(choice.NarrativeResult);
                    break;
                default:
                    KarmaTracker.Instance.NeutralAction(choice.NarrativeResult);
                    break;
            }
        }

        // Gold is visible
        if (GameManager.Instance != null)
            GameManager.Instance.Player.Gold += choice.GoldChange;
    }

    void ExitWorld()
    {
        int karmaTotal = 0;
        if (KarmaTracker.Instance != null)
            karmaTotal = KarmaTracker.Instance.SettleAndReset();

        var result = new GridWorldResult
        {
            GoldEarned = GoldReward,
            KarmaChange = karmaTotal,
            IsDead = false
        };
        GameManager.Instance.ExitGridWorld(result);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            Debug.Log($"[EventTrigger] Near: {InteractionPrompt}");
    }
}

/// <summary>
/// A choice the player can make during an event
/// ActionType determines hidden karma effect
/// Player only sees Text and NarrativeResult
/// </summary>
[System.Serializable]
public class EventChoice
{
    public string Text;                     // What the player sees as option
    public string NarrativeResult;          // What happens (shown to player)
    public KarmaActionType ActionType;      // Hidden: determines karma direction
    public int GoldChange;                  // Gold is visible to player
}

public enum KarmaActionType
{
    Neutral,    // tiny random karma
    Help,       // positive karma
    Harm,       // negative karma
    Selfish,    // mild negative
    Selfless,   // mild positive
    Ignore      // mild negative (inaction)
}