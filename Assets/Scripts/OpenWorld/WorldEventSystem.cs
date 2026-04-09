using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// World Events System - Uncontrollable life events that happen TO the player
/// 
/// DESIGN PHILOSOPHY:
/// Life is not controllable. Bad things happen. Good things happen.
/// The player should feel: "I didn't choose this. But I have to deal with it."
/// 
/// Events can be:
/// - Random disasters (car accident, layoff, illness, natural disaster)
/// - NPC-driven drama (betrayal, bullying, toxic relationship, manipulation)
/// - Positive surprises (lottery, unexpected friendship, promotion)
/// - Emotional gut-punches (NPC death, breakup, family crisis)
/// 
/// Surviving hardship gives Resilience bonuses.
/// Being manipulated and breaking free gives Willpower bonuses.
/// </summary>
public class WorldEventSystem : MonoBehaviour
{
    public static WorldEventSystem Instance { get; private set; }

    // Events that have happened this life
    public List<string> LifeEventLog = new List<string>();

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    /// <summary>
    /// Roll for random world events at each age
    /// Called by BoardManager when player advances
    /// </summary>
    public WorldEventResult CheckWorldEvent(int age, PlayerStats stats)
    {
        var r = new System.Random(age * 31 + (stats?.Luck ?? 5));

        // Higher luck = fewer bad events
        int luckMod = stats != null ? stats.Luck - 10 : 0; // -10 to +10
        int roll = r.Next(0, 100) + luckMod;

        // Age-specific event pools
        if (age >= 7 && age <= 17) return CheckSchoolEvent(age, roll, r, stats);
        if (age >= 18 && age <= 30) return CheckYoungAdultEvent(age, roll, r, stats);
        if (age >= 31 && age <= 50) return CheckMidlifeEvent(age, roll, r, stats);
        if (age >= 51 && age <= 65) return CheckLateCareerEvent(age, roll, r, stats);
        if (age >= 66) return CheckElderEvent(age, roll, r, stats);

        return null; // no event
    }

    // ==================== School Age (7-17) ====================
    WorldEventResult CheckSchoolEvent(int age, int roll, System.Random r, PlayerStats stats)
    {
        if (roll > 30) return null; // 30% chance of event

        int type = r.Next(0, 100);

        if (type < 25)
        {
            // === BULLYING ===
            bool canResist = stats != null && (stats.Resilience + stats.Willpower) > 10;
            if (canResist)
            {
                stats?.Modify(StatType.Resilience, 2);
                stats?.Modify(StatType.Willpower, 1);
                return new WorldEventResult
                {
                    Title = "School Bullying",
                    Description = "Some older kids targeted you. But you stood your ground. It wasn't easy, but you got through it.",
                    Survived = true,
                    Achievement = "Stood Tall",
                    ResilienceGain = 2,
                    WillpowerGain = 1
                };
            }
            else
            {
                stats?.Modify(StatType.Resilience, 1); // still gain some from suffering
                return new WorldEventResult
                {
                    Title = "School Bullying",
                    Description = "Some older kids targeted you. The days felt endless. You cried alone at night. But somehow, you survived.",
                    Survived = true,
                    ResilienceGain = 1,
                    GoldChange = -10,
                    EmotionalImpact = "The scars stay with you."
                };
            }
        }
        else if (type < 45)
        {
            // === TOXIC FRIEND ===
            bool canSeeThrough = stats != null && stats.Empathy > 8;
            if (canSeeThrough)
            {
                stats?.Modify(StatType.Willpower, 1);
                return new WorldEventResult
                {
                    Title = "Toxic Friendship",
                    Description = "A 'friend' kept putting you down and using you. You sensed something was wrong and distanced yourself.",
                    Survived = true,
                    WillpowerGain = 1,
                    EmotionalImpact = "You learned to trust your instincts."
                };
            }
            else
            {
                return new WorldEventResult
                {
                    Title = "Toxic Friendship",
                    Description = "A 'friend' manipulated you for months. You did everything they asked. By the time you realized, you'd lost other friends.",
                    Survived = true,
                    GoldChange = -20,
                    EmotionalImpact = "You wonder if any of it was real."
                };
            }
        }
        else if (type < 60)
        {
            // === TEACHER WHO BELIEVES IN YOU ===
            stats?.Modify(StatType.Resilience, 1);
            stats?.Modify(StatType.Charisma, 1);
            return new WorldEventResult
            {
                Title = "A Teacher's Faith",
                Description = "One teacher saw something in you that no one else did. They stayed after school to help you. Years later, you still remember their words.",
                Survived = true,
                IsPositive = true,
                CharismaGain = 1,
                EmotionalImpact = "Someone believed in you."
            };
        }
        else
        {
            // === FIRST HEARTBREAK ===
            return new WorldEventResult
            {
                Title = "First Heartbreak",
                Description = "The person you liked chose someone else. The world didn't end, but it felt like it did.",
                Survived = true,
                ResilienceGain = 1,
                EmotionalImpact = "You learned that feelings don't always go both ways."
            };
        }
    }

    // ==================== Young Adult (18-30) ====================
    WorldEventResult CheckYoungAdultEvent(int age, int roll, System.Random r, PlayerStats stats)
    {
        if (roll > 35) return null;

        int type = r.Next(0, 100);

        if (type < 20)
        {
            // === MANIPULATIVE PARTNER ===
            bool canBreakFree = stats != null && stats.Willpower > 10;
            if (canBreakFree)
            {
                stats?.Modify(StatType.Willpower, 2);
                return new WorldEventResult
                {
                    Title = "Toxic Relationship",
                    Description = "Your partner controlled everything - who you saw, what you wore, how you spent money. One day you found the courage to leave.",
                    Survived = true,
                    Achievement = "Breaking Free",
                    WillpowerGain = 2,
                    EmotionalImpact = "Freedom never tasted so bittersweet."
                };
            }
            else
            {
                stats?.Modify(StatType.Willpower, -1);
                return new WorldEventResult
                {
                    Title = "Toxic Relationship",
                    Description = "Your partner controlled everything. You lost yourself. Friends tried to warn you but you couldn't see it.",
                    Survived = true,
                    GoldChange = -100,
                    WillpowerGain = -1,
                    EmotionalImpact = "You forgot who you used to be."
                };
            }
        }
        else if (type < 35)
        {
            // === SUDDEN LAYOFF ===
            return new WorldEventResult
            {
                Title = "Laid Off",
                Description = "Monday morning. Your badge stopped working. 'We're restructuring.' Ten years of work, gone in ten minutes.",
                Survived = true,
                GoldChange = -200,
                ResilienceGain = 1,
                EmotionalImpact = "You stared at the ceiling for three days."
            };
        }
        else if (type < 50)
        {
            // === UNEXPECTED KINDNESS ===
            stats?.Modify(StatType.Empathy, 1);
            return new WorldEventResult
            {
                Title = "A Stranger's Kindness",
                Description = "At your lowest point, a complete stranger helped you. No reason. No reward. They just... helped.",
                Survived = true,
                IsPositive = true,
                GoldChange = 50,
                EmpathyGain = 1,
                EmotionalImpact = "You promised yourself to pass it on."
            };
        }
        else if (type < 65)
        {
            // === BETRAYAL BY CLOSE FRIEND ===
            return new WorldEventResult
            {
                Title = "Betrayal",
                Description = "Your best friend went behind your back. The details don't matter. What matters is you trusted them completely.",
                Survived = true,
                ResilienceGain = 1,
                EmotionalImpact = "Trust became something you gave carefully after that."
            };
        }
        else
        {
            // === WINDFALL ===
            int amount = r.Next(100, 500);
            return new WorldEventResult
            {
                Title = "Lucky Break",
                Description = "Sometimes life just gives you a gift. An unexpected bonus, a winning ticket, a forgotten inheritance.",
                Survived = true,
                IsPositive = true,
                GoldChange = amount,
                EmotionalImpact = "You felt like the universe was smiling at you."
            };
        }
    }

    // ==================== Midlife (31-50) ====================
    WorldEventResult CheckMidlifeEvent(int age, int roll, System.Random r, PlayerStats stats)
    {
        if (roll > 30) return null;

        int type = r.Next(0, 100);

        if (type < 25)
        {
            // === NPC DEATH (close friend/family) ===
            return new WorldEventResult
            {
                Title = "Loss",
                Description = "Someone you loved is gone. The phone call came at 3 AM. You sat in the dark for hours.",
                Survived = true,
                ResilienceGain = 1,
                EmpathyGain = 1,
                EmotionalImpact = "You never got to say goodbye."
            };
        }
        else if (type < 40)
        {
            // === WORKPLACE POLITICS / BEING USED ===
            bool canNavigate = stats != null && stats.Charisma > 12;
            if (canNavigate)
            {
                stats?.Modify(StatType.Charisma, 1);
                return new WorldEventResult
                {
                    Title = "Office Politics",
                    Description = "A colleague tried to take credit for your work and turn the team against you. But you played it smart.",
                    Survived = true,
                    CharismaGain = 1,
                    GoldChange = 100,
                    EmotionalImpact = "You learned the game."
                };
            }
            else
            {
                return new WorldEventResult
                {
                    Title = "Office Politics",
                    Description = "A colleague took credit for your work. The boss believed them. You were passed over for promotion.",
                    Survived = true,
                    GoldChange = -50,
                    EmotionalImpact = "The unfairness burned."
                };
            }
        }
        else if (type < 55)
        {
            // === DIVORCE / RELATIONSHIP END ===
            return new WorldEventResult
            {
                Title = "Separation",
                Description = "It wasn't one big fight. It was a thousand small silences. One day you realized you were strangers sharing a roof.",
                Survived = true,
                GoldChange = -300,
                ResilienceGain = 1,
                EmotionalImpact = "Half the photos on the wall came down."
            };
        }
        else
        {
            // === CHILD'S ACHIEVEMENT ===
            return new WorldEventResult
            {
                Title = "Pride",
                Description = "Your child did something amazing. Standing in the audience, you cried. Not because of them, but because of everything it took to get here.",
                Survived = true,
                IsPositive = true,
                EmpathyGain = 1,
                EmotionalImpact = "This is what it was all for."
            };
        }
    }

    // ==================== Late Career (51-65) ====================
    WorldEventResult CheckLateCareerEvent(int age, int roll, System.Random r, PlayerStats stats)
    {
        if (roll > 25) return null;

        int type = r.Next(0, 100);

        if (type < 40)
        {
            // === HEALTH SCARE ===
            return new WorldEventResult
            {
                Title = "Health Scare",
                Description = "The doctor's face told you everything before they spoke. 'We need to run more tests.' The longest week of your life followed.",
                Survived = true,
                ResilienceGain = 1,
                EmotionalImpact = "You started noticing sunsets."
            };
        }
        else
        {
            // === RECONNECTION ===
            return new WorldEventResult
            {
                Title = "Old Friend Returns",
                Description = "A message from someone you hadn't heard from in 20 years. 'Remember me?' You did. You always did.",
                Survived = true,
                IsPositive = true,
                EmpathyGain = 1,
                EmotionalImpact = "Some connections never really break."
            };
        }
    }

    // ==================== Elder (66+) ====================
    WorldEventResult CheckElderEvent(int age, int roll, System.Random r, PlayerStats stats)
    {
        if (roll > 20) return null;

        int type = r.Next(0, 100);

        if (type < 50)
        {
            // === OUTLIVING FRIENDS ===
            return new WorldEventResult
            {
                Title = "Outliving",
                Description = "Another name in the obituaries you recognize. The phone rings less and less these days.",
                Survived = true,
                EmotionalImpact = "The price of a long life is watching others leave first."
            };
        }
        else
        {
            // === GRANDCHILD'S LOVE ===
            return new WorldEventResult
            {
                Title = "Unconditional",
                Description = "Your grandchild climbed into your lap and said 'I love you' for no reason at all.",
                Survived = true,
                IsPositive = true,
                EmpathyGain = 1,
                EmotionalImpact = "The simplest words. The deepest meaning."
            };
        }
    }

    /// <summary>
    /// Log event for life summary
    /// </summary>
    public void LogEvent(WorldEventResult evt)
    {
        if (evt != null)
            LifeEventLog.Add($"Age {GameManager.Instance?.Player?.CurrentAge}: {evt.Title}");
    }

    public void ResetForNewLife()
    {
        LifeEventLog.Clear();
    }
}

[Serializable]
public class WorldEventResult
{
    public string Title;
    public string Description;
    public string EmotionalImpact;      // the gut-punch line
    public string Achievement;          // if survived something hard
    public bool Survived = true;
    public bool IsPositive = false;
    public int GoldChange = 0;
    public int ResilienceGain = 0;
    public int WillpowerGain = 0;
    public int CharismaGain = 0;
    public int EmpathyGain = 0;
}