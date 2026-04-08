using UnityEngine;

/// <summary>
/// Day-Night + Season cycle for grid worlds
/// 
/// One grid = one year of life
/// Player experiences accelerated time: ~30-40s per day/night cycle
/// Seasons change across the grid world visit duration:
///   Enter = Spring morning -> Summer -> Autumn -> Winter night = Exit
/// 
/// Controls: directional light rotation, color, intensity, sky color
/// </summary>
public class TimeOfDaySystem : MonoBehaviour
{
    public static TimeOfDaySystem Instance { get; private set; }

    [Header("References")]
    public Light SunLight;
    public Camera SceneCamera; // to change background/sky color

    [Header("Cycle Settings")]
    public float DayDuration = 35f;         // seconds per full day-night cycle
    public float TotalWorldDuration = 120f; // total time in grid world (synced with GridWorldTimer)

    [Header("Runtime")]
    public float TimeElapsed = 0f;
    public float DayProgress = 0f;          // 0-1 within current day
    public float YearProgress = 0f;         // 0-1 across entire visit (spring->winter)
    public int DayCount = 0;
    public bool IsRunning = false;

    // Season names for UI
    public string CurrentSeason => YearProgress < 0.25f ? "Spring" : YearProgress < 0.5f ? "Summer" : YearProgress < 0.75f ? "Autumn" : "Winter";
    public string CurrentTimeOfDay => DayProgress < 0.25f ? "Morning" : DayProgress < 0.5f ? "Noon" : DayProgress < 0.75f ? "Evening" : "Night";

    // Color presets
    readonly Color springLight = new Color(1f, 0.92f, 0.75f);
    readonly Color summerLight = new Color(1f, 0.98f, 0.9f);
    readonly Color autumnLight = new Color(1f, 0.8f, 0.55f);
    readonly Color winterLight = new Color(0.7f, 0.75f, 0.9f);

    readonly Color morningSky = new Color(0.5f, 0.7f, 0.9f);
    readonly Color noonSky = new Color(0.4f, 0.6f, 0.95f);
    readonly Color eveningSky = new Color(0.8f, 0.45f, 0.2f);
    readonly Color nightSky = new Color(0.05f, 0.05f, 0.15f);

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Update()
    {
        if (!IsRunning) return;

        TimeElapsed += Time.deltaTime;
        YearProgress = Mathf.Clamp01(TimeElapsed / TotalWorldDuration);

        // Day cycle
        float dayTime = TimeElapsed % DayDuration;
        DayProgress = dayTime / DayDuration;
        DayCount = Mathf.FloorToInt(TimeElapsed / DayDuration);

        UpdateLighting();
        UpdateSkyColor();
    }

    /// <summary>
    /// Start the time cycle when entering a grid world
    /// </summary>
    public void StartCycle(float worldDuration)
    {
        TotalWorldDuration = worldDuration;
        TimeElapsed = 0;
        DayCount = 0;
        IsRunning = true;

        // Try to find sun light if not assigned
        if (SunLight == null)
        {
            var lights = FindObjectsOfType<Light>();
            foreach (var l in lights)
                if (l.type == LightType.Directional) { SunLight = l; break; }
        }

        if (SceneCamera == null)
            SceneCamera = Camera.main;

        Debug.Log($"[TimeOfDay] Started. Duration: {worldDuration}s, Day cycle: {DayDuration}s");
    }

    public void StopCycle()
    {
        IsRunning = false;
    }

    void UpdateLighting()
    {
        if (SunLight == null) return;

        // Sun rotation: morning(east) -> noon(top) -> evening(west) -> night(below)
        float sunAngle = DayProgress * 360f - 90f; // -90 = sunrise, 90 = noon, 270 = sunset
        SunLight.transform.rotation = Quaternion.Euler(sunAngle, -30f, 0);

        // Intensity: bright at noon, dim at night
        float intensity;
        if (DayProgress < 0.25f) // morning
            intensity = Mathf.Lerp(0.3f, 1.0f, DayProgress * 4f);
        else if (DayProgress < 0.5f) // noon
            intensity = 1.2f;
        else if (DayProgress < 0.75f) // evening
            intensity = Mathf.Lerp(1.0f, 0.4f, (DayProgress - 0.5f) * 4f);
        else // night
            intensity = Mathf.Lerp(0.4f, 0.1f, (DayProgress - 0.75f) * 4f);

        SunLight.intensity = intensity;

        // Light color: blend between season colors
        Color seasonColor;
        if (YearProgress < 0.25f)
            seasonColor = Color.Lerp(springLight, summerLight, YearProgress * 4f);
        else if (YearProgress < 0.5f)
            seasonColor = Color.Lerp(summerLight, autumnLight, (YearProgress - 0.25f) * 4f);
        else if (YearProgress < 0.75f)
            seasonColor = Color.Lerp(autumnLight, winterLight, (YearProgress - 0.5f) * 4f);
        else
            seasonColor = winterLight;

        SunLight.color = seasonColor;
    }

    void UpdateSkyColor()
    {
        if (SceneCamera == null) return;

        // Sky color based on time of day
        Color skyColor;
        if (DayProgress < 0.25f)
            skyColor = Color.Lerp(nightSky, morningSky, DayProgress * 4f);
        else if (DayProgress < 0.5f)
            skyColor = Color.Lerp(morningSky, noonSky, (DayProgress - 0.25f) * 4f);
        else if (DayProgress < 0.75f)
            skyColor = Color.Lerp(noonSky, eveningSky, (DayProgress - 0.5f) * 4f);
        else
            skyColor = Color.Lerp(eveningSky, nightSky, (DayProgress - 0.75f) * 4f);

        // Tint by season
        if (YearProgress > 0.5f && YearProgress < 0.75f)
            skyColor = Color.Lerp(skyColor, new Color(0.6f, 0.4f, 0.2f), 0.2f); // autumn tint
        else if (YearProgress >= 0.75f)
            skyColor = Color.Lerp(skyColor, new Color(0.15f, 0.15f, 0.25f), 0.3f); // winter darken

        SceneCamera.backgroundColor = skyColor;
    }
}