using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Era/News System - The world changes around you
/// Different decades have different social events, economy, technology
/// Creates a sense of time passing and world beyond your control
/// </summary>
public class EraSystem : MonoBehaviour
{
    public static EraSystem Instance { get; private set; }

    public EraEvent CurrentEraEvent;
    public List<string> NewsHistory = new List<string>();

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    /// <summary>
    /// Generate era context for current age
    /// </summary>
    public EraEvent GenerateEraEvent(int age)
    {
        var r = new System.Random(age * 23);
        int roll = r.Next(100);

        // Major era events by decade of life
        EraEvent evt = null;

        if (age >= 5 && age <= 12)
        {
            if (roll < 20) evt = new EraEvent("Economic Boom", "The economy is thriving. Everyone seems optimistic.", EraType.Prosperity, 0.2f);
            else if (roll < 30) evt = new EraEvent("Natural Disaster", "A major earthquake/flood hit the region.", EraType.Disaster, -0.1f);
        }
        else if (age >= 13 && age <= 20)
        {
            if (roll < 15) evt = new EraEvent("Tech Revolution", "The internet changes everything. A new world opens up.", EraType.TechBoom, 0.15f);
            else if (roll < 25) evt = new EraEvent("Education Reform", "New policies change how schools work.", EraType.SocialChange, 0);
            else if (roll < 30) evt = new EraEvent("Youth Movement", "Young people take to the streets for change.", EraType.SocialChange, 0);
        }
        else if (age >= 21 && age <= 35)
        {
            if (roll < 15) evt = new EraEvent("Housing Crisis", "Housing prices skyrocket. Young people can't afford homes.", EraType.EconomicCrisis, -0.2f);
            else if (roll < 25) evt = new EraEvent("Startup Boom", "Everyone's starting a company. Money flows freely.", EraType.Prosperity, 0.3f);
            else if (roll < 30) evt = new EraEvent("Pandemic", "A virus sweeps the world. Lockdowns change daily life.", EraType.Pandemic, -0.15f);
        }
        else if (age >= 36 && age <= 50)
        {
            if (roll < 15) evt = new EraEvent("Financial Crisis", "Banks collapse. Savings evaporate overnight.", EraType.EconomicCrisis, -0.3f);
            else if (roll < 25) evt = new EraEvent("Golden Age", "Peace and prosperity. The best years for many.", EraType.Prosperity, 0.2f);
            else if (roll < 30) evt = new EraEvent("War Nearby", "Conflict erupts in a neighboring region. Refugees arrive.", EraType.War, -0.1f);
        }
        else if (age >= 51 && age <= 70)
        {
            if (roll < 15) evt = new EraEvent("AI Revolution", "Machines start doing jobs humans used to do.", EraType.TechBoom, 0.1f);
            else if (roll < 25) evt = new EraEvent("Climate Crisis", "Extreme weather becomes the new normal.", EraType.Disaster, -0.1f);
            else if (roll < 30) evt = new EraEvent("Medical Breakthrough", "A new treatment extends life expectancy.", EraType.TechBoom, 0.1f);
        }
        else if (age > 70)
        {
            if (roll < 20) evt = new EraEvent("Peaceful Era", "The world seems calmer now. Or maybe you just care less.", EraType.Prosperity, 0.05f);
            else if (roll < 30) evt = new EraEvent("New Generation", "The young people are so different. You barely recognize the world.", EraType.SocialChange, 0);
        }

        if (evt != null)
        {
            CurrentEraEvent = evt;
            NewsHistory.Add($"Age {age}: {evt.Title}");
            Debug.Log($"[Era] {evt.Title}: {evt.Description}");
        }

        return evt;
    }

    /// <summary>
    /// Apply era effects to player economy
    /// </summary>
    public int GetGoldModifier(int baseGold)
    {
        if (CurrentEraEvent == null) return 0;
        return Mathf.RoundToInt(baseGold * CurrentEraEvent.EconomyModifier);
    }

    public void ResetForNewLife()
    {
        CurrentEraEvent = null;
        NewsHistory.Clear();
    }
}

[System.Serializable]
public class EraEvent
{
    public string Title;
    public string Description;
    public EraType Type;
    public float EconomyModifier; // -0.3 to +0.3

    public EraEvent(string t, string d, EraType type, float econ)
    { Title = t; Description = d; Type = type; EconomyModifier = econ; }
}

public enum EraType
{
    Prosperity,
    EconomicCrisis,
    War,
    Pandemic,
    TechBoom,
    Disaster,
    SocialChange
}