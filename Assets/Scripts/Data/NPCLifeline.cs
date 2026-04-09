using System;
using System.Collections.Generic;

/// <summary>
/// NPC Life Timeline - NPCs have their own life events
/// They age alongside the player, have families, milestones, problems
/// Close NPCs will INVITE the player to their life events
/// </summary>
[Serializable]
public class NPCLifeline
{
    public string NpcId;
    public string NpcName;

    // NPC's own life data
    public int BirthYear;           // relative to player (e.g. same age, 2 years older)
    public int CurrentAge;
    public bool IsAlive = true;
    public int DeathAge = -1;       // -1 = not yet determined

    // Family tree
    public string FatherId;         // NPCProfile id
    public string MotherId;
    public string SpouseId;
    public List<string> ChildrenIds = new List<string>();

    // Life milestones (age -> event)
    public List<NPCLifeEvent> LifeEvents = new List<NPCLifeEvent>();

    // Current status
    public NPCLifeStatus Status = NPCLifeStatus.Single;
    public string Occupation = "";
    public int Wealth = 0;          // 0-10

    /// <summary>
    /// Generate a full life timeline for this NPC
    /// Called once when NPC is first created
    /// </summary>
    public static NPCLifeline Generate(NPCProfile profile, int playerBirthAge)
    {
        var r = new Random(profile.Id.GetHashCode());
        var life = new NPCLifeline
        {
            NpcId = profile.Id,
            NpcName = profile.Name,
            BirthYear = playerBirthAge + r.Next(-5, 6), // born within 5 years of player
            CurrentAge = 0,
            Wealth = r.Next(1, 8)
        };

        // Determine death age (60-100, weighted)
        life.DeathAge = 60 + r.Next(0, 41);
        if (profile.Kindness > 7) life.DeathAge += 5; // kind people live longer :)

        // Generate parents
        life.FatherId = "father_of_" + profile.Id;
        life.MotherId = "mother_of_" + profile.Id;

        // Generate life events
        life.LifeEvents = GenerateEvents(profile, life.DeathAge, r);

        // Occupation based on personality
        if (profile.Ambition > 7) life.Occupation = GetAmbitiousJob(r);
        else if (profile.Kindness > 7) life.Occupation = GetKindJob(r);
        else life.Occupation = GetNormalJob(r);

        return life;
    }

    static List<NPCLifeEvent> GenerateEvents(NPCProfile p, int deathAge, Random r)
    {
        var events = new List<NPCLifeEvent>();

        // School graduation
        events.Add(new NPCLifeEvent { Age = 18, Type = NPCEventType.Graduation, Title = "Graduated high school" });

        // College (if ambitious)
        if (p.Ambition > 4)
            events.Add(new NPCLifeEvent { Age = 22, Type = NPCEventType.Graduation, Title = "Graduated college" });

        // First job
        int jobAge = p.Ambition > 4 ? 23 : 19;
        events.Add(new NPCLifeEvent { Age = jobAge, Type = NPCEventType.Career, Title = "Started working" });

        // Marriage (if not too introverted)
        if (p.Introversion < 8)
        {
            int marryAge = 24 + r.Next(0, 10);
            if (marryAge < deathAge)
            {
                events.Add(new NPCLifeEvent { Age = marryAge, Type = NPCEventType.Marriage, Title = "Got married", CanInvitePlayer = true });

                // Children
                int childAge = marryAge + r.Next(1, 5);
                if (childAge < deathAge - 10)
                {
                    events.Add(new NPCLifeEvent { Age = childAge, Type = NPCEventType.ChildBorn, Title = "Had a child", CanInvitePlayer = true });

                    // Second child maybe
                    if (r.Next(100) < 40)
                    {
                        int child2 = childAge + r.Next(2, 5);
                        if (child2 < deathAge - 5)
                            events.Add(new NPCLifeEvent { Age = child2, Type = NPCEventType.ChildBorn, Title = "Had another child", CanInvitePlayer = true });
                    }
                }
            }
        }

        // Birthday milestones
        foreach (int bday in new[] { 30, 40, 50, 60, 70, 80 })
        {
            if (bday < deathAge)
                events.Add(new NPCLifeEvent { Age = bday, Type = NPCEventType.Birthday, Title = $"Turned {bday}", CanInvitePlayer = true });
        }

        // Crisis events (random)
        if (r.Next(100) < 50)
        {
            int crisisAge = 30 + r.Next(0, 25);
            if (crisisAge < deathAge)
            {
                var crisisTypes = new[] { NPCEventType.FinancialTrouble, NPCEventType.HealthCrisis, NPCEventType.LifeDilemma };
                var crisisTitles = new[] { "Financial trouble", "Health crisis", "Life dilemma" };
                int ci = r.Next(crisisTypes.Length);
                events.Add(new NPCLifeEvent { Age = crisisAge, Type = crisisTypes[ci], Title = crisisTitles[ci], CanInvitePlayer = true, NeedsHelp = true });
            }
        }

        // Retirement
        int retireAge = 55 + r.Next(0, 10);
        if (retireAge < deathAge)
            events.Add(new NPCLifeEvent { Age = retireAge, Type = NPCEventType.Retirement, Title = "Retired" });

        // Death
        events.Add(new NPCLifeEvent { Age = deathAge, Type = NPCEventType.Death, Title = "Passed away", CanInvitePlayer = true });

        // Sort by age
        events.Sort((a, b) => a.Age.CompareTo(b.Age));
        return events;
    }

    static string GetAmbitiousJob(Random r)
    {
        var jobs = new[] { "CEO", "Lawyer", "Surgeon", "Entrepreneur", "Politician", "Professor" };
        return jobs[r.Next(jobs.Length)];
    }
    static string GetKindJob(Random r)
    {
        var jobs = new[] { "Teacher", "Nurse", "Social Worker", "Volunteer", "Therapist", "Vet" };
        return jobs[r.Next(jobs.Length)];
    }
    static string GetNormalJob(Random r)
    {
        var jobs = new[] { "Office Worker", "Driver", "Cook", "Shopkeeper", "Mechanic", "Farmer", "Clerk" };
        return jobs[r.Next(jobs.Length)];
    }

    /// <summary>
    /// Get events that happen at a specific age
    /// </summary>
    public List<NPCLifeEvent> GetEventsAtAge(int npcAge)
    {
        return LifeEvents.FindAll(e => e.Age == npcAge);
    }
}

[Serializable]
public class NPCLifeEvent
{
    public int Age;
    public NPCEventType Type;
    public string Title;
    public bool CanInvitePlayer;    // will invite player if closeness is high enough
    public bool NeedsHelp;          // NPC needs player's help (money, advice, etc.)
    public bool PlayerAttended;     // did the player show up?
}

public enum NPCEventType
{
    Birthday,
    Graduation,
    Career,
    Marriage,
    ChildBorn,
    FinancialTrouble,
    HealthCrisis,
    LifeDilemma,
    Retirement,
    Death
}

public enum NPCLifeStatus
{
    Single,
    Dating,
    Married,
    Divorced,
    Widowed
}