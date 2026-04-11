using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// GameFlowController v3 - All systems integrated into death/reincarnation
/// </summary>
public class GameFlowController : MonoBehaviour
{
    [Header("References")]
    public BoardManager Board;
    public GameObject PlayerObj, BoardObj;
    public Transform PlayerToken, DiceVisual;
    public GameObject OpenWorldRoot;
    public Camera OpenWorldCamera;

    Text diceResultText, eventTitle, eventDesc, deathAge, deathGold, deathKarma, realmText;
    Text nameText, ageText, goldText, karmaText;
    GameObject choice1Btn, choice2Btn, continueBtn;
    float diceSpinTimer; bool isSpinning; int diceResult;
    bool showingEvent = false; // blocks dice rolling while event is shown
    GridData pendingGrid; // grid waiting to enter open world
    GameObject openWorldObj;
    Camera mainCam, owCam;

    void Start()
    {
        CacheUI(); BindButtons(); ShowMainMenu();
        // Find OpenWorld even if inactive - search all root game objects
        foreach (var root in UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects())
        {
            if (root.name == "OpenWorld")
            {
                openWorldObj = root;
                var camT = root.transform.Find("OW_Camera");
                if (camT != null) owCam = camT.GetComponent<Camera>();
                break;
            }
        }
        mainCam = Camera.main;

        Debug.Log($"[Flow] Start: openWorldObj={openWorldObj != null}, owCam={owCam != null}, mainCam={mainCam != null}");
    }

    void EnsurePausePanel()
    {
        if (pausePanel != null) return;
        if (UIManager.Instance == null) return;
        var canvasT = UIManager.Instance.transform;

        pausePanel = new GameObject("PauseMenuPanel");
        pausePanel.transform.SetParent(canvasT, false);
        var rt = pausePanel.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one; rt.sizeDelta = Vector2.zero;
        var bg = pausePanel.AddComponent<Image>();
        bg.color = new Color(0.05f, 0.03f, 0.1f, 0.92f);

        // Title
        var title = new GameObject("PauseTitle");
        title.transform.SetParent(pausePanel.transform, false);
        var titleRT = title.AddComponent<RectTransform>();
        titleRT.anchoredPosition = new Vector2(0, 80);
        titleRT.sizeDelta = new Vector2(400, 60);
        var titleTxt = title.AddComponent<Text>();
        titleTxt.text = "= 暂停 =";
        titleTxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        titleTxt.fontSize = 42;
        titleTxt.alignment = TextAnchor.MiddleCenter;
        titleTxt.color = new Color(0.3f, 0.9f, 0.95f);

        CreatePauseButton(pausePanel.transform, "ResumeBtn", ">> 继续游戏 <<", new Vector2(0, -10),
            new Color(0.15f, 0.55f, 0.2f), OnResume);
        CreatePauseButton(pausePanel.transform, "ExitWorldBtn", "退出到棋盘", new Vector2(0, -90),
            new Color(0.65f, 0.12f, 0.12f), OnExitWorld);

        pausePanel.SetActive(false);
    }

    void CreatePauseButton(Transform parent, string name, string label, Vector2 pos, Color color, UnityEngine.Events.UnityAction action)
    {
        var btnObj = new GameObject(name);
        btnObj.transform.SetParent(parent, false);
        var btnRT = btnObj.AddComponent<RectTransform>();
        btnRT.anchoredPosition = pos;
        btnRT.sizeDelta = new Vector2(280, 60);
        var btnImg = btnObj.AddComponent<Image>();
        btnImg.color = color;
        var btn = btnObj.AddComponent<Button>();
        btn.targetGraphic = btnImg;
        btn.onClick.AddListener(action);

        var txtObj = new GameObject("Text");
        txtObj.transform.SetParent(btnObj.transform, false);
        var txtRT = txtObj.AddComponent<RectTransform>();
        txtRT.anchorMin = Vector2.zero; txtRT.anchorMax = Vector2.one;
        txtRT.offsetMin = Vector2.zero; txtRT.offsetMax = Vector2.zero;
        var txt = txtObj.AddComponent<Text>();
        txt.text = label;
        txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        txt.fontSize = 24;
        txt.alignment = TextAnchor.MiddleCenter;
        txt.color = Color.white;
    }

    GameObject pausePanel;
    bool isPaused = false;

    void Update()
    {
        if (isSpinning) { diceSpinTimer -= Time.deltaTime; if (DiceVisual != null) DiceVisual.Rotate(Vector3.forward * 720 * Time.deltaTime); if (diceSpinTimer <= 0) FinishRoll(); }
        if (GameManager.Instance != null && GameManager.Instance.CurrentState == GameState.BoardGame && !isSpinning && !showingEvent && !isWalking)
            if (Input.GetKeyDown(KeyCode.Space)) OnRollDice();

        // ESC in open world toggles pause menu
        if (GameManager.Instance != null && GameManager.Instance.CurrentState == GameState.OpenWorld)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                EnsurePausePanel();
                Debug.Log($"[Flow] ESC pressed! isPaused={isPaused}, pausePanel={pausePanel != null}");
                isPaused = !isPaused;
                if (pausePanel != null) pausePanel.SetActive(isPaused);
                else Debug.LogWarning("[Flow] pausePanel is NULL!");
            }
        }

        // Detect return from open world
        if (wasInOpenWorld && GameManager.Instance != null && GameManager.Instance.CurrentState == GameState.BoardGame)
        {
            wasInOpenWorld = false;
            ExitOpenWorld();
        }
        if (GameManager.Instance != null && GameManager.Instance.CurrentState == GameState.OpenWorld)
            wasInOpenWorld = true;
    }

    bool wasInOpenWorld = false;

    void CacheUI()
    {
        var c = GameObject.Find("UICanvas"); if (c == null) return;
        diceResultText = FT(c, "DiceResultText"); eventTitle = FT(c, "EventTitle"); eventDesc = FT(c, "EventDesc");
        deathAge = FT(c, "DeathAge"); deathGold = FT(c, "DeathGold"); deathKarma = FT(c, "DeathKarma"); realmText = FT(c, "RealmText");
        nameText = FT(c, "NameText"); ageText = FT(c, "AgeText"); goldText = FT(c, "GoldText"); karmaText = FT(c, "KarmaText");
        choice1Btn = FG(c, "Choice1Btn"); choice2Btn = FG(c, "Choice2Btn"); continueBtn = FG(c, "ContinueBtn");
    }

    void BindButtons()
    {
        var c = GameObject.Find("UICanvas"); if (c == null) return;
        B(c,"StartButton",OnStart); B(c,"RollDiceBtn",OnRollDice);
        B(c,"SlowBtn",()=>OnSpeed(DiceSpeed.Slow)); B(c,"FastBtn",()=>OnSpeed(DiceSpeed.Fast));
        B(c,"ReincarnateBtn",OnReincarnate); B(c,"MainMenuBtn",ShowMainMenu); B(c,"ContinueBtn",OnContinue);
    }

    void B(GameObject r, string n, UnityEngine.Events.UnityAction a) { var o=FG(r,n); if(o!=null){var b=o.GetComponent<Button>();if(b!=null)b.onClick.AddListener(a);} }

    void ShowMainMenu()
    {
        GameManager.Instance.ChangeState(GameState.MainMenu);
        UIManager.Instance.UpdateUI(GameState.MainMenu);
        if (PlayerObj != null) PlayerObj.SetActive(false);
        if (BoardObj != null) BoardObj.SetActive(true);
    }

    void OnStart()
    {
        var p = new PlayerData { PlayerName = "Player" };
        p.CurrentAge = 1;
        GameManager.Instance.Player = p;
        if (Board != null)
        {
            Board.ResetBoard();
            Debug.Log($"[Flow] Board spacing={Board.CellSpacing}, rowH={Board.RowHeight}, perRow={Board.CellsPerRow}");
            Debug.Log($"[Flow] Cell 1 pos={Board.GetCellPosition(0)}, Cell 10 pos={Board.GetCellPosition(9)}, Cell 11 pos={Board.GetCellPosition(10)}");
        }
        GameManager.Instance.ChangeState(GameState.BoardGame);
        UIManager.Instance.UpdateUI(GameState.BoardGame);
        UIManager.Instance.ShowSpeedChoice(null);

        // Place token on first cell
        if (PlayerToken != null && Board != null)
        {
            Vector3 pos = Board.GetCellPosition(0);
            pos.y += 1f; pos.z = -0.5f;
            PlayerToken.position = pos;
            if (DiceVisual != null) DiceVisual.position = pos + new Vector3(0, 1.5f, 0);
        }

        UpdateHUD();

        // Trigger first grid event (birth) immediately
        if (Board != null && Board.AllGrids != null)
        {
            foreach (var g in Board.AllGrids)
            {
                if (g.Age == 1) { ShowEvent(g); return; }
            }
        }
    }

    void OnSpeed(DiceSpeed s)
    {
        GameManager.Instance.Player.CurrentDiceSpeed = s;
        var sp = GameObject.Find("SpeedChoicePanel"); if (sp != null) sp.SetActive(false);
        UIManager.Instance.UpdateUI(GameState.BoardGame); UpdateHUD();
    }

    void OnRollDice()
    {
        if (isSpinning || showingEvent || isWalking) return;
        diceResult = DiceSystem.Roll(GameManager.Instance.Player.CurrentDiceSpeed);
        isSpinning = true; diceSpinTimer = 0.8f;
        if (diceResultText != null) diceResultText.text = "...";
        Debug.Log($"[Flow] OnRollDice: rolled {diceResult}");
    }

    void FinishRoll()
    {
        isSpinning = false;
        if (DiceVisual != null) DiceVisual.rotation = Quaternion.identity;
        if (diceResultText != null) diceResultText.text = diceResult.ToString();
        Debug.Log($"[Flow] FinishRoll: diceResult={diceResult}");

        if (Board != null)
        {
            var p = GameManager.Instance.Player;
            int oldAge = p.CurrentAge;
            int newAge = Mathf.Min(oldAge + diceResult, 100);
            // Start walking animation coroutine
            StartCoroutine(WalkToAge(oldAge, newAge));
        }
        else
        {
            Debug.LogWarning("[Flow] Board is NULL!");
        }
    }

    bool isWalking = false;

    IEnumerator WalkToAge(int fromAge, int toAge)
    {
        isWalking = true;
        var p = GameManager.Instance.Player;

        for (int age = fromAge + 1; age <= toAge; age++)
        {
            // Snap directly to cell center
            Vector3 cellPos = Board.GetCellPosition(age - 1);
            PlayerToken.position = new Vector3(cellPos.x, cellPos.y + 1f, -0.5f);
            if (DiceVisual != null) DiceVisual.position = PlayerToken.position + new Vector3(0, 1.5f, 0);

            // Brief pause so player can see the movement
            yield return new WaitForSeconds(0.15f);
        }

        p.CurrentAge = toAge;
        Board.CurrentGridIndex = toAge;
        UpdateHUD();

        isWalking = false;

        if (Board.AllGrids != null)
        {
            foreach (var g in Board.AllGrids)
            {
                if (g.Age == toAge)
                {
                    Debug.Log($"[Flow] Landed on age {g.Age} - {g.Title}");
                    ShowEvent(g);
                    yield break;
                }
            }
        }

        if (toAge >= 100) ShowDeath();
    }

    void ShowEvent(GridData g)
    {
        pendingGrid = g;
        showingEvent = true;
        Debug.Log($"[Flow] ShowEvent: age {g.Age} - {g.Title}");

        // Hide dice panel, show event dialog via UIManager references
        if (UIManager.Instance.DicePanel != null) UIManager.Instance.DicePanel.SetActive(false);
        if (UIManager.Instance.EventDialogPanel != null) UIManager.Instance.EventDialogPanel.SetActive(true);

        if (eventTitle != null) eventTitle.text = g.Age + " - " + g.Title;
        if (eventDesc != null) eventDesc.text = g.Description + "\n\n[Click Continue to enter this world]";
        if (choice1Btn != null) choice1Btn.SetActive(false);
        if (choice2Btn != null) choice2Btn.SetActive(false);
        if (continueBtn != null) continueBtn.SetActive(true);

        GameManager.Instance.Player.Gold += g.BaseGoldReward;

        if (g.HasDeathRisk && Random.value < g.DeathProbability)
        {
            pendingGrid = null;
            if (eventDesc != null) eventDesc.text = g.Description + "\n\n...You didn't make it.";
            if (StatGrowth.Instance != null && g.DeathProbability >= 0.3f)
                StatGrowth.Instance.OnSurvivedHighDeathProb(g.DeathProbability);
            if (continueBtn != null) { var btn = continueBtn.GetComponent<Button>(); if (btn != null) { btn.onClick.RemoveAllListeners(); btn.onClick.AddListener(ShowDeath); } }
            return;
        }
        else if (g.HasDeathRisk)
        {
            if (StatGrowth.Instance != null)
            {
                StatGrowth.Instance.OnSurvivedDeathRisk();
                if (g.DeathProbability >= 0.3f) StatGrowth.Instance.OnSurvivedHighDeathProb(g.DeathProbability);
            }
        }

        UpdateHUD();
    }

    void OnContinue()
    {
        Debug.Log($"[Flow] OnContinue called. pendingGrid={pendingGrid != null} openWorldObj={openWorldObj != null} owCam={owCam != null} mainCam={mainCam != null}");
        showingEvent = false;
        if (UIManager.Instance.EventDialogPanel != null) UIManager.Instance.EventDialogPanel.SetActive(false);
        if (continueBtn != null) { var btn = continueBtn.GetComponent<Button>(); if (btn != null) { btn.onClick.RemoveAllListeners(); btn.onClick.AddListener(OnContinue); } }

        if (pendingGrid != null)
        {
            Debug.Log($"[Flow] pendingGrid age={pendingGrid.Age} sceneId={pendingGrid.SceneId}");
            EnterOpenWorld(pendingGrid);
            pendingGrid = null;
            return;
        }

        Debug.Log("[Flow] No pendingGrid, returning to board");
        UpdateHUD();
        if (GameManager.Instance.Player.CurrentAge >= 100) { ShowDeath(); return; }
        UIManager.Instance.UpdateUI(GameState.BoardGame);
    }

    void EnterOpenWorld(GridData grid)
    {
        Debug.Log($"[Flow] === ENTERING OPEN WORLD === age {grid.Age} - {grid.Title} sceneId={grid.SceneId}");
        Debug.Log($"[Flow] openWorldObj={openWorldObj != null}, mainCam={mainCam != null}, owCam={owCam != null}");
        Debug.Log($"[Flow] SceneGenerator.Instance={SceneGenerator.Instance != null}");

        // Generate scene at OpenWorld position
        if (SceneGenerator.Instance != null)
        {
            SceneGenerator.Instance.ClearScene();
            var generated = SceneGenerator.Instance.Generate(grid.SceneId ?? "home_child", grid.Age);
            if (generated != null)
            {
                // Place generated scene at OpenWorld's position
                generated.transform.position = openWorldObj.transform.position;
            }
        }

        // Activate open world area and hide pre-built objects
        if (openWorldObj != null)
        {
            openWorldObj.SetActive(true);
            // Hide pre-built buildings/event points, keep Player and OW_Camera
            foreach (Transform child in openWorldObj.transform)
            {
                if (child.name != "Player" && child.name != "OW_Camera" && child.name != "OW_Ground" && child.name != "OW_Light")
                    child.gameObject.SetActive(false);
            }
        }

        // Switch cameras - find OW_Camera after OpenWorld is activated
        if (mainCam != null) { mainCam.enabled = false; Debug.Log("[Flow] MainCam disabled"); }
        else Debug.LogWarning("[Flow] mainCam is NULL!");

        // Always re-find OW_Camera since it's a child of OpenWorld (was inactive before)
        if (openWorldObj != null)
        {
            var owCamObj = openWorldObj.transform.Find("OW_Camera");
            if (owCamObj != null) owCam = owCamObj.GetComponent<Camera>();
        }
        if (owCam != null) { owCam.enabled = true; Debug.Log("[Flow] OW_Camera enabled"); }
        else Debug.LogWarning("[Flow] owCam is NULL! No open world camera.");

        // Hide board
        if (BoardObj != null) BoardObj.SetActive(false);

        // Reset player position to ground level in open world
        var player = openWorldObj?.GetComponentInChildren<PlayerController>();
        if (player != null)
        {
            var cc = player.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false; // disable to teleport
            player.transform.localPosition = new Vector3(0, 1.5f, 0);
            if (cc != null) cc.enabled = true;
        }

        // Initialize social system
        if (SocialSystem.Instance != null)
            SocialSystem.Instance.EnterWorld(grid.Age);

        // Start timer
        if (GridWorldTimer.Instance != null)
            GridWorldTimer.Instance.StartTimer(grid.Age);

        // Change state
        GameManager.Instance.EnterGridWorld(grid.Age);
        UIManager.Instance.UpdateUI(GameState.OpenWorld);
        Debug.Log($"[Flow] === OPEN WORLD ENTERED === state={GameManager.Instance.CurrentState}");
    }

    void ExitOpenWorld()
    {
        Debug.Log("[Flow] Exiting open world, back to board");

        // Hide pause menu if open
        isPaused = false;
        if (pausePanel != null) pausePanel.SetActive(false);

        // Deactivate open world
        if (openWorldObj != null) openWorldObj.SetActive(false);

        // Switch cameras back
        if (owCam != null) owCam.enabled = false;
        if (mainCam != null) mainCam.enabled = true;

        // Show board
        if (BoardObj != null) BoardObj.SetActive(true);

        // Restore player token to correct board position
        if (PlayerToken != null && Board != null)
        {
            int currentAge = GameManager.Instance.Player.CurrentAge;
            Vector3 pos = Board.GetCellPosition(currentAge - 1);
            pos.y += 1f; pos.z = -0.5f;
            PlayerToken.position = pos;
            if (DiceVisual != null) DiceVisual.position = pos + new Vector3(0, 1.5f, 0);
        }

        // Clear generated scene
        if (SceneGenerator.Instance != null)
            SceneGenerator.Instance.ClearScene();

        UpdateHUD();
        if (GameManager.Instance.Player.CurrentAge >= 100) { ShowDeath(); return; }
        UIManager.Instance.UpdateUI(GameState.BoardGame);
    }

    void OnResume()
    {
        isPaused = false;
        if (pausePanel != null) pausePanel.SetActive(false);
    }

    void OnExitWorld()
    {
        isPaused = false;
        if (pausePanel != null) pausePanel.SetActive(false);
        var player = openWorldObj?.GetComponentInChildren<PlayerController>();
        if (player != null) player.ExitWorld();
    }

    void ShowDeath()
    {
        GameManager.Instance.ChangeState(GameState.Death);
        UIManager.Instance.UpdateUI(GameState.Death);
        var p = GameManager.Instance.Player;

        // === WRITER MEMOIR BONUS ===
        if (p.Dream?.ActiveCareer == DreamCareer.Writer && p.CurrentAge >= 85)
            p.KarmaValue += 5;

        // === UNWAVERING HEART ===
        if (p.Dream != null && p.Dream.IsUnwaveringHeart())
            p.KarmaValue += 10;

        // === SOLDIER LIFETIME KARMA ===
        if (p.Dream?.ActiveCareer == DreamCareer.Soldier)
            p.KarmaValue += 2; // final bonus

        // Basic info
        if (deathAge != null) deathAge.text = "Lived to: " + p.CurrentAge;
        if (deathGold != null) deathGold.text = "Wealth: " + p.Gold + " (Assets: " + (p.Assets?.GetTotalAssetValue() ?? 0) + ")";

        // === KARMA DESCRIPTION ===
        string karmaDesc;
        if (p.KarmaValue > 15) karmaDesc = "A truly radiant soul";
        else if (p.KarmaValue > 10) karmaDesc = "A truly kind soul";
        else if (p.KarmaValue > 5) karmaDesc = "More good than bad";
        else if (p.KarmaValue > 0) karmaDesc = "A gentle heart";
        else if (p.KarmaValue == 0) karmaDesc = "Perfectly balanced";
        else if (p.KarmaValue > -5) karmaDesc = "Some regrets linger";
        else if (p.KarmaValue > -10) karmaDesc = "Debts unpaid";
        else karmaDesc = "A heavy conscience";
        if (deathKarma != null) deathKarma.text = karmaDesc;

        // === BUILD LIFE SUMMARY ===
        var summary = new List<string>();

        // Career
        if (p.Dream?.ActiveCareer != DreamCareer.None)
        {
            string careerName = "";
            foreach (var c in DreamSystem.AllCareers)
                if (c.Career == p.Dream.ActiveCareer) { careerName = c.Name; break; }
            summary.Add("Career: " + careerName);
            if (p.Dream.IsUnwaveringHeart()) summary.Add("Unwavering Heart - never changed their dream");
        }

        // Education
        if (p.Skills != null) summary.Add("Education: " + p.Skills.Education);

        // Social
        if (SocialSystem.Instance != null) summary.Add(SocialSystem.Instance.GetLifeSummary());

        // Health
        if (p.Health != null)
        {
            if (p.Health.HasChronicIllness) summary.Add("Battled " + p.Health.ChronicIllnessName);
            if (p.Health.PhysicalHealth > 60) summary.Add("Stayed healthy until the end");
        }

        // Reputation
        if (p.Reputation != null) summary.Add(p.Reputation.GetDescription());

        // Assets
        if (p.Assets != null)
        {
            int total = p.Assets.GetTotalAssetValue();
            if (total > 10000) summary.Add("Left behind a fortune");
            else if (total > 3000) summary.Add("Comfortable wealth");
            else if (total > 0) summary.Add("Modest possessions");
            else summary.Add("Left with nothing material");
        }

        // Last wishes
        if (p.LastWishes != null && p.LastWishes.IsUnlocked)
            summary.Add(p.LastWishes.GetDeathReflection());

        // Era highlights
        if (EraSystem.Instance != null && EraSystem.Instance.NewsHistory.Count > 0)
            summary.Add("Lived through: " + string.Join(", ", EraSystem.Instance.NewsHistory.GetRange(0, Mathf.Min(3, EraSystem.Instance.NewsHistory.Count))));

        // World events
        if (WorldEventSystem.Instance != null && WorldEventSystem.Instance.LifeEventLog.Count > 0)
            summary.Add("Key moments: " + WorldEventSystem.Instance.LifeEventLog.Count + " life events");

        // Diary highlights
        if (p.Diary != null)
        {
            var highlights = p.Diary.GetHighlights(3);
            foreach (var h in highlights)
                summary.Add($"Age {h.Age}: {h.Content}");
        }

        if (eventDesc != null) eventDesc.text = string.Join("\n", summary);

        // === REALM JUDGMENT ===
        string realm = KarmaJudge.Judge(p) == WorldRealm.Heaven ? "Heaven" : "Hell";
        if (realmText != null) { realmText.text = "-> " + realm; realmText.color = realm == "Heaven" ? Color.cyan : Color.red; }
    }

    void OnReincarnate()
    {
        var p = GameManager.Instance.Player;

        // Collect all soul memories
        var memories = new List<string>();

        // Writer bonus
        if (p.Dream?.ActiveCareer == DreamCareer.Writer)
        { memories.Add("The stories you wrote"); memories.Add("The words that touched hearts"); }

        // Social memories
        if (SocialSystem.Instance != null) memories.AddRange(SocialSystem.Instance.GetSoulMemories());

        // Diary memories
        if (p.Diary != null) memories.AddRange(p.Diary.ToSoulMemories());

        // Last wish memories
        if (p.LastWishes != null)
        {
            foreach (var w in p.LastWishes.Wishes)
                if (w.IsCompleted) memories.Add("Fulfilled: " + w.Text);
                else memories.Add("Unfulfilled: " + w.Text);
        }

        // Reincarnate
        var newPlayer = KarmaJudge.Reincarnate(p);
        var allMem = new List<string>(newPlayer.SoulMemories);
        allMem.AddRange(memories);
        newPlayer.SoulMemories = allMem.ToArray();

        // Luck +1 from reincarnation
        if (StatGrowth.Instance != null) StatGrowth.Instance.OnReincarnation();

        GameManager.Instance.Player = newPlayer;

        // Reset all systems
        if (Board != null) Board.ResetBoard();
        if (SocialSystem.Instance != null) SocialSystem.Instance.ResetForNewLife();
        if (NPCEventManager.Instance != null) NPCEventManager.Instance.ResetForNewLife();
        if (PhoneSystem.Instance != null) PhoneSystem.Instance.ResetForNewLife();
        if (WorldEventSystem.Instance != null) WorldEventSystem.Instance.ResetForNewLife();
        if (EraSystem.Instance != null) EraSystem.Instance.ResetForNewLife();
        if (StatGrowth.Instance != null) StatGrowth.Instance.ResetForNewLife();

        OnStart();
    }

    void UpdateHUD()
    {
        var p = GameManager.Instance.Player;
        if (nameText != null) nameText.text = "> " + p.PlayerName;
        if (ageText != null) ageText.text = "年龄: " + p.CurrentAge;
        if (goldText != null) goldText.text = "金币: " + p.Gold;
        if (karmaText != null) karmaText.text = "";
    }

    Text FT(GameObject r, string n) { foreach (var t in r.GetComponentsInChildren<Text>(true)) if (t.name == n) return t; return null; }
    GameObject FG(GameObject r, string n) { foreach (var t in r.GetComponentsInChildren<Transform>(true)) if (t.name == n) return t.gameObject; return null; }
}