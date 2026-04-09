using System.Collections.Generic;

/// <summary>
/// Generates dialogue trees procedurally based on NPC personality, role, scene, and age
/// No hand-written JSON needed - every NPC gets unique dialogue
/// 
/// Dialogue is assembled from pools of lines categorized by:
/// - NPC role (teacher, colleague, stranger, etc.)
/// - NPC personality (kind, ambitious, funny, angry, etc.)
/// - Scene context (school, city, hospital, etc.)
/// - Player age phase
/// </summary>
public static class DialogueGenerator
{
    /// <summary>
    /// Generate dialogue for NPC who approached the player (warmer, more initiative)
    /// </summary>
    public static DialogueTree GenerateApproach(NPCProfile npc, string sceneId, int playerAge)
    {
        var r = new System.Random(npc.Id.GetHashCode() + playerAge + 999);
        string greeting = PickApproachGreeting(npc, r);
        var choices = PickApproachChoices(npc, r);
        var responses = new List<DialogueNode>();

        int nodeIdx = 1;
        var choiceList = new List<DialogueChoice>();
        foreach (var c in choices)
        {
            choiceList.Add(new DialogueChoice
            {
                Text = c.PlayerText,
                NextNodeIndex = nodeIdx,
                ActionType = c.Action,
                GoldChange = c.Gold
            });
            responses.Add(new DialogueNode
            {
                Speaker = npc.Name,
                Text = c.NpcResponse,
                NextNodeIndex = -1,
                Choices = null
            });
            nodeIdx++;
        }

        var nodes = new List<DialogueNode>();
        nodes.Add(new DialogueNode
        {
            Speaker = npc.Name,
            Text = greeting,
            NextNodeIndex = -1,
            Choices = choiceList.ToArray()
        });
        nodes.AddRange(responses);

        return new DialogueTree { NpcName = npc.Name, Nodes = nodes.ToArray() };
    }

    static string PickApproachGreeting(NPCProfile npc, System.Random r)
    {
        var pool = new List<string>();

        switch (npc.Role)
        {
            case NPCRole.Romantic:
                pool.AddRange(new[] {
                    "Hey... I've been wanting to talk to you for a while.",
                    "I noticed you from across the room. I'm " + npc.Name + ".",
                    "Sorry to bother you, but... I think you're really interesting.",
                    "I kept hoping we'd run into each other. Here we are.",
                    "My friends dared me to come talk to you. But honestly, I wanted to anyway."
                });
                break;
            case NPCRole.Family:
                pool.AddRange(new[] {
                    "There you are! I've been looking for you.",
                    "Hey, come here. I need to tell you something.",
                    "I was worried about you. How are you doing?",
                    "Don't run off without telling me next time!"
                });
                break;
            case NPCRole.Rival:
                pool.AddRange(new[] {
                    "We need to talk. Now.",
                    "I've been watching you. You think you're so great?",
                    "Don't ignore me. I have something to say."
                });
                break;
            default:
                pool.AddRange(new[] {
                    "Hey! You look like someone I'd get along with.",
                    "Excuse me! I just had to come say hi.",
                    "I don't usually do this, but... hi! I'm " + npc.Name + ".",
                    "You seem different from everyone else here. In a good way.",
                    "Sorry to just walk up like this, but I felt like we should meet."
                });
                if (npc.Humor > 7)
                    pool.Add("I promise I'm not weird. Okay, maybe a little. Hi!!");
                if (npc.Kindness > 7)
                    pool.Add("You looked like you could use a friend. I'm " + npc.Name + ".");
                break;
        }

        return pool[r.Next(pool.Count)];
    }

    static List<ChoiceTemplate> PickApproachChoices(NPCProfile npc, System.Random r)
    {
        var choices = new List<ChoiceTemplate>();

        // Warm response (reciprocate)
        var warmPool = new ChoiceTemplate[] {
            new ChoiceTemplate("Nice to meet you too! I'm glad you came over.", WarmApproachResponse(npc, r), KarmaActionType.Help, 0),
            new ChoiceTemplate("Hey! Yeah, let's talk. What's on your mind?", WarmApproachResponse(npc, r), KarmaActionType.Help, 0),
            new ChoiceTemplate("I was hoping someone would talk to me. Hi!", WarmApproachResponse(npc, r), KarmaActionType.Selfless, 0),
        };
        choices.Add(warmPool[r.Next(warmPool.Length)]);

        // Cool/distant response
        var coolPool = new ChoiceTemplate[] {
            new ChoiceTemplate("Oh, hi. I'm kind of busy right now.", CoolApproachResponse(npc, r), KarmaActionType.Ignore, 0),
            new ChoiceTemplate("Do I know you?", CoolApproachResponse(npc, r), KarmaActionType.Neutral, 0),
        };
        choices.Add(coolPool[r.Next(coolPool.Length)]);

        // Role-specific third choice
        if (npc.Role == NPCRole.Romantic)
        {
            choices.Add(new ChoiceTemplate(
                "You're pretty brave coming up to me like that. I like it.",
                FlirtyResponse(npc, r), KarmaActionType.Neutral, 0));
        }
        else if (npc.Role == NPCRole.Rival)
        {
            choices.Add(new ChoiceTemplate(
                "Bring it on. I'm not afraid of you.",
                RivalBraveResponse(npc, r), KarmaActionType.Neutral, 0));
        }
        else
        {
            choices.Add(new ChoiceTemplate(
                "Sure, want to grab a drink or something?",
                FriendlyResponse(npc, r), KarmaActionType.Help, -5));
        }

        return choices;
    }

    static string WarmApproachResponse(NPCProfile npc, System.Random r)
    {
        var pool = new[] {
            "See? I knew you'd be cool. This is going to be a good friendship.",
            "I'm so glad I came over. You're easy to talk to.",
            "Finally, someone normal around here! Haha.",
            "I have a feeling we'll be seeing a lot more of each other."
        };
        return pool[r.Next(pool.Length)];
    }

    static string CoolApproachResponse(NPCProfile npc, System.Random r)
    {
        if (npc.Temper > 6)
            return "Fine. Forget I said anything.";
        if (npc.Kindness > 6)
            return "Oh, sorry! I didn't mean to bother you. Maybe another time.";
        return "Okay... I'll leave you alone then.";
    }

    static string FlirtyResponse(NPCProfile npc, System.Random r)
    {
        var pool = new[] {
            "*blushes* You're not so bad yourself.",
            "Haha, I almost chickened out three times. Worth it though.",
            "I think my heart just skipped a beat."
        };
        return pool[r.Next(pool.Length)];
    }

    static string FriendlyResponse(NPCProfile npc, System.Random r)
    {
        var pool = new[] {
            "Yes! I know a great place nearby. Let's go!",
            "I'd love that. It's been a while since I made a new friend.",
            "Sure! My treat."
        };
        return pool[r.Next(pool.Length)];
    }

    /// <summary>
    /// Generate a complete dialogue tree for an NPC
    /// </summary>
    public static DialogueTree Generate(NPCProfile npc, string sceneId, int playerAge)
    {
        var r = new System.Random(npc.Id.GetHashCode() + playerAge);
        string greeting = PickGreeting(npc, sceneId, playerAge, r);
        var choices = PickChoices(npc, sceneId, playerAge, r);
        var responses = new List<DialogueNode>();

        // Build response nodes for each choice
        int nodeIdx = 1;
        var choiceList = new List<DialogueChoice>();
        foreach (var c in choices)
        {
            choiceList.Add(new DialogueChoice
            {
                Text = c.PlayerText,
                NextNodeIndex = nodeIdx,
                ActionType = c.Action,
                GoldChange = c.Gold
            });
            responses.Add(new DialogueNode
            {
                Speaker = npc.Name,
                Text = c.NpcResponse,
                NextNodeIndex = -1,
                Choices = null
            });
            nodeIdx++;
        }

        // Build tree
        var nodes = new List<DialogueNode>();
        nodes.Add(new DialogueNode
        {
            Speaker = npc.Name,
            Text = greeting,
            NextNodeIndex = -1,
            Choices = choiceList.ToArray()
        });
        nodes.AddRange(responses);

        return new DialogueTree
        {
            NpcName = npc.Name,
            Nodes = nodes.ToArray()
        };
    }

    // ==================== Greeting Pools ====================

    static string PickGreeting(NPCProfile npc, string sceneId, int age, System.Random r)
    {
        var pool = new List<string>();

        // Role-based greetings
        switch (npc.Role)
        {
            case NPCRole.Family:
                pool.AddRange(new[] {
                    "Hey, how's your day going?",
                    "You look tired. Everything okay?",
                    "Come sit down, let's talk.",
                    "I made your favorite food today.",
                    "We need to talk about something."
                });
                break;
            case NPCRole.Classmate:
                pool.AddRange(new[] {
                    "Hey! Did you finish the homework?",
                    "Want to hang out after class?",
                    "Did you hear what happened yesterday?",
                    "Can I borrow your notes?",
                    "The exam is tomorrow... I'm so nervous."
                });
                break;
            case NPCRole.Colleague:
                pool.AddRange(new[] {
                    "Morning. Coffee?",
                    "Did you see the email from the boss?",
                    "I'm thinking about quitting. Don't tell anyone.",
                    "Want to grab lunch together?",
                    "Can you help me with this project?"
                });
                break;
            case NPCRole.Authority:
                pool.AddRange(new[] {
                    "I need to speak with you.",
                    "Your performance has been... interesting.",
                    "I see potential in you.",
                    "We have a problem.",
                    "I have an opportunity for you."
                });
                break;
            case NPCRole.Romantic:
                pool.AddRange(new[] {
                    "Oh, hi... I didn't expect to see you here.",
                    "I was just thinking about you.",
                    "Do you want to go somewhere quieter?",
                    "There's something I've been wanting to tell you...",
                    "You look nice today."
                });
                break;
            case NPCRole.Rival:
                pool.AddRange(new[] {
                    "Oh, it's you.",
                    "I heard you got the promotion. Congrats, I guess.",
                    "Don't think you're better than me.",
                    "Let's see who does better this time.",
                    "Stay out of my way."
                });
                break;
            case NPCRole.Elder:
                pool.AddRange(new[] {
                    "Young one, come sit with me.",
                    "You remind me of someone I knew long ago.",
                    "Want to hear a story?",
                    "Life is shorter than you think.",
                    "I've seen a lot. Let me give you some advice."
                });
                break;
            case NPCRole.Child:
                pool.AddRange(new[] {
                    "Hey mister/miss! Look what I found!",
                    "Can you play with me?",
                    "Why do grown-ups always look so tired?",
                    "I lost my ball... can you help?",
                    "When I grow up, I want to be a superhero!"
                });
                break;
            default: // Stranger, Neighbor
                pool.AddRange(new[] {
                    "Excuse me...",
                    "Nice weather today, isn't it?",
                    "Do you live around here?",
                    "Sorry to bother you, but...",
                    "Hey, can I ask you something?"
                });
                break;
        }

        // Personality modifiers - add extra lines
        if (npc.Humor > 7)
            pool.AddRange(new[] { "Haha, you won't believe what just happened!", "Want to hear a joke?" });
        if (npc.Temper > 7)
            pool.AddRange(new[] { "What are you looking at?", "I'm not in the mood today." });
        if (npc.Kindness > 7)
            pool.AddRange(new[] { "You look like you could use a friend.", "Is there anything I can help with?" });
        if (npc.Introversion > 7)
            pool.AddRange(new[] { "...", "Oh, um... hi.", "I don't usually talk to people." });

        return pool[r.Next(pool.Count)];
    }

    // ==================== Choice Pools ====================

    static List<ChoiceTemplate> PickChoices(NPCProfile npc, string sceneId, int age, System.Random r)
    {
        var choices = new List<ChoiceTemplate>();

        // Always have a kind option
        choices.Add(PickKindChoice(npc, r));

        // Always have a neutral/selfish option
        choices.Add(PickSelfishChoice(npc, r));

        // Third choice based on context
        if (npc.Role == NPCRole.Romantic)
            choices.Add(PickRomanticChoice(npc, r));
        else if (npc.Role == NPCRole.Rival)
            choices.Add(PickRivalChoice(npc, r));
        else if (npc.Role == NPCRole.Child || npc.Role == NPCRole.Elder)
            choices.Add(PickWisdomChoice(npc, r));
        else
            choices.Add(PickNeutralChoice(npc, r));

        return choices;
    }

    static ChoiceTemplate PickKindChoice(NPCProfile npc, System.Random r)
    {
        var options = new ChoiceTemplate[] {
            new ChoiceTemplate("Sure, I'd love to help.", KindResponse(npc, r), KarmaActionType.Help, 0),
            new ChoiceTemplate("Tell me more, I'm listening.", KindResponse(npc, r), KarmaActionType.Help, 0),
            new ChoiceTemplate("That sounds tough. I'm here for you.", KindResponse(npc, r), KarmaActionType.Selfless, 0),
            new ChoiceTemplate("Let me see what I can do.", KindResponse(npc, r), KarmaActionType.Help, -10),
            new ChoiceTemplate("You're not alone in this.", KindResponse(npc, r), KarmaActionType.Selfless, 0),
        };
        return options[r.Next(options.Length)];
    }

    static ChoiceTemplate PickSelfishChoice(NPCProfile npc, System.Random r)
    {
        var options = new ChoiceTemplate[] {
            new ChoiceTemplate("Sorry, I'm busy.", ColdResponse(npc, r), KarmaActionType.Ignore, 0),
            new ChoiceTemplate("That's not my problem.", ColdResponse(npc, r), KarmaActionType.Selfish, 0),
            new ChoiceTemplate("What's in it for me?", TransactionalResponse(npc, r), KarmaActionType.Selfish, 10),
            new ChoiceTemplate("I don't have time for this.", ColdResponse(npc, r), KarmaActionType.Ignore, 0),
            new ChoiceTemplate("Figure it out yourself.", HarshResponse(npc, r), KarmaActionType.Harm, 0),
        };
        return options[r.Next(options.Length)];
    }

    static ChoiceTemplate PickRomanticChoice(NPCProfile npc, System.Random r)
    {
        var options = new ChoiceTemplate[] {
            new ChoiceTemplate("I like spending time with you.", RomanticResponse(npc, r), KarmaActionType.Neutral, 0),
            new ChoiceTemplate("You mean a lot to me.", RomanticResponse(npc, r), KarmaActionType.Selfless, 0),
            new ChoiceTemplate("Let's just be friends.", FriendZoneResponse(npc, r), KarmaActionType.Neutral, 0),
        };
        return options[r.Next(options.Length)];
    }

    static ChoiceTemplate PickRivalChoice(NPCProfile npc, System.Random r)
    {
        var options = new ChoiceTemplate[] {
            new ChoiceTemplate("Let's settle this fairly.", RivalFairResponse(npc, r), KarmaActionType.Selfless, 0),
            new ChoiceTemplate("You don't scare me.", RivalBraveResponse(npc, r), KarmaActionType.Neutral, 0),
            new ChoiceTemplate("Maybe we don't have to be enemies.", RivalPeaceResponse(npc, r), KarmaActionType.Help, 0),
        };
        return options[r.Next(options.Length)];
    }

    static ChoiceTemplate PickWisdomChoice(NPCProfile npc, System.Random r)
    {
        var options = new ChoiceTemplate[] {
            new ChoiceTemplate("Tell me about your life.", WisdomResponse(npc, r), KarmaActionType.Help, 0),
            new ChoiceTemplate("What advice would you give?", WisdomResponse(npc, r), KarmaActionType.Neutral, 0),
            new ChoiceTemplate("I wish I had more time.", TimeResponse(npc, r), KarmaActionType.Neutral, 0),
        };
        return options[r.Next(options.Length)];
    }

    static ChoiceTemplate PickNeutralChoice(NPCProfile npc, System.Random r)
    {
        var options = new ChoiceTemplate[] {
            new ChoiceTemplate("Interesting. Tell me more.", NeutralResponse(npc, r), KarmaActionType.Neutral, 0),
            new ChoiceTemplate("I see. Good luck with that.", NeutralResponse(npc, r), KarmaActionType.Neutral, 0),
            new ChoiceTemplate("Let's talk about something else.", NeutralResponse(npc, r), KarmaActionType.Neutral, 0),
        };
        return options[r.Next(options.Length)];
    }

    // ==================== Response Generators ====================

    static string KindResponse(NPCProfile npc, System.Random r)
    {
        if (npc.Kindness > 6)
        {
            var pool = new[] { "Thank you so much! You're a good person.", "I really appreciate that.", "You have no idea how much this means to me.", "The world needs more people like you." };
            return pool[r.Next(pool.Length)];
        }
        var pool2 = new[] { "Oh... thanks, I guess.", "Didn't expect that. Thanks.", "Huh. Okay then." };
        return pool2[r.Next(pool2.Length)];
    }

    static string ColdResponse(NPCProfile npc, System.Random r)
    {
        if (npc.Temper > 6)
        {
            var pool = new[] { "Fine. Whatever.", "Typical.", "I'll remember this.", "Don't come to me when YOU need help." };
            return pool[r.Next(pool.Length)];
        }
        var pool2 = new[] { "Oh... okay.", "I understand. You're busy.", "No worries..." };
        return pool2[r.Next(pool2.Length)];
    }

    static string HarshResponse(NPCProfile npc, System.Random r)
    {
        if (npc.Temper > 7)
        {
            var pool = new[] { "You'll regret saying that.", "We're done.", "I thought you were different." };
            return pool[r.Next(pool.Length)];
        }
        var pool2 = new[] { "...", "That hurt.", "Okay. I'll go." };
        return pool2[r.Next(pool2.Length)];
    }

    static string TransactionalResponse(NPCProfile npc, System.Random r)
    {
        if (npc.Ambition > 6)
        {
            var pool = new[] { "Fair enough. Here's the deal...", "I respect that. Let's negotiate.", "A businessperson, I see." };
            return pool[r.Next(pool.Length)];
        }
        var pool2 = new[] { "Really? That's how you see this?", "I was hoping for kindness, not a transaction.", "..." };
        return pool2[r.Next(pool2.Length)];
    }

    static string RomanticResponse(NPCProfile npc, System.Random r)
    {
        var pool = new[] { "I... feel the same way.", "You make me smile.", "I've been wanting to hear that.", "My heart is beating so fast right now." };
        return pool[r.Next(pool.Length)];
    }

    static string FriendZoneResponse(NPCProfile npc, System.Random r)
    {
        var pool = new[] { "Oh... friends. Right. Of course.", "I understand. Friends is good too.", "...yeah. Friends." };
        return pool[r.Next(pool.Length)];
    }

    static string RivalFairResponse(NPCProfile npc, System.Random r)
    {
        var pool = new[] { "Fine. May the best one win.", "At least you have some honor.", "Deal." };
        return pool[r.Next(pool.Length)];
    }

    static string RivalBraveResponse(NPCProfile npc, System.Random r)
    {
        var pool = new[] { "Hmph. We'll see about that.", "Big words. Back them up.", "I like your spirit. But you'll still lose." };
        return pool[r.Next(pool.Length)];
    }

    static string RivalPeaceResponse(NPCProfile npc, System.Random r)
    {
        if (npc.Kindness > 5)
        {
            var pool = new[] { "...maybe you're right.", "I'm tired of fighting too.", "Truce?" };
            return pool[r.Next(pool.Length)];
        }
        var pool2 = new[] { "Don't be naive.", "Enemies is all we'll ever be.", "Nice try." };
        return pool2[r.Next(pool2.Length)];
    }

    static string WisdomResponse(NPCProfile npc, System.Random r)
    {
        var pool = new[] {
            "When I was your age, I thought I had all the time in the world. I didn't.",
            "The things that matter most are the things you can't buy.",
            "Don't wait for the perfect moment. It doesn't exist.",
            "I wish someone had told me to slow down.",
            "The people you love won't be here forever. Neither will you.",
            "Every choice closes a door and opens another. That's not sad. That's life."
        };
        return pool[r.Next(pool.Length)];
    }

    static string TimeResponse(NPCProfile npc, System.Random r)
    {
        var pool = new[] { "Time is the one thing you can never get back.", "We all wish that.", "Then don't waste what you have." };
        return pool[r.Next(pool.Length)];
    }

    static string NeutralResponse(NPCProfile npc, System.Random r)
    {
        if (npc.Humor > 7)
        {
            var pool = new[] { "Haha, sure thing!", "You're funny. I like you.", "Life's too short to be serious!" };
            return pool[r.Next(pool.Length)];
        }
        var pool2 = new[] { "Sure.", "Okay.", "Alright then.", "Mm-hmm." };
        return pool2[r.Next(pool2.Length)];
    }
}

class ChoiceTemplate
{
    public string PlayerText;
    public string NpcResponse;
    public KarmaActionType Action;
    public int Gold;
    public ChoiceTemplate(string pt, string nr, KarmaActionType a, int g)
    { PlayerText = pt; NpcResponse = nr; Action = a; Gold = g; }
}