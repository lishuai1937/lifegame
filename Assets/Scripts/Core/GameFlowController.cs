using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// GameFlow - Wires UI to game logic
/// Karma is HIDDEN during gameplay, only revealed at death
/// </summary>
public class GameFlowController : MonoBehaviour
{
    [Header("References")]
    public BoardManager Board;
    public GameObject PlayerObj;
    public GameObject BoardObj;
    public Transform PlayerToken;
    public Transform DiceVisual;

    Text diceResultText, eventTitle, eventDesc, deathAge, deathGold, deathKarma, realmText;
    Text nameText, ageText, goldText, karmaText;
    GameObject choice1Btn, choice2Btn, continueBtn;
    float diceSpinTimer;
    bool isSpinning;
    int diceResult;

    void Start()
    {
        CacheUI();
        BindButtons();
        ShowMainMenu();
    }

    void Update()
    {
        if (isSpinning)
        {
            diceSpinTimer -= Time.deltaTime;
            if (DiceVisual != null) DiceVisual.Rotate(Vector3.forward * 720 * Time.deltaTime);
            if (diceSpinTimer <= 0) FinishRoll();
        }
        if (GameManager.Instance != null &&
            GameManager.Instance.CurrentState == GameState.BoardGame && !isSpinning)
        {
            if (Input.GetKeyDown(KeyCode.Space)) OnRollDice();
        }
    }

    void CacheUI()
    {
        var c = GameObject.Find("UICanvas");
        if (c == null) return;
        diceResultText = FindText(c, "DiceResultText");
        eventTitle = FindText(c, "EventTitle");
        eventDesc = FindText(c, "EventDesc");
        deathAge = FindText(c, "DeathAge");
        deathGold = FindText(c, "DeathGold");
        deathKarma = FindText(c, "DeathKarma");
        realmText = FindText(c, "RealmText");
        nameText = FindText(c, "NameText");
        ageText = FindText(c, "AgeText");
        goldText = FindText(c, "GoldText");
        karmaText = FindText(c, "KarmaText");
        choice1Btn = FindGO(c, "Choice1Btn");
        choice2Btn = FindGO(c, "Choice2Btn");
        continueBtn = FindGO(c, "ContinueBtn");
    }

    void BindButtons()
    {
        var c = GameObject.Find("UICanvas");
        if (c == null) return;
        Bind(c, "StartButton", OnStart);
        Bind(c, "RollDiceBtn", OnRollDice);
        Bind(c, "SlowBtn", () => OnSpeed(DiceSpeed.Slow));
        Bind(c, "FastBtn", () => OnSpeed(DiceSpeed.Fast));
        Bind(c, "ReincarnateBtn", OnReincarnate);
        Bind(c, "MainMenuBtn", ShowMainMenu);
        Bind(c, "ContinueBtn", OnContinue);
    }

    void Bind(GameObject root, string name, UnityEngine.Events.UnityAction act)
    {
        var o = FindGO(root, name);
        if (o != null) { var b = o.GetComponent<Button>(); if (b != null) b.onClick.AddListener(act); }
    }

    void ShowMainMenu()
    {
        GameManager.Instance.ChangeState(GameState.MainMenu);
        UIManager.Instance.UpdateUI(GameState.MainMenu);
        if (PlayerObj != null) PlayerObj.SetActive(false);
        if (BoardObj != null) BoardObj.SetActive(true);
        var ow = GameObject.Find("OpenWorld");
        if (ow != null) ow.SetActive(false);
    }

    void OnStart()
    {
        GameManager.Instance.Player = new PlayerData { PlayerName = "Player" };
        if (Board != null) Board.ResetBoard();
        GameManager.Instance.ChangeState(GameState.BoardGame);
        UIManager.Instance.UpdateUI(GameState.BoardGame);
        UIManager.Instance.ShowSpeedChoice(null);
        UpdateHUD();
    }

    void OnSpeed(DiceSpeed s)
    {
        GameManager.Instance.Player.CurrentDiceSpeed = s;
        var sp = GameObject.Find("SpeedChoicePanel");
        if (sp != null) sp.SetActive(false);
        UIManager.Instance.UpdateUI(GameState.BoardGame);
        UpdateHUD();
    }

    void OnRollDice()
    {
        if (isSpinning) return;
        diceResult = DiceSystem.Roll(GameManager.Instance.Player.CurrentDiceSpeed);
        isSpinning = true;
        diceSpinTimer = 0.8f;
        if (diceResultText != null) diceResultText.text = "...";
    }

    void FinishRoll()
    {
        isSpinning = false;
        if (DiceVisual != null) DiceVisual.rotation = Quaternion.identity;
        if (diceResultText != null) diceResultText.text = diceResult.ToString();

        if (Board != null)
        {
            var player = GameManager.Instance.Player;
            int oldAge = player.CurrentAge;
            int newAge = Mathf.Min(oldAge + diceResult, 100);
            player.CurrentAge = newAge;
            Board.CurrentGridIndex = newAge;

            if (PlayerToken != null)
            {
                Vector3 target = Board.GetCellPosition(newAge - 1);
                target.y += 1f; target.z = -0.5f;
                PlayerToken.position = target;
                if (DiceVisual != null) DiceVisual.position = target + new Vector3(0, 1.5f, 0);
            }

            if (newAge >= 6 && player.Family == null)
            {
                player.Family = FamilyGenerator.Generate();
                player.Gold = player.Family.InitialGold;
            }

            UpdateHUD();

            if (Board.AllGrids != null)
            {
                foreach (var g in Board.AllGrids)
                {
                    if (g.Age <= newAge && g.Age > oldAge) { ShowEvent(g); return; }
                }
            }
            if (newAge >= 100) ShowDeath();
        }
    }

    void ShowEvent(GridData g)
    {
        UIManager.Instance.UpdateUI(GameState.BoardGame);
        var ep = GameObject.Find("EventDialogPanel");
        if (ep != null) ep.SetActive(true);
        if (eventTitle != null) eventTitle.text = g.Age + " - " + g.Title;
        if (eventDesc != null) eventDesc.text = g.Description;
        if (choice1Btn != null) choice1Btn.SetActive(false);
        if (choice2Btn != null) choice2Btn.SetActive(false);
        if (continueBtn != null) continueBtn.SetActive(true);

        GameManager.Instance.Player.Gold += g.BaseGoldReward;
        // NO karma change here - karma only changes from player actions in open world

        if (g.HasDeathRisk && Random.value < g.DeathProbability)
        {
            if (eventDesc != null) eventDesc.text = g.Description + "\n\n...";
            if (continueBtn != null)
            {
                var btn = continueBtn.GetComponent<Button>();
                if (btn != null) { btn.onClick.RemoveAllListeners(); btn.onClick.AddListener(ShowDeath); }
            }
            return;
        }
        UpdateHUD();
    }

    void OnContinue()
    {
        var ep = GameObject.Find("EventDialogPanel");
        if (ep != null) ep.SetActive(false);
        if (continueBtn != null)
        {
            var btn = continueBtn.GetComponent<Button>();
            if (btn != null) { btn.onClick.RemoveAllListeners(); btn.onClick.AddListener(OnContinue); }
        }
        UpdateHUD();
        if (GameManager.Instance.Player.CurrentAge >= 100) { ShowDeath(); return; }
        UIManager.Instance.UpdateUI(GameState.BoardGame);
    }

    void ShowDeath()
    {
        GameManager.Instance.ChangeState(GameState.Death);
        UIManager.Instance.UpdateUI(GameState.Death);
        var p = GameManager.Instance.Player;
        if (deathAge != null) deathAge.text = "Lived to: " + p.CurrentAge;
        if (deathGold != null) deathGold.text = "Wealth: " + p.Gold;

        // Karma revealed ONLY at death - player sees it for the first time here
        string karmaDesc;
        if (p.KarmaValue > 10) karmaDesc = "A truly kind soul";
        else if (p.KarmaValue > 0) karmaDesc = "More good than bad";
        else if (p.KarmaValue == 0) karmaDesc = "Perfectly balanced";
        else if (p.KarmaValue > -10) karmaDesc = "Some regrets linger";
        else karmaDesc = "A heavy conscience";

        if (deathKarma != null) deathKarma.text = karmaDesc; // descriptive, not numeric

        string realm = KarmaJudge.Judge(p) == WorldRealm.Heaven ? "Heaven" : "Hell";
        if (realmText != null)
        {
            realmText.text = "-> " + realm;
            realmText.color = realm == "Heaven" ? Color.cyan : Color.red;
        }
    }

    void OnReincarnate()
    {
        GameManager.Instance.Player = KarmaJudge.Reincarnate(GameManager.Instance.Player);
        if (Board != null) Board.ResetBoard();
        OnStart();
    }

    /// <summary>
    /// HUD shows name, age, gold - but NOT karma (hidden)
    /// </summary>
    void UpdateHUD()
    {
        var p = GameManager.Instance.Player;
        if (nameText != null) nameText.text = p.PlayerName;
        if (ageText != null) ageText.text = "Age: " + p.CurrentAge;
        if (goldText != null) goldText.text = "Gold: " + p.Gold;
        // Karma NOT shown in HUD - it's hidden until death
        if (karmaText != null) karmaText.text = ""; // empty, or could show "???"
    }

    Text FindText(GameObject r, string n) { foreach (var t in r.GetComponentsInChildren<Text>(true)) if (t.name == n) return t; return null; }
    GameObject FindGO(GameObject r, string n) { foreach (var t in r.GetComponentsInChildren<Transform>(true)) if (t.name == n) return t.gameObject; return null; }
}