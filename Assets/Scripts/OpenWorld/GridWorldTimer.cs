using UnityEngine;
using System;

/// <summary>
/// Grid World Timer - manages forced exit from grid worlds
/// Each grid world has a time limit. When it expires, player is forced out.
/// Time Rewind item can extend the timer.
/// Main quest completion can also trigger exit.
/// </summary>
public class GridWorldTimer : MonoBehaviour
{
    public static GridWorldTimer Instance { get; private set; }

    [Header("Timer")]
    public float BaseTime = 120f;       // default 2 minutes per grid world
    public float RemainingTime;
    public bool IsRunning = false;

    [Header("Events")]
    public event Action OnTimeWarning;  // 30 seconds left
    public event Action OnTimeUp;       // forced exit
    public event Action OnQuestComplete;// main quest done

    private bool warningFired = false;
    private bool mainQuestDone = false;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Update()
    {
        if (!IsRunning) return;

        RemainingTime -= Time.deltaTime;

        // Warning at 30 seconds
        if (!warningFired && RemainingTime <= 30f)
        {
            warningFired = true;
            OnTimeWarning?.Invoke();
            Debug.Log("[GridTimer] 30 seconds remaining!");
        }

        // Time's up
        if (RemainingTime <= 0)
        {
            RemainingTime = 0;
            IsRunning = false;
            OnTimeUp?.Invoke();
            Debug.Log("[GridTimer] Time's up! Forced exit.");
            ForceExit();
        }
    }

    /// <summary>
    /// Start timer for a grid world. Time varies by age phase.
    /// </summary>
    public void StartTimer(int age)
    {
        // Younger ages = shorter worlds, older = longer reflection
        if (age <= 12) BaseTime = 60f;
        else if (age <= 17) BaseTime = 90f;
        else if (age <= 30) BaseTime = 120f;
        else if (age <= 50) BaseTime = 150f;
        else if (age <= 65) BaseTime = 120f;
        else BaseTime = 90f;

        RemainingTime = BaseTime;
        IsRunning = true;
        warningFired = false;
        mainQuestDone = false;
        Debug.Log($"[GridTimer] Started: {BaseTime}s for age {age}");
    }

    /// <summary>
    /// Extend time (Time Rewind item)
    /// </summary>
    public void ExtendTime(float seconds)
    {
        RemainingTime += seconds;
        warningFired = false; // reset warning
        Debug.Log($"[GridTimer] Extended by {seconds}s. Remaining: {RemainingTime}s");
    }

    /// <summary>
    /// Main quest completed - can trigger exit or give bonus time
    /// </summary>
    public void CompleteMainQuest()
    {
        mainQuestDone = true;
        OnQuestComplete?.Invoke();
        Debug.Log("[GridTimer] Main quest complete!");
        // Give 30 seconds bonus to explore after quest
        RemainingTime = Mathf.Min(RemainingTime, 30f);
        if (RemainingTime < 30f) RemainingTime = 30f;
    }

    /// <summary>
    /// Force immediate exit (triggered by events, death, etc.)
    /// </summary>
    public void ForceExit()
    {
        IsRunning = false;

        // Settle karma
        int karma = 0;
        if (KarmaTracker.Instance != null)
            karma = KarmaTracker.Instance.SettleAndReset();

        var result = new GridWorldResult
        {
            GoldEarned = 0,
            KarmaChange = karma,
            IsDead = false
        };

        if (GameManager.Instance != null)
            GameManager.Instance.ExitGridWorld(result);
    }

    public void StopTimer()
    {
        IsRunning = false;
    }

    public float GetTimePercent()
    {
        if (BaseTime <= 0) return 0;
        return Mathf.Clamp01(RemainingTime / BaseTime);
    }
}