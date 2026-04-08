using UnityEngine;

/// <summary>
/// UI管理器
/// 负责各面板的显示/隐藏切换
/// </summary>
public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("UI面板")]
    public GameObject MainMenuPanel;
    public GameObject BoardGamePanel;
    public GameObject DicePanel;
    public GameObject SpeedChoicePanel;
    public GameObject PlayerInfoPanel;
    public GameObject EventDialogPanel;
    public GameObject DeathPanel;
    public GameObject CGPanel;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    /// <summary>
    /// 根据游戏状态切换UI
    /// </summary>
    public void UpdateUI(GameState state)
    {
        HideAll();

        switch (state)
        {
            case GameState.MainMenu:
                SetActive(MainMenuPanel, true);
                break;
            case GameState.BoardGame:
                SetActive(BoardGamePanel, true);
                SetActive(DicePanel, true);
                SetActive(PlayerInfoPanel, true);
                break;
            case GameState.OpenWorld:
                SetActive(PlayerInfoPanel, true);
                break;
            case GameState.Death:
                SetActive(DeathPanel, true);
                break;
            case GameState.CG:
                SetActive(CGPanel, true);
                break;
        }
    }

    /// <summary>
    /// 显示事件对话框
    /// </summary>
    public void ShowEventDialog(string title, string description, System.Action onConfirm)
    {
        SetActive(EventDialogPanel, true);
        // TODO: 设置对话框内容并绑定回调
        Debug.Log($"[UI] 事件对话: {title} - {description}");
    }

    /// <summary>
    /// 显示速度选择面板
    /// </summary>
    public void ShowSpeedChoice(System.Action<DiceSpeed> onChoice)
    {
        SetActive(SpeedChoicePanel, true);
        // TODO: 绑定按钮回调
    }

    private void HideAll()
    {
        SetActive(MainMenuPanel, false);
        SetActive(BoardGamePanel, false);
        SetActive(DicePanel, false);
        SetActive(SpeedChoicePanel, false);
        SetActive(PlayerInfoPanel, false);
        SetActive(EventDialogPanel, false);
        SetActive(DeathPanel, false);
        SetActive(CGPanel, false);
    }

    private void SetActive(GameObject panel, bool active)
    {
        if (panel != null) panel.SetActive(active);
    }
}
