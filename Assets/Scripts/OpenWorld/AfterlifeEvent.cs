using UnityEngine;

/// <summary>
/// Afterlife event trigger - works in Heaven or Hell worlds
/// Similar to EventTrigger but with afterlife-specific behavior
/// 
/// Heaven events: peaceful tasks, helping spirits, gaining wisdom
/// Hell events: trials, redemption tasks, endurance challenges
/// </summary>
public class AfterlifeEvent : MonoBehaviour, IInteractable
{
    [Header("Event")]
    public string EventTitle;
    [TextArea] public string EventDescription;
    public WorldRealm Realm; // which afterlife this belongs to

    [Header("Choices")]
    public AfterlifeChoice[] Choices;

    [Header("Settings")]
    public bool IsExitPoint = false; // triggers reincarnation
    public bool OneTimeOnly = true;
    private bool hasTriggered = false;

    public string InteractionPrompt => $"Press E: {EventTitle}";

    public void Interact()
    {
        if (OneTimeOnly && hasTriggered) return;
        hasTriggered = true;

        Debug.Log($"[AfterlifeEvent] {EventTitle}");

        if (Choices != null && Choices.Length > 0)
        {
            // TODO: Show choice UI, for now auto-pick first
            MakeChoice(0);
        }

        if (IsExitPoint && AfterlifeManager.Instance != null)
        {
            AfterlifeManager.Instance.PromptReincarnation();
        }
    }

    public void MakeChoice(int index)
    {
        if (Choices == null || index >= Choices.Length) return;
        var choice = Choices[index];

        Debug.Log($"[AfterlifeEvent] Choice: {choice.Text} -> {choice.Result}");

        // Afterlife karma bonus (can improve next life)
        if (AfterlifeManager.Instance != null)
            AfterlifeManager.Instance.AddAfterlifeKarma(choice.KarmaBonus);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            Debug.Log($"[AfterlifeEvent] Near: {InteractionPrompt}");
    }
}

[System.Serializable]
public class AfterlifeChoice
{
    public string Text;         // what player sees
    public string Result;       // narrative result
    public int KarmaBonus;      // bonus karma for next life
}