using UnityEngine;

/// <summary>
/// Weather & Season System
/// Affects: scene atmosphere, NPC behavior, event probabilities, mood
/// </summary>
public class WeatherSystem : MonoBehaviour
{
    public static WeatherSystem Instance { get; private set; }

    public Weather CurrentWeather = Weather.Sunny;
    public Season CurrentSeason = Season.Spring;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    /// <summary>
    /// Generate weather for a grid world based on age and luck
    /// </summary>
    public void GenerateWeather(int age, int luck)
    {
        var r = new System.Random(age * 13 + luck);

        // Season from age (each year = cycle through seasons)
        CurrentSeason = (Season)(age % 4);

        // Weather based on season + randomness
        int roll = r.Next(100);
        CurrentWeather = CurrentSeason switch
        {
            Season.Spring => roll < 40 ? Weather.Sunny : roll < 70 ? Weather.Cloudy : roll < 90 ? Weather.Rainy : Weather.Foggy,
            Season.Summer => roll < 50 ? Weather.Sunny : roll < 70 ? Weather.Hot : roll < 85 ? Weather.Stormy : Weather.Rainy,
            Season.Autumn => roll < 30 ? Weather.Sunny : roll < 60 ? Weather.Cloudy : roll < 80 ? Weather.Windy : Weather.Rainy,
            Season.Winter => roll < 20 ? Weather.Sunny : roll < 40 ? Weather.Cloudy : roll < 70 ? Weather.Snowy : Weather.Cold,
            _ => Weather.Sunny
        };

        // Luck affects weather slightly
        if (luck > 15 && CurrentWeather == Weather.Stormy) CurrentWeather = Weather.Cloudy;
        if (luck < 3 && CurrentWeather == Weather.Sunny) CurrentWeather = Weather.Rainy;

        Debug.Log($"[Weather] {CurrentSeason}, {CurrentWeather}");
    }

    /// <summary>
    /// Mental health modifier from weather
    /// </summary>
    public int GetMoodModifier()
    {
        return CurrentWeather switch
        {
            Weather.Sunny => 2,
            Weather.Hot => -1,
            Weather.Rainy => -1,
            Weather.Stormy => -3,
            Weather.Snowy => 1,     // cozy
            Weather.Cold => -2,
            Weather.Foggy => -1,
            Weather.Windy => 0,
            Weather.Cloudy => 0,
            _ => 0
        };
    }

    /// <summary>
    /// NPC behavior modifier (introverts stay inside in bad weather)
    /// </summary>
    public float GetNPCActivityModifier()
    {
        return CurrentWeather switch
        {
            Weather.Sunny => 1.0f,      // all NPCs active
            Weather.Rainy => 0.6f,      // fewer NPCs outside
            Weather.Stormy => 0.3f,     // most stay inside
            Weather.Snowy => 0.5f,
            Weather.Cold => 0.5f,
            Weather.Hot => 0.7f,
            _ => 0.8f
        };
    }

    /// <summary>
    /// Event probability modifier
    /// </summary>
    public float GetEventModifier()
    {
        return CurrentWeather switch
        {
            Weather.Stormy => 1.3f,     // more accidents in storms
            Weather.Foggy => 1.2f,
            Weather.Sunny => 0.9f,      // fewer bad events
            _ => 1.0f
        };
    }
}

public enum Weather { Sunny, Cloudy, Rainy, Stormy, Snowy, Cold, Hot, Foggy, Windy }
public enum Season { Spring, Summer, Autumn, Winter }