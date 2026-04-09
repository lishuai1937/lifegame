using System;
using System.Collections.Generic;

/// <summary>
/// Education & Skill System
/// Skills learned through school, work, and life events
/// Skills affect: career income, event options, NPC interactions
/// </summary>
[Serializable]
public class SkillSystem
{
    public EducationLevel Education = EducationLevel.None;
    public List<Skill> Skills = new List<Skill>();

    public void SetEducation(EducationLevel level) { Education = level; }

    public void LearnSkill(string name, SkillCategory category, int level = 1)
    {
        var existing = Skills.Find(s => s.Name == name);
        if (existing != null)
        {
            existing.Level = Math.Min(10, existing.Level + 1);
        }
        else
        {
            Skills.Add(new Skill { Name = name, Category = category, Level = level });
        }
    }

    public int GetSkillLevel(string name)
    {
        var s = Skills.Find(sk => sk.Name == name);
        return s != null ? s.Level : 0;
    }

    /// <summary>
    /// Income multiplier from education + skills
    /// </summary>
    public float GetIncomeMultiplier()
    {
        float mult = Education switch
        {
            EducationLevel.None => 0.6f,
            EducationLevel.HighSchool => 0.8f,
            EducationLevel.College => 1.0f,
            EducationLevel.University => 1.3f,
            EducationLevel.Masters => 1.6f,
            EducationLevel.PhD => 2.0f,
            _ => 1.0f
        };
        // Bonus from skill count
        mult += Skills.Count * 0.02f;
        return mult;
    }

    /// <summary>
    /// Auto-learn skills based on education level
    /// </summary>
    public void OnGraduate(EducationLevel level)
    {
        Education = level;
        switch (level)
        {
            case EducationLevel.HighSchool:
                LearnSkill("Basic Math", SkillCategory.Academic);
                LearnSkill("Writing", SkillCategory.Academic);
                break;
            case EducationLevel.University:
                LearnSkill("Critical Thinking", SkillCategory.Academic);
                LearnSkill("Research", SkillCategory.Academic);
                LearnSkill("Presentation", SkillCategory.Social);
                break;
            case EducationLevel.Masters:
            case EducationLevel.PhD:
                LearnSkill("Expertise", SkillCategory.Academic, 3);
                LearnSkill("Leadership", SkillCategory.Social);
                break;
        }
    }
}

[Serializable]
public class Skill
{
    public string Name;
    public SkillCategory Category;
    public int Level; // 1-10
}

public enum SkillCategory { Academic, Social, Technical, Creative, Physical, Life }
public enum EducationLevel { None, HighSchool, College, University, Masters, PhD }