using System;
using System.Collections.Generic;

/// <summary>
/// Last Wishes System - Available from age 70+
/// Player creates a bucket list of things to do before death
/// Completing wishes gives achievements, karma, and emotional closure
/// Uncompleted wishes become regrets on death screen
/// </summary>
[Serializable]
public class LastWishSystem
{
    public bool IsUnlocked = false;     // unlocks at 70
    public List<LastWish> Wishes = new List<LastWish>();

    // Pre-defined wish templates
    public static readonly WishTemplate[] Templates = {
        new WishTemplate("See the ocean one more time", WishCategory.Travel, "You stood at the shore. The waves sounded the same as when you were young."),
        new WishTemplate("Reconcile with an old enemy", WishCategory.Relationship, "You called them. The silence was long. Then they said: 'I'm sorry too.'"),
        new WishTemplate("Write a letter to your younger self", WishCategory.Reflection, "Dear me at 18: Don't rush. The best parts are the ones you almost missed."),
        new WishTemplate("Visit your childhood home", WishCategory.Nostalgia, "The house looked smaller than you remembered. But the tree was still there."),
        new WishTemplate("Tell someone you love them", WishCategory.Relationship, "You said it. They cried. You cried. It was enough."),
        new WishTemplate("Watch one more sunrise", WishCategory.Simple, "4:47 AM. The sky turned gold. You whispered: 'Thank you.'"),
        new WishTemplate("Teach something to a grandchild", WishCategory.Legacy, "They listened with wide eyes. Someday they'll teach their own children."),
        new WishTemplate("Eat your favorite childhood meal", WishCategory.Nostalgia, "It didn't taste the same. But the memory did."),
        new WishTemplate("Forgive yourself", WishCategory.Reflection, "You closed your eyes and let it go. All of it."),
        new WishTemplate("Dance one more time", WishCategory.Simple, "Your knees hurt. You didn't care. The music was beautiful."),
        new WishTemplate("Finish something you started years ago", WishCategory.Legacy, "It took decades. But you finished it. Finally."),
        new WishTemplate("Say goodbye properly", WishCategory.Relationship, "This time, you didn't leave anything unsaid."),
    };

    /// <summary>
    /// Unlock at age 70, player picks 3 wishes
    /// </summary>
    public void Unlock()
    {
        IsUnlocked = true;
    }

    public void AddWish(string text, WishCategory category, string completionText)
    {
        Wishes.Add(new LastWish { Text = text, Category = category, CompletionText = completionText });
    }

    public void CompleteWish(int index)
    {
        if (index >= 0 && index < Wishes.Count)
            Wishes[index].IsCompleted = true;
    }

    public int GetCompletedCount()
    {
        int count = 0;
        foreach (var w in Wishes) if (w.IsCompleted) count++;
        return count;
    }

    public List<LastWish> GetUncompletedWishes()
    {
        return Wishes.FindAll(w => !w.IsCompleted);
    }

    /// <summary>
    /// Death screen text based on wish completion
    /// </summary>
    public string GetDeathReflection()
    {
        int completed = GetCompletedCount();
        int total = Wishes.Count;
        if (total == 0) return "You never made a bucket list. Maybe that's okay.";
        if (completed == total) return "You completed every last wish. No regrets.";
        if (completed > total / 2) return $"You fulfilled {completed} of {total} wishes. Close enough.";
        if (completed > 0) return $"Only {completed} of {total} wishes came true. So much left undone.";
        return "None of your wishes came true. The list sits in a drawer, untouched.";
    }
}

[Serializable]
public class LastWish
{
    public string Text;
    public WishCategory Category;
    public string CompletionText;
    public bool IsCompleted = false;
}

[Serializable]
public class WishTemplate
{
    public string Text;
    public WishCategory Category;
    public string CompletionText;
    public WishTemplate(string t, WishCategory c, string ct) { Text = t; Category = c; CompletionText = ct; }
}

public enum WishCategory { Travel, Relationship, Reflection, Nostalgia, Simple, Legacy }