using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// Life Path / Branch system
/// The board is NOT linear - it branches based on player performance
/// Key decision points (age 18, 28, 30, etc.) split into different paths
/// Each path has different grid events, NPCs, and storylines
/// Paths can merge back at certain ages
/// </summary>
public class LifePathManager : MonoBehaviour
{
    public static LifePathManager Instance { get; private set; }

    [Header("Current Path")]
    public string CurrentPathId = "default";
    public List<string> PathHistory = new List<string>();

    // Accumulated performance scores that influence branching
    public int AcademicScore = 0;       // from school performance
    public int BusinessScore = 0;       // from work/business choices
    public int SocialScore = 0;         // from relationships
    public int CreativeScore = 0;       // from creative choices

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    /// <summary>
    /// Record performance from a grid world (called on exit)
    /// </summary>
    public void RecordPerformance(string category, int score)
    {
        switch (category)
        {
            case "academic": AcademicScore += score; break;
            case "business": BusinessScore += score; break;
            case "social": SocialScore += score; break;
            case "creative": CreativeScore += score; break;
        }
        Debug.Log($"[LifePath] {category} +{score} (total: A={AcademicScore} B={BusinessScore} S={SocialScore} C={CreativeScore})");
    }

    /// <summary>
    /// Evaluate branch at a decision point
    /// Returns the new path ID based on accumulated scores and player state
    /// </summary>
    public string EvaluateBranch(int age)
    {
        var player = GameManager.Instance.Player;
        string newPath = CurrentPathId;

        switch (age)
        {
            case 18: // Gaokao - academic vs work
                if (AcademicScore >= 15)
                    newPath = "university_elite";
                else if (AcademicScore >= 5)
                    newPath = "university_normal";
                else
                    newPath = "work_early";
                break;

            case 22: // Post-graduation
                if (CurrentPathId.StartsWith("university"))
                {
                    if (AcademicScore >= 25) newPath = "career_professional";
                    else newPath = "career_normal";
                }
                else
                {
                    if (BusinessScore >= 10) newPath = "career_entrepreneur";
                    else newPath = "career_labor";
                }
                break;

            case 28: // Life choice
                if (player.Gold >= 1000 && BusinessScore >= 15)
                    newPath = "business_owner";
                else if (CurrentPathId == "career_professional")
                    newPath = "career_senior";
                else if (CreativeScore >= 15)
                    newPath = "creative_path";
                else
                    newPath = "career_stable";
                break;

            case 35: // Midlife
                if (CurrentPathId == "business_owner")
                {
                    // Business success or failure
                    if (BusinessScore >= 25 && player.Gold >= 2000)
                        newPath = "business_success";
                    else
                        newPath = "business_failed";
                }
                break;

            case 50: // Late career
                // Paths start merging back
                if (player.Gold >= 3000) newPath = "wealthy_elder";
                else newPath = "modest_elder";
                break;

            case 60: // Retirement
                newPath = "retirement"; // all paths merge here
                break;
        }

        if (newPath != CurrentPathId)
        {
            PathHistory.Add(CurrentPathId);
            CurrentPathId = newPath;
            Debug.Log($"[LifePath] Branch at age {age}: {CurrentPathId}");
        }

        return CurrentPathId;
    }

    /// <summary>
    /// Get the grid events for current path at given age
    /// Different paths have different events at the same age
    /// </summary>
    public string GetSceneIdForAge(int age, string defaultSceneId)
    {
        // Override scene based on current path
        // This allows same age to have different scenes on different paths
        if (age >= 19 && age <= 22)
        {
            if (CurrentPathId == "work_early") return "factory";
            if (CurrentPathId == "university_elite") return "university_elite";
        }

        if (age >= 24 && age <= 30)
        {
            if (CurrentPathId == "career_entrepreneur") return "city_startup";
            if (CurrentPathId == "career_professional") return "city_office_pro";
            if (CurrentPathId == "career_labor") return "factory";
        }

        if (age >= 31 && age <= 40)
        {
            if (CurrentPathId == "business_owner") return "company_office";
            if (CurrentPathId == "business_failed") return "city_apartment_small";
            if (CurrentPathId == "business_success") return "mansion";
        }

        return defaultSceneId;
    }

    /// <summary>
    /// Check if a branch point exists at this age
    /// </summary>
    public bool IsBranchPoint(int age)
    {
        return age == 18 || age == 22 || age == 28 || age == 35 || age == 50 || age == 60;
    }

    public void Reset()
    {
        CurrentPathId = "default";
        PathHistory.Clear();
        AcademicScore = 0;
        BusinessScore = 0;
        SocialScore = 0;
        CreativeScore = 0;
    }
}