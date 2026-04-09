using System;

/// <summary>
/// Relationship between player and one NPC
/// Tracks closeness, type, history, and how it evolves
/// </summary>
[Serializable]
public class Relationship
{
    public string NpcId;
    public string NpcName;

    // Closeness: -100 (enemy) to +100 (soulmate)
    public int Closeness = 0;

    // Relationship stage
    public RelationStage Stage = RelationStage.Stranger;

    // Relationship type (can evolve)
    public RelationType Type = RelationType.None;

    // Interaction count (player energy is limited)
    public int InteractionCount = 0;

    // Memory: key moments in this relationship
    public string[] Memories = Array.Empty<string>();

    // NPC personality affects how relationship develops
    public NPCProfile Profile;

    /// <summary>
    /// Interact with this NPC. Returns narrative result.
    /// Closeness change depends on NPC personality + player action.
    /// </summary>
    public string Interact(SocialAction action)
    {
        InteractionCount++;
        int delta = CalcClosenessDelta(action, Profile);
        Closeness = Math.Max(-100, Math.Min(100, Closeness + delta));
        UpdateStage();
        return GetNarrativeResult(action, delta);
    }

    int CalcClosenessDelta(SocialAction action, NPCProfile p)
    {
        if (p == null) return 0;
        var r = new Random(NpcId.GetHashCode() + InteractionCount);

        switch (action)
        {
            case SocialAction.Chat:
                // Introverts warm up slower
                return p.Introversion > 7 ? r.Next(0, 3) : r.Next(1, 5);

            case SocialAction.Help:
                // Kind NPCs appreciate help more
                return p.Kindness > 5 ? r.Next(3, 8) : r.Next(1, 5);

            case SocialAction.Gift:
                // Ambitious NPCs care less about gifts
                return p.Ambition > 7 ? r.Next(0, 3) : r.Next(2, 6);

            case SocialAction.Joke:
                // Humorous NPCs love jokes, serious ones don't
                return p.Humor > 5 ? r.Next(2, 7) : r.Next(-2, 3);

            case SocialAction.Argue:
                // High temper NPCs escalate, loyal ones forgive
                int loss = p.Temper > 6 ? r.Next(-10, -3) : r.Next(-5, -1);
                if (p.Loyalty > 7) loss /= 2; // loyal NPCs forgive easier
                return loss;

            case SocialAction.Betray:
                // Everyone hates betrayal, loyal NPCs hurt most
                return p.Loyalty > 5 ? r.Next(-20, -10) : r.Next(-15, -5);

            case SocialAction.Confess:
                // Romantic confession - depends on closeness + personality
                if (Closeness < 30) return r.Next(-10, -3); // too early
                return p.Kindness > 5 ? r.Next(5, 15) : r.Next(-5, 10);

            case SocialAction.Ignore:
                // Introverts don't mind, extroverts do
                return p.Introversion > 6 ? r.Next(-1, 1) : r.Next(-5, -1);

            default:
                return r.Next(-1, 3);
        }
    }

    void UpdateStage()
    {
        if (Closeness >= 80) Stage = RelationStage.Soulmate;
        else if (Closeness >= 50) Stage = RelationStage.CloseFriend;
        else if (Closeness >= 20) Stage = RelationStage.Friend;
        else if (Closeness >= 5) Stage = RelationStage.Acquaintance;
        else if (Closeness >= -10) Stage = RelationStage.Stranger;
        else if (Closeness >= -40) Stage = RelationStage.Dislike;
        else Stage = RelationStage.Enemy;
    }

    string GetNarrativeResult(SocialAction action, int delta)
    {
        if (delta >= 5) return $"{NpcName} seems really happy.";
        if (delta >= 1) return $"{NpcName} nods warmly.";
        if (delta == 0) return $"{NpcName} doesn't react much.";
        if (delta >= -3) return $"{NpcName} looks a bit uncomfortable.";
        return $"{NpcName} seems upset.";
    }
}

public enum RelationStage
{
    Enemy,          // -100 to -40
    Dislike,        // -40 to -10
    Stranger,       // -10 to 5
    Acquaintance,   // 5 to 20
    Friend,         // 20 to 50
    CloseFriend,    // 50 to 80
    Soulmate        // 80 to 100
}

public enum RelationType
{
    None,
    Friend,
    BestFriend,
    Lover,
    Spouse,
    Rival,
    Mentor,
    Student,
    Parent,
    Child,
    Sibling,
    ExLover,
    BusinessPartner
}

public enum SocialAction
{
    Chat,       // casual conversation
    Help,       // do something for them
    Gift,       // give them something
    Joke,       // lighten the mood
    Argue,      // disagree/conflict
    Betray,     // break trust
    Confess,    // romantic confession
    Ignore      // walk away
}