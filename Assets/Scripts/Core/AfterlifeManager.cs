using UnityEngine;

/// <summary>
/// Afterlife Manager - Handles Heaven and Hell gameplay
/// 
/// After death + karma judgment, player enters either Heaven or Hell
/// Each is a small open world with its own events and atmosphere
/// Player can explore, interact, then choose to reincarnate
/// 
/// Heaven: peaceful, bright, reward-based
/// Hell: dark, harsh, redemption-based
/// </summary>
public class AfterlifeManager : MonoBehaviour
{
    public static AfterlifeManager Instance { get; private set; }

    [Header("Afterlife Scenes")]
    public GameObject HeavenWorld;      // Heaven scene root (generated or pre-built)
    public GameObject HellWorld;        // Hell scene root
    public GameObject BoardWorld;       // Reference to board (to hide during afterlife)

    [Header("Cameras")]
    public Camera AfterlifeCamera;      // 3D camera for afterlife exploration

    [Header("State")]
    public WorldRealm CurrentRealm = WorldRealm.Mortal;
    public bool IsInAfterlife = false;
    public float TimeInAfterlife = 0f;
    public float MaxAfterlifeTime = 120f; // 2 minutes before forced reincarnation prompt

    private int afterlifeKarmaBonus = 0; // bonus karma earned in afterlife, carries to next life

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Update()
    {
        if (!IsInAfterlife) return;

        TimeInAfterlife += Time.deltaTime;

        // After max time, prompt reincarnation
        if (TimeInAfterlife >= MaxAfterlifeTime)
        {
            PromptReincarnation();
        }

        // ESC to leave afterlife early
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            PromptReincarnation();
        }
    }

    /// <summary>
    /// Enter afterlife after death judgment
    /// </summary>
    public void EnterAfterlife(WorldRealm realm)
    {
        CurrentRealm = realm;
        IsInAfterlife = true;
        TimeInAfterlife = 0f;
        afterlifeKarmaBonus = 0;

        // Hide board, show afterlife
        if (BoardWorld != null) BoardWorld.SetActive(false);

        if (realm == WorldRealm.Heaven)
        {
            if (HeavenWorld != null) HeavenWorld.SetActive(true);
            if (HellWorld != null) HellWorld.SetActive(false);
            Debug.Log("[Afterlife] Entered Heaven");
        }
        else
        {
            if (HeavenWorld != null) HeavenWorld.SetActive(false);
            if (HellWorld != null) HellWorld.SetActive(true);
            Debug.Log("[Afterlife] Entered Hell");
        }

        // Enable afterlife camera
        if (AfterlifeCamera != null) AfterlifeCamera.enabled = true;
        var mainCam = Camera.main;
        if (mainCam != null && mainCam != AfterlifeCamera) mainCam.enabled = false;
    }

    /// <summary>
    /// Called by afterlife events (good deeds in hell = redemption, etc.)
    /// </summary>
    public void AddAfterlifeKarma(int amount)
    {
        afterlifeKarmaBonus += amount;
        Debug.Log($"[Afterlife] Karma bonus: {afterlifeKarmaBonus}");
    }

    /// <summary>
    /// Show reincarnation prompt
    /// </summary>
    public void PromptReincarnation()
    {
        IsInAfterlife = false;

        // Apply afterlife karma bonus to player before reincarnation
        if (GameManager.Instance != null)
            GameManager.Instance.Player.KarmaValue += afterlifeKarmaBonus;

        // Disable afterlife
        if (HeavenWorld != null) HeavenWorld.SetActive(false);
        if (HellWorld != null) HellWorld.SetActive(false);
        if (AfterlifeCamera != null) AfterlifeCamera.enabled = false;

        // Show death/reincarnation UI
        GameManager.Instance.ChangeState(GameState.Reincarnation);
        Debug.Log("[Afterlife] Reincarnation prompt");
    }

    /// <summary>
    /// Get a description of the afterlife experience (for UI)
    /// </summary>
    public string GetAfterlifeSummary()
    {
        if (CurrentRealm == WorldRealm.Heaven)
        {
            if (afterlifeKarmaBonus > 0)
                return "In heaven, your kindness continued to shine.";
            return "You rested peacefully among the clouds.";
        }
        else
        {
            if (afterlifeKarmaBonus > 0)
                return "Even in hell, you found redemption through good deeds.";
            return "The flames tested your resolve.";
        }
    }
}