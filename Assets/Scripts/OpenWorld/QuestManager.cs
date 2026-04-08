using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// Quest system for grid worlds
/// 
/// Main quests: ~25 key ages have story-driven main quests
/// Hidden quests: randomly distributed, reward special items (Regret Pill, Time Rewind)
/// 
/// Quests have objectives (talk to NPC, reach location, make choice, find item)
/// Completing main quest triggers GridWorldTimer.CompleteMainQuest()
/// </summary>
public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance { get; private set; }

    [Header("Active Quests")]
    public Quest MainQuest;
    public List<Quest> HiddenQuests = new List<Quest>();

    public event Action<Quest> OnQuestComplete;
    public event Action<Quest> OnQuestFailed;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    /// <summary>
    /// Setup quests when entering a grid world
    /// </summary>
    public void SetupQuestsForAge(int age)
    {
        MainQuest = null;
        HiddenQuests.Clear();

        // Main quest for key ages
        MainQuest = GetMainQuestForAge(age);

        // Random hidden quest (30% chance on any grid)
        if (UnityEngine.Random.value < 0.3f)
        {
            HiddenQuests.Add(GenerateHiddenQuest(age));
        }

        if (MainQuest != null)
            Debug.Log($"[Quest] Main: {MainQuest.Title}");
        Debug.Log($"[Quest] Hidden quests: {HiddenQuests.Count}");
    }

    /// <summary>
    /// Report progress on a quest objective
    /// </summary>
    public void ReportProgress(string objectiveId)
    {
        if (MainQuest != null && !MainQuest.IsComplete)
        {
            if (MainQuest.CheckObjective(objectiveId))
            {
                Debug.Log($"[Quest] Main quest complete: {MainQuest.Title}");
                OnQuestComplete?.Invoke(MainQuest);
                if (GridWorldTimer.Instance != null)
                    GridWorldTimer.Instance.CompleteMainQuest();
            }
        }

        foreach (var q in HiddenQuests)
        {
            if (!q.IsComplete && q.CheckObjective(objectiveId))
            {
                Debug.Log($"[Quest] Hidden quest complete: {q.Title}");
                GiveHiddenReward(q);
                OnQuestComplete?.Invoke(q);
            }
        }
    }

    void GiveHiddenReward(Quest quest)
    {
        if (InventoryManager.Instance == null) return;

        switch (quest.RewardType)
        {
            case QuestRewardType.RegretPill:
                InventoryManager.Instance.AddRegretPill();
                break;
            case QuestRewardType.TimeRewind:
                InventoryManager.Instance.AddTimeRewind();
                break;
            case QuestRewardType.Gold:
                if (GameManager.Instance != null)
                    GameManager.Instance.Player.Gold += quest.GoldReward;
                break;
        }
    }

    // ==================== Main Quest Definitions ====================

    Quest GetMainQuestForAge(int age)
    {
        switch (age)
        {
            case 5: return new Quest("First Friend", "Make a friend at kindergarten", "talk_classmate", QuestRewardType.Gold, 10);
            case 7: return new Quest("First Day", "Find your classroom", "reach_classroom", QuestRewardType.Gold, 15);
            case 12: return new Quest("The Exam", "Complete the entrance exam", "finish_exam", QuestRewardType.Gold, 30);
            case 15: return new Quest("Zhongkao", "Take the high school entrance exam", "finish_exam", QuestRewardType.Gold, 50);
            case 18: return new Quest("Gaokao", "Complete the college entrance exam", "finish_gaokao", QuestRewardType.Gold, 100);
            case 20: return new Quest("Campus Life", "Join a club or make 2 friends", "social_goal", QuestRewardType.Gold, 50);
            case 22: return new Quest("Graduation Trip", "Reach the lighthouse with friends", "reach_lighthouse", QuestRewardType.Gold, 40);
            case 24: return new Quest("First Job", "Survive your first day at work", "complete_task", QuestRewardType.Gold, 100);
            case 28: return new Quest("Life Choice", "Make your career decision", "career_choice", QuestRewardType.Gold, 50);
            case 30: return new Quest("Standing Tall", "Achieve one life goal", "life_goal", QuestRewardType.Gold, 200);
            case 33: return new Quest("New Life", "Welcome your child", "hospital_event", QuestRewardType.Gold, 50);
            case 35: return new Quest("Midlife", "Find a way to cope with pressure", "cope_choice", QuestRewardType.Gold, 100);
            case 40: return new Quest("Recovery", "Get through the accident", "survive", QuestRewardType.Gold, 50);
            case 45: return new Quest("Filial Duty", "Visit your aging parents", "visit_parents", QuestRewardType.Gold, 50);
            case 50: return new Quest("Reflection", "Write down your biggest regret", "write_regret", QuestRewardType.Gold, 100);
            case 55: return new Quest("Second Wind", "Learn something new", "learn_skill", QuestRewardType.Gold, 50);
            case 60: return new Quest("Farewell", "Say goodbye to colleagues", "farewell_speech", QuestRewardType.Gold, 200);
            case 65: return new Quest("Journey", "Visit one dream destination", "reach_destination", QuestRewardType.Gold, 100);
            case 70: return new Quest("Legacy", "Teach your grandchild something", "teach_grandchild", QuestRewardType.Gold, 50);
            case 80: return new Quest("Acceptance", "Make peace with your life", "acceptance", QuestRewardType.Gold, 50);
            case 85: return new Quest("Memoir", "Finish writing your memoir", "write_memoir", QuestRewardType.Gold, 50);
            case 90: return new Quest("Sunset", "Watch the sunset one more time", "watch_sunset", QuestRewardType.Gold, 50);
            case 100: return new Quest("Century", "Blow out 100 candles", "birthday", QuestRewardType.Gold, 1000);
            default: return null; // no main quest for this age
        }
    }

    // ==================== Hidden Quest Generator ====================

    Quest GenerateHiddenQuest(int age)
    {
        string[] titles = {
            "Secret Note", "Lost Item", "Hidden Path",
            "Mysterious Stranger", "Old Photograph", "Buried Treasure",
            "Forgotten Song", "Secret Garden", "Time Capsule"
        };
        string[] objectives = {
            "find_secret", "explore_hidden", "help_mystery_npc",
            "discover_item", "reach_secret_area"
        };

        int idx = UnityEngine.Random.Range(0, titles.Length);
        string objId = objectives[UnityEngine.Random.Range(0, objectives.Length)];

        // Reward: 60% Time Rewind, 30% Regret Pill, 10% Gold
        QuestRewardType reward;
        float roll = UnityEngine.Random.value;
        if (roll < 0.3f) reward = QuestRewardType.RegretPill;
        else if (roll < 0.9f) reward = QuestRewardType.TimeRewind;
        else reward = QuestRewardType.Gold;

        int goldReward = reward == QuestRewardType.Gold ? UnityEngine.Random.Range(50, 200) : 0;

        return new Quest(titles[idx], "A hidden quest awaits...", objId, reward, goldReward, isHidden: true);
    }
}

[Serializable]
public class Quest
{
    public string Title;
    public string Description;
    public string ObjectiveId;      // what triggers completion
    public QuestRewardType RewardType;
    public int GoldReward;
    public bool IsHidden;
    public bool IsComplete = false;

    public Quest(string title, string desc, string objId, QuestRewardType reward, int gold, bool isHidden = false)
    {
        Title = title; Description = desc; ObjectiveId = objId;
        RewardType = reward; GoldReward = gold; IsHidden = isHidden;
    }

    public bool CheckObjective(string reportedId)
    {
        if (IsComplete) return false;
        if (reportedId == ObjectiveId)
        {
            IsComplete = true;
            return true;
        }
        return false;
    }
}

public enum QuestRewardType
{
    Gold,
    RegretPill,
    TimeRewind
}