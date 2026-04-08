using System;

/// <summary>
/// A complete dialogue tree for one NPC conversation
/// </summary>
[Serializable]
public class DialogueTree
{
    public string NpcName;
    public DialogueNode[] Nodes;
}

/// <summary>
/// A single node in the dialogue tree
/// Can be a line of text (with Continue) or a choice point
/// </summary>
[Serializable]
public class DialogueNode
{
    public string Speaker;          // who is talking
    public string Text;             // what they say
    public int NextNodeIndex = -1;  // for linear flow (-1 = end)
    public DialogueChoice[] Choices;// null or empty = just "Continue"
}

/// <summary>
/// A player choice in dialogue
/// ActionType drives hidden karma, player only sees Text
/// </summary>
[Serializable]
public class DialogueChoice
{
    public string Text;                     // what player sees
    public int NextNodeIndex = -1;          // where to go next (-1 = end)
    public KarmaActionType ActionType;      // hidden karma effect
    public int GoldChange;                  // visible gold change
}