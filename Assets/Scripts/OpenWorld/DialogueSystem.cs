using UnityEngine;
using System;

/// <summary>
/// Dialogue system for NPC conversations in open world
/// Supports branching dialogue trees with karma-tracked choices
/// </summary>
public class DialogueSystem : MonoBehaviour
{
    public static DialogueSystem Instance { get; private set; }

    public event Action<DialogueNode> OnDialogueStarted;
    public event Action OnDialogueEnded;
    public event Action<DialogueNode> OnNodeChanged;

    private DialogueTree currentTree;
    private DialogueNode currentNode;
    private bool isActive;

    public bool IsActive => isActive;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    /// <summary>
    /// Start a dialogue tree
    /// </summary>
    public void StartDialogue(DialogueTree tree)
    {
        if (tree == null || tree.Nodes == null || tree.Nodes.Length == 0) return;
        currentTree = tree;
        currentNode = tree.Nodes[0];
        isActive = true;
        OnDialogueStarted?.Invoke(currentNode);
        Debug.Log($"[Dialogue] Started: {tree.NpcName}");
    }

    /// <summary>
    /// Player picks a choice (0-indexed)
    /// </summary>
    public void PickChoice(int index)
    {
        if (!isActive || currentNode == null) return;
        if (currentNode.Choices == null || index >= currentNode.Choices.Length) return;

        var choice = currentNode.Choices[index];

        // Track karma through KarmaTracker (hidden from player)
        if (KarmaTracker.Instance != null)
        {
            switch (choice.ActionType)
            {
                case KarmaActionType.Help: KarmaTracker.Instance.HelpedSomeone(choice.Text); break;
                case KarmaActionType.Harm: KarmaTracker.Instance.HarmedSomeone(choice.Text); break;
                case KarmaActionType.Selfish: KarmaTracker.Instance.SelfishChoice(choice.Text); break;
                case KarmaActionType.Selfless: KarmaTracker.Instance.SelflessChoice(choice.Text); break;
                case KarmaActionType.Ignore: KarmaTracker.Instance.IgnoredSomeone(choice.Text); break;
                default: KarmaTracker.Instance.NeutralAction(choice.Text); break;
            }
        }

        // Gold change (visible)
        if (GameManager.Instance != null)
            GameManager.Instance.Player.Gold += choice.GoldChange;

        // Navigate to next node
        if (choice.NextNodeIndex >= 0 && choice.NextNodeIndex < currentTree.Nodes.Length)
        {
            currentNode = currentTree.Nodes[choice.NextNodeIndex];
            OnNodeChanged?.Invoke(currentNode);
        }
        else
        {
            EndDialogue();
        }
    }

    /// <summary>
    /// Advance to next node (for nodes without choices, just "continue")
    /// </summary>
    public void Continue()
    {
        if (!isActive || currentNode == null) return;

        if (currentNode.NextNodeIndex >= 0 && currentNode.NextNodeIndex < currentTree.Nodes.Length)
        {
            currentNode = currentTree.Nodes[currentNode.NextNodeIndex];
            OnNodeChanged?.Invoke(currentNode);
        }
        else
        {
            EndDialogue();
        }
    }

    public void EndDialogue()
    {
        isActive = false;
        currentTree = null;
        currentNode = null;
        OnDialogueEnded?.Invoke();
        Debug.Log("[Dialogue] Ended");
    }
}