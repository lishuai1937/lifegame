using UnityEngine;

/// <summary>
/// NPC v3 - New social model:
/// - Talk to any NPC for free (they stay as shadow)
/// - Spend energy to UNLOCK (reveal true form + contacts + recurring)
/// - Some NPCs approach you and auto-unlock for free
/// </summary>
public class NPC : MonoBehaviour, IInteractable
{
    [Header("NPC Info")]
    public string NpcName = "Stranger";
    [TextArea] public string GreetingText = "...";
    public NPCRole Role = NPCRole.Stranger;

    [Header("Dialogue")]
    public TextAsset DialogueJson;

    [HideInInspector] public NPCProfile Profile;
    private DialogueTree dialogueTree;
    private NPCReveal reveal;
    private bool hasSpoken = false;
    private bool isUnlocked = false;
    private bool hasApproached = false;

    public string InteractionPrompt
    {
        get
        {
            if (isUnlocked)
            {
                var rel = SocialSystem.Instance?.GetRelationship(Profile?.Id);
                if (rel != null && rel.Stage != RelationStage.Stranger)
                    return $"Press E: {NpcName} ({rel.Stage})";
                return $"Press E: {NpcName}";
            }
            if (hasSpoken)
                return "Press E: ??? [Press F to Unlock]";
            return "Press E: ???";
        }
    }

    void Start()
    {
        reveal = GetComponent<NPCReveal>();

        if (Profile == null)
        {
            int age = GameManager.Instance != null ? GameManager.Instance.Player.CurrentAge : 20;
            Profile = NPCProfile.GenerateRandom(NpcName, Role, age);
        }

        if (SocialSystem.Instance != null)
            SocialSystem.Instance.RegisterNPC(Profile);

        // Check if already unlocked from previous world
        if (SocialSystem.Instance != null && SocialSystem.Instance.IsUnlocked(Profile.Id))
        {
            isUnlocked = true;
            if (reveal != null) reveal.IsImportantNPC = true; // show true form
        }

        // Check if NPC wants to approach player (auto-unlock)
        if (!isUnlocked && SocialSystem.Instance != null)
        {
            var stats = GameManager.Instance?.Player?.Stats;
            if (SocialSystem.Instance.CheckNPCApproachesPlayer(Profile, stats))
            {
                hasApproached = true;
                // Will auto-unlock on first interaction
            }
        }

        // Load dialogue
        if (DialogueJson != null)
            dialogueTree = JsonUtility.FromJson<DialogueTree>(DialogueJson.text);
        if (dialogueTree == null)
        {
            string sceneId = "";
            int age = GameManager.Instance != null ? GameManager.Instance.Player.CurrentAge : 20;
            dialogueTree = DialogueGenerator.Generate(Profile, sceneId, age);
        }
    }

    void Update()
    {
        // F key to unlock NPC (spend energy to reveal true form)
        if (hasSpoken && !isUnlocked && Input.GetKeyDown(KeyCode.F))
        {
            TryUnlock();
        }
    }

    public void Interact()
    {
        hasSpoken = true;

        // If NPC approached player, auto-unlock for free
        if (hasApproached && !isUnlocked)
        {
            AutoUnlock();
        }

        // Chat is always free
        if (SocialSystem.Instance != null && Profile != null)
        {
            SocialSystem.Instance.ChatWith(Profile.Id, SocialAction.Chat);
        }

        // Start dialogue
        if (DialogueSystem.Instance != null && !DialogueSystem.Instance.IsActive)
            DialogueSystem.Instance.StartDialogue(dialogueTree);
    }

    /// <summary>
    /// Player spends energy to unlock this NPC
    /// </summary>
    void TryUnlock()
    {
        if (SocialSystem.Instance == null || Profile == null) return;

        bool success = SocialSystem.Instance.UnlockNPC(Profile.Id);
        if (success)
        {
            isUnlocked = true;
            if (reveal != null && !reveal.IsRevealed)
                reveal.Reveal();
            Debug.Log($"[NPC] Unlocked: {NpcName}");
        }
        else
        {
            Debug.Log($"[NPC] Can't unlock {NpcName} - no energy");
        }
    }

    /// <summary>
    /// NPC approached player - free unlock + higher initial closeness + approach dialogue
    /// </summary>
    void AutoUnlock()
    {
        if (SocialSystem.Instance == null || Profile == null) return;

        SocialSystem.Instance.AutoUnlockNPC(Profile.Id);
        isUnlocked = true;
        if (reveal != null && !reveal.IsRevealed)
            reveal.Reveal();

        // NPCs who approach you start with higher closeness (they already like you)
        var rel = SocialSystem.Instance.GetRelationship(Profile.Id);
        if (rel != null)
        {
            int bonus = 10; // base approach bonus
            if (Profile.Role == NPCRole.Romantic) bonus = 20;
            if (Profile.Role == NPCRole.Family) bonus = 30;
            if (Profile.Kindness > 7) bonus += 5;
            rel.Closeness = Mathf.Clamp(rel.Closeness + bonus, -100, 100);
        }

        // Switch to approach-specific dialogue (warmer, more initiative)
        string sceneId = "";
        int age = GameManager.Instance != null ? GameManager.Instance.Player.CurrentAge : 20;
        dialogueTree = DialogueGenerator.GenerateApproach(Profile, sceneId, age);

        Debug.Log($"[NPC] {NpcName} approached you! Closeness boosted, approach dialogue loaded.");
    }

    public void OnDialogueChoice(KarmaActionType actionType)
    {
        if (SocialSystem.Instance == null || Profile == null) return;
        SocialAction socialAction = actionType switch
        {
            KarmaActionType.Help => SocialAction.Help,
            KarmaActionType.Harm => SocialAction.Argue,
            KarmaActionType.Selfish => SocialAction.Ignore,
            KarmaActionType.Selfless => SocialAction.Gift,
            KarmaActionType.Ignore => SocialAction.Ignore,
            _ => SocialAction.Chat
        };
        SocialSystem.Instance.ChatWith(Profile.Id, socialAction);
    }

    public void SetDialogue(DialogueTree tree) { dialogueTree = tree; }
    public void SetProfile(NPCProfile profile) { Profile = profile; }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (hasApproached && !isUnlocked)
                Debug.Log($"[NPC] {NpcName} walks up to you: \"{GreetingText}\"");
            else
                Debug.Log($"[NPC] {InteractionPrompt}");
        }
    }
}