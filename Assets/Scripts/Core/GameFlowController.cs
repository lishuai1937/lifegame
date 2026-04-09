using UnityEngine;
using UnityEngine.UI;
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

    Text diceResultText, eventTitle, eventDesc, deathAge, deathGold, deathKarma, realmText;
    Text nameText, ageText, goldText, karmaText;
    GameObject choice1Btn, choice2Btn, continueBtn;
    float diceSpinTimer; bool isSpinning; int diceResult;

    void Start() { CacheUI(); BindButtons(); ShowMainMenu(); }

    void Update()
    {
        if (isSpinning) { diceSpinTimer -= Time.deltaTime; if (DiceVisual != null) DiceVisual.Rotate(Vector3.forward * 720 * Time.deltaTime); if (diceSpinTimer <= 0) FinishRoll(); }
        if (GameManager.Instance != null && GameManager.Instance.CurrentState == GameState.BoardGame && !isSpinning)
            if (Input.GetKeyDown(KeyCode.Space)) OnRollDice();
    }

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
        GameManager.Instance.Player = p;
        if (Board != null) Board.ResetBoard();
        GameManager.Instance.ChangeState(GameState.BoardGame);
        UIManager.Instance.UpdateUI(GameState.BoardGame);
        UIManager.Instance.ShowSpeedChoice(null);
        UpdateHUD();
    }

    void OnSpeed(DiceSpeed s)
    {
        GameManager.Instance.Player.CurrentDiceSpeed = s;
        var sp = GameObject.Find("SpeedChoicePanel"); if (sp != null) sp.SetActive(false);
        UIManager.Instance.UpdateUI(GameState.BoardGame); UpdateHUD();
    }

    void OnRollDice()
    {
        if (isSpinning) return;
        diceResult = DiceSystem.Roll(GameManager.Instance.Player.CurrentDiceSpeed);
        isSpinning = true; diceSpinTimer = 0.8f;
        if (diceResultText != null) diceResultText.text = "...";
    }

    void FinishRoll()
    {
        isSpinning = false;
        if (DiceVisual != null) DiceVisual.rotation = Quaternion.identity;
        if (diceResultText != null) diceResultText.text = diceResult.ToString();

        if (Board != null)
        {
            var p = GameManager.Instance.Player;
            int oldAge = p.CurrentAge;
            int newAge = Mathf.Min(oldAge + diceResult, 100);
            p.CurrentAge = newAge;
            Board.CurrentGridIndex = newAge;

            if (PlayerToken != null)
            {
                Vector3 target = Board.GetCellPosition(newAge - 1); target.y += 1f; target.z = -0.5f;
                PlayerToken.position = target;
                if (DiceVisual != null) DiceVisual.position = target + new Vector3(0, 1.5f, 0);
            }

            UpdateHUD();

            // Find event in range
            if (Board.AllGrids != null)
                foreach (var g in Board.AllGrids)
                    if (g.Age <= newAge && g.Age > oldAge) { ShowEvent(g); return; }

            if (newAge >= 100) ShowDeath();
        }
    }

    void ShowEvent(GridData g)
    {
        UIManager.Instance.UpdateUI(GameState.BoardGame);
        var ep = GameObject.Find("EventDialogPanel"); if (ep != null) ep.SetActive(true);
        if (eventTitle != null) eventTitle.text = g.Age + " - " + g.Title;
        if (eventDesc != null) eventDesc.text = g.Description;
        if (choice1Btn != null) choice1Btn.SetActive(false);
        if (choice2Btn != null) choice2Btn.SetActive(false);
        if (continueBtn != null) continueBtn.SetActive(true);

        GameManager.Instance.Player.Gold += g.BaseGoldReward;

        if (g.HasDeathRisk && Random.value < g.DeathProbability)
        {
            if (eventDesc != null) eventDesc.text = g.Description + "\n\n...";
            if (StatGrowth.Instance != null && g.DeathProbability >= 0.3f)
                StatGrowth.Instance.OnSurvivedHighDeathProb(g.DeathProbability); // ironic: they didn't survive
            if (continueBtn != null) { var btn = continueBtn.GetComponent<Button>(); if (btn != null) { btn.onClick.RemoveAllListeners(); btn.onClick.AddListener(ShowDeath); } }
            return;
        }
        else if (g.HasDeathRisk)
        {
            // Survived death risk!
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
        var ep = GameObject.Find("EventDialogPanel"); if (ep != null) ep.SetActive(false);
        if (continueBtn != null) { var btn = continueBtn.GetComponent<Button>(); if (btn != null) { btn.onClick.RemoveAllListeners(); btn.onClick.AddListener(OnContinue); } }
        UpdateHUD();
        if (GameManager.Instance.Player.CurrentAge >= 100) { ShowDeath(); return; }
        UIManager.Instance.UpdateUI(GameState.BoardGame);
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
        if (nameText != null) nameText.text = p.PlayerName;
        if (ageText != null) ageText.text = "Age: " + p.CurrentAge;
        if (goldText != null) goldText.text = "Gold: " + p.Gold;
        if (karmaText != null) karmaText.text = ""; // hidden
    }

    Text FT(GameObject r, string n) { foreach (var t in r.GetComponentsInChildren<Text>(true)) if (t.name == n) return t; return null; }
    GameObject FG(GameObject r, string n) { foreach (var t in r.GetComponentsInChildren<Transform>(true)) if (t.name == n) return t.gameObject; return null; }
}