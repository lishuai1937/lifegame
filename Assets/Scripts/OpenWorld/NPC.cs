using UnityEngine;

/// <summary>
/// NPC that can be interacted with to start dialogue
/// On first interaction, triggers NPCReveal (shadow -> true form)
/// </summary>
public class NPC : MonoBehaviour, IInteractable
{
    [Header("NPC Info")]
    public string NpcName = "Stranger";
    [TextArea] public string GreetingText = "...";

    [Header("Dialogue")]
    public TextAsset DialogueJson;

    private DialogueTree dialogueTree;
    private NPCReveal reveal;
    private bool hasSpoken = false;

    public string InteractionPrompt
    {
        get
        {
            // Before reveal: mysterious prompt. After: show name
            if (hasSpoken || (reveal != null && reveal.IsRevealed))
                return $"Press E: Talk to {NpcName}";
            else
                return "Press E: ???";
        }
    }

    void Start()
    {
        reveal = GetComponent<NPCReveal>();

        if (DialogueJson != null)
            dialogueTree = JsonUtility.FromJson<DialogueTree>(DialogueJson.text);

        if (dialogueTree == null)
            dialogueTree = CreateDefaultDialogue();
    }

    public void Interact()
    {
        // First interaction: reveal true form
        if (!hasSpoken)
        {
            hasSpoken = true;
            if (reveal != null && !reveal.IsRevealed)
                reveal.Reveal();
        }

        if (DialogueSystem.Instance != null && !DialogueSystem.Instance.IsActive)
            DialogueSystem.Instance.StartDialogue(dialogueTree);
    }

    public void SetDialogue(DialogueTree tree) { dialogueTree = tree; }

    DialogueTree CreateDefaultDialogue()
    {
        return new DialogueTree
        {
            NpcName = NpcName,
            Nodes = new DialogueNode[]
            {
                new DialogueNode
                {
                    Speaker = NpcName,
                    Text = GreetingText,
                    Choices = new DialogueChoice[]
                    {
                        new DialogueChoice { Text = "Help them", NextNodeIndex = 1, ActionType = KarmaActionType.Help },
                        new DialogueChoice { Text = "Ignore them", NextNodeIndex = 2, ActionType = KarmaActionType.Ignore },
                    }
                },
                new DialogueNode { Speaker = NpcName, Text = "Thank you!", NextNodeIndex = -1 },
                new DialogueNode { Speaker = NpcName, Text = "...", NextNodeIndex = -1 }
            }
        };
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            Debug.Log($"[NPC] {InteractionPrompt}");
    }
}