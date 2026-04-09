using System;
using System.Collections.Generic;

/// <summary>
/// Diary/Memory System - Auto-records life events
/// Player can review their life story at any time
/// In old age, reviewing diary triggers nostalgia events
/// On death, diary becomes the "life summary"
/// On reincarnation, key entries become soul memories
/// </summary>
[Serializable]
public class DiarySystem
{
    public List<DiaryEntry> Entries = new List<DiaryEntry>();

    public void AddEntry(int age, string title, string content, DiaryCategory category)
    {
        Entries.Add(new DiaryEntry
        {
            Age = age,
            Title = title,
            Content = content,
            Category = category
        });
    }

    // Convenience methods
    public void RecordMilestone(int age, string text) { AddEntry(age, "Milestone", text, DiaryCategory.Milestone); }
    public void RecordRelationship(int age, string text) { AddEntry(age, "Relationship", text, DiaryCategory.Relationship); }
    public void RecordAchievement(int age, string text) { AddEntry(age, "Achievement", text, DiaryCategory.Achievement); }
    public void RecordHardship(int age, string text) { AddEntry(age, "Hardship", text, DiaryCategory.Hardship); }
    public void RecordJoy(int age, string text) { AddEntry(age, "Joy", text, DiaryCategory.Joy); }
    public void RecordLoss(int age, string text) { AddEntry(age, "Loss", text, DiaryCategory.Loss); }
    public void RecordDream(int age, string text) { AddEntry(age, "Dream", text, DiaryCategory.Dream); }

    /// <summary>
    /// Get entries for a specific age range (for reviewing)
    /// </summary>
    public List<DiaryEntry> GetEntriesForPhase(int startAge, int endAge)
    {
        return Entries.FindAll(e => e.Age >= startAge && e.Age <= endAge);
    }

    /// <summary>
    /// Get the most emotional entries (for death summary)
    /// </summary>
    public List<DiaryEntry> GetHighlights(int maxCount = 10)
    {
        // Prioritize: milestones, relationships, hardships
        var highlights = new List<DiaryEntry>();
        foreach (var e in Entries)
        {
            if (e.Category == DiaryCategory.Milestone || e.Category == DiaryCategory.Relationship ||
                e.Category == DiaryCategory.Loss || e.Category == DiaryCategory.Achievement)
                highlights.Add(e);
        }
        if (highlights.Count > maxCount)
            highlights = highlights.GetRange(0, maxCount);
        return highlights;
    }

    /// <summary>
    /// Convert key diary entries to soul memories for reincarnation
    /// </summary>
    public List<string> ToSoulMemories()
    {
        var memories = new List<string>();
        foreach (var e in Entries)
        {
            if (e.Category == DiaryCategory.Relationship || e.Category == DiaryCategory.Loss)
                memories.Add(e.Content);
        }
        return memories;
    }
}

[Serializable]
public class DiaryEntry
{
    public int Age;
    public string Title;
    public string Content;
    public DiaryCategory Category;
}

public enum DiaryCategory
{
    Milestone,      // graduation, marriage, retirement
    Relationship,   // met someone, fell in love, broke up
    Achievement,    // career success, dream fulfilled
    Hardship,       // bullying, layoff, illness
    Joy,            // happy moments
    Loss,           // death of loved one, divorce
    Dream           // dream chosen/changed
}