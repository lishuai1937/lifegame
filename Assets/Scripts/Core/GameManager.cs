using UnityEngine;

/// <summary>
/// GameManager - Singleton, state machine, global data
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("State")]
    public GameState CurrentState = GameState.MainMenu;

    [Header("Player Data")]
    public PlayerData Player;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        Player = new PlayerData();
    }

    public void ChangeState(GameState newState)
    {
        var old = CurrentState;
        CurrentState = newState;
        Debug.Log($"[GameManager] {old} -> {newState}");

        // Auto-update UI if available
        if (UIManager.Instance != null)
            UIManager.Instance.UpdateUI(newState);

        // Auto-switch audio if available
        if (AudioManager.Instance != null && newState == GameState.BoardGame)
            AudioManager.Instance.PlayBGMForPhase(Player.GetAgePhase());
    }

    public void EnterGridWorld(int age)
    {
        Player.CurrentAge = age;
        ChangeState(GameState.OpenWorld);
        Debug.Log($"[GameManager] Enter grid world: age {age}");
    }

    public void ExitGridWorld(GridWorldResult result)
    {
        Player.Gold += result.GoldEarned;
        Player.KarmaValue += result.KarmaChange;
        ChangeState(result.IsDead ? GameState.Death : GameState.BoardGame);
    }
}