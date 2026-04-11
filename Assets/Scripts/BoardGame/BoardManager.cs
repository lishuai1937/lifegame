using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// BoardManager v3 - All systems integrated into main game loop
/// 
/// Each age step triggers:
/// 1. Health decay
/// 2. Weather generation
/// 3. Era event check
/// 4. Asset passive income
/// 5. Career passive stat growth
/// 6. Karma echo (luck from good deeds)
/// 7. Dream milestone check (6/18/22/30)
/// 8. Education milestone check
/// 9. Phone unlock check (12/18)
/// 10. Last wish unlock (70)
/// 11. NPC event invitations
/// 12. World random event
/// 13. Diary auto-record
/// 14. Death probability (health + career + age + weather + era)
/// 15. Grid event (enter open world)
/// </summary>
public class BoardManager : MonoBehaviour
{
    [Header("Config")]
    public TextAsset GridDataJson;

    [Header("2D Board Layout")]
    public float CellSpacing = 2.2f;
    public int CellsPerRow = 10;
    public float RowHeight = 3.2f;
    public Transform PlayerToken;
    public float TokenMoveSpeed = 8f;

    [Header("Runtime")]
    public GridData[] AllGrids;
    public int CurrentGridIndex = 0;
    private bool isMoving = false;
    private static readonly int[] DreamAges = { 6, 18, 22, 30 };

    void Start() { LoadGridData(); }

    void LoadGridData()
    {
        if (GridDataJson == null) return;
        var c = JsonUtility.FromJson<GridDataCollection>(GridDataJson.text);
        AllGrids = c.Grids;
        Debug.Log($"[Board] Loaded {AllGrids.Length} grids");
    }

    public Vector3 GetCellPosition(int index)
    {
        int row = index / CellsPerRow, col = index % CellsPerRow;
        bool right = (row % 2 == 0);
        float x = right ? col * CellSpacing : (CellsPerRow - 1 - col) * CellSpacing;
        return new Vector3(x, row * RowHeight, 0);
    }

    public void RollDice()
    {
        if (isMoving) return;
        var p = GameManager.Instance.Player;
        int dice = DiceSystem.Roll(p.CurrentDiceSpeed);
        Debug.Log($"[Board] Rolled: {dice}");
        StartCoroutine(MoveTokenSteps(dice));
    }

    IEnumerator MoveTokenSteps(int steps)
    {
        isMoving = true;
        var player = GameManager.Instance.Player;

        for (int i = 0; i < steps; i++)
        {
            CurrentGridIndex++;
            player.CurrentAge = CurrentGridIndex;
            if (CurrentGridIndex > 100) { OnNaturalDeath(); isMoving = false; yield break; }

            // Animate hop
            if (PlayerToken != null)
            {
                Vector3 target = GetCellPosition(CurrentGridIndex - 1); target.y += 1f;
                Vector3 start = PlayerToken.position;
                Vector3 peak = (start + target) / 2f + Vector3.up * 1.5f;
                float t = 0;
                while (t < 1f) { t = Mathf.Min(t + Time.deltaTime * TokenMoveSpeed, 1f); Vector3 a = Vector3.Lerp(start, peak, t); Vector3 b = Vector3.Lerp(peak, target, t); PlayerToken.position = Vector3.Lerp(a, b, t); yield return null; }
            }
            yield return new WaitForSeconds(0.1f);
        }

        isMoving = false;
        ProcessAge(CurrentGridIndex);
    }

    /// <summary>
    /// MAIN INTEGRATION POINT - all systems trigger here
    /// </summary>
    void ProcessAge(int age)
    {
        var p = GameManager.Instance.Player;

        // === 1. HEALTH DECAY + HOUSING RECOVERY ===
        if (p.Health != null)
        {
            p.Health.AgeDecay(age);
            // Weather affects mental health
            if (WeatherSystem.Instance != null)
                p.Health.Heal(0, WeatherSystem.Instance.GetMoodModifier());
            // Furniture health bonuses
            if (p.Housing != null)
            {
                p.Health.Heal(p.Housing.GetHealthBonus(), p.Housing.GetMentalBonus());
                // Apply furniture stat bonuses
                foreach (var bonus in p.Housing.GetStatBonuses())
                    if (p.Stats != null) p.Stats.Modify(bonus.Stat, bonus.Value);
            }
        }

        // === 2. WEATHER ===
        if (WeatherSystem.Instance != null)
            WeatherSystem.Instance.GenerateWeather(age, p.Stats != null ? p.Stats.Luck : 5);

        // === 3. ERA EVENT ===
        EraEvent era = null;
        if (EraSystem.Instance != null)
        {
            era = EraSystem.Instance.GenerateEraEvent(age);
            if (era != null)
            {
                // Apply economy modifier to gold
                int eraGold = EraSystem.Instance.GetGoldModifier(p.Gold);
                p.Gold = Mathf.Max(0, p.Gold + eraGold);
                // Diary
                p.Diary?.AddEntry(age, era.Title, era.Description, DiaryCategory.Milestone);
                // Economic crisis can hit assets
                if (era.Type == EraType.EconomicCrisis && p.Assets != null)
                    p.Assets.EconomicCrisis();
            }
        }

        // === 4. ASSET PASSIVE INCOME ===
        if (p.Assets != null)
        {
            int luck = p.Stats != null ? p.Stats.Luck : 5;
            int income = p.Assets.ProcessPassiveIncome(age, luck);
            p.Gold = Mathf.Max(0, p.Gold + income);
            p.Assets.AppreciateCollectibles(age);
        }

        // === 5. CAREER PASSIVE STAT GROWTH ===
        if (StatGrowth.Instance != null)
            StatGrowth.Instance.ApplyCareerPassive();

        // === 6. KARMA ECHO (luck from good deeds) ===
        if (StatGrowth.Instance != null)
            StatGrowth.Instance.CheckKarmaEcho();

        // === 7. DREAM MILESTONES ===
        foreach (int dreamAge in DreamAges)
        {
            if (age == dreamAge)
            {
                Debug.Log($"[Board] Dream milestone at age {dreamAge}");
                p.Diary?.RecordDream(age, $"Asked about dream at age {dreamAge}");
                // If keeping same dream, willpower +1
                if (dreamAge > 6 && p.Dream != null)
                {
                    int prev = dreamAge == 18 ? p.Dream.DreamAt6 :
                               dreamAge == 22 ? p.Dream.DreamAt18 :
                               dreamAge == 30 ? p.Dream.DreamAt22 : -1;
                    // Will be checked after player makes choice in UI
                }
                // TODO: trigger dream selection UI
            }
        }

        // === 8. EDUCATION MILESTONES ===
        if (p.Skills != null)
        {
            if (age == 15) { p.Skills.OnGraduate(EducationLevel.HighSchool); p.Diary?.RecordMilestone(age, "Graduated middle school"); }
            if (age == 18) { p.Skills.OnGraduate(EducationLevel.HighSchool); p.Diary?.RecordMilestone(age, "Graduated high school"); }
            if (age == 22) { p.Skills.OnGraduate(EducationLevel.University); p.Diary?.RecordMilestone(age, "Graduated university"); }
        }

        // === 9. PHONE UNLOCK ===
        if (PhoneSystem.Instance != null)
            PhoneSystem.Instance.CheckUnlock(age);

        // === 10. LAST WISH UNLOCK ===
        if (age == 70 && p.LastWishes != null && !p.LastWishes.IsUnlocked)
        {
            p.LastWishes.Unlock();
            p.Diary?.RecordMilestone(age, "Started thinking about last wishes");
            Debug.Log("[Board] Last wishes unlocked");
            // TODO: trigger wish selection UI
        }

        // === 11. NPC EVENT INVITATIONS ===
        List<PendingInvite> invites = null;
        if (NPCEventManager.Instance != null)
        {
            invites = NPCEventManager.Instance.CheckEventsAtAge(age);
            if (invites != null && invites.Count > 0)
            {
                Debug.Log($"[Board] {invites.Count} NPC invitations");
                // TODO: show invite UI
                // For now, auto-process first invite
                foreach (var inv in invites)
                {
                    if (inv.Event.Type == NPCEventType.Death)
                        p.Diary?.RecordLoss(age, $"{inv.NpcName} passed away");
                    else if (inv.Event.Type == NPCEventType.Marriage)
                        p.Diary?.RecordRelationship(age, $"Attended {inv.NpcName}'s wedding");
                }
            }
        }

        // === 12. WORLD RANDOM EVENT ===
        if (WorldEventSystem.Instance != null)
        {
            var worldEvt = WorldEventSystem.Instance.CheckWorldEvent(age, p.Stats);
            if (worldEvt != null)
            {
                WorldEventSystem.Instance.LogEvent(worldEvt);
                p.Gold = Mathf.Max(0, p.Gold + worldEvt.GoldChange);
                if (worldEvt.GoldChange <= -200 && StatGrowth.Instance != null)
                    StatGrowth.Instance.OnBigGoldLoss(Mathf.Abs(worldEvt.GoldChange));

                // Diary
                if (worldEvt.IsPositive)
                    p.Diary?.RecordJoy(age, worldEvt.Title + ": " + worldEvt.EmotionalImpact);
                else
                    p.Diary?.RecordHardship(age, worldEvt.Title + ": " + worldEvt.EmotionalImpact);

                // Reputation from world events
                if (p.Reputation != null)
                {
                    if (worldEvt.IsPositive) p.Reputation.Modify(1);
                    if (worldEvt.Achievement != null) p.Reputation.OnPublicAchievement();
                }

                Debug.Log($"[Board] World event: {worldEvt.Title}");
                // TODO: show world event UI
            }
        }

        // === 13. SPEED CHOICE ===
        if (p.CurrentAge >= p.NextSpeedChoiceAge)
        {
            p.NextSpeedChoiceAge += 20;
            if (UIManager.Instance != null) UIManager.Instance.ShowSpeedChoice(null);
        }

        // === 14. FAMILY AT 6 ===
        if (age >= 6 && p.Family == null)
        {
            p.Family = FamilyGenerator.Generate();
            p.Gold = p.Family.InitialGold;
            p.Diary?.RecordMilestone(6, $"Family: {p.Family.FamilyTrait}, wealth level {p.Family.WealthLevel}");
        }

        // === 15. FIND GRID EVENT ===
        GridData grid = FindGridForAge(age);
        if (grid != null)
        {
            // Apply all modifiers to death probability
            float deathMod = 0;
            if (p.Dream != null) deathMod += p.Dream.GetDeathProbModifier();
            if (p.Health != null) deathMod += p.Health.GetDeathModifier();
            if (WeatherSystem.Instance != null) deathMod += (WeatherSystem.Instance.GetEventModifier() - 1f) * 0.1f;
            if (p.Dream?.ActiveCareer == DreamCareer.Athlete && age < 60) deathMod = Mathf.Max(deathMod, 0); // no athlete penalty before 60

            grid.DeathProbability = Mathf.Clamp01(grid.DeathProbability + deathMod);

            // Apply gold multipliers
            if (grid.BaseGoldReward > 0)
            {
                float mult = 1f;
                if (p.Dream != null) mult *= p.Dream.GetGoldMultiplier();
                if (p.Skills != null) mult *= p.Skills.GetIncomeMultiplier();
                if (p.Dream != null && p.Dream.IsUnwaveringHeart() && age >= 30) mult *= 2f;
                // Travel bonus from vehicle
                if (grid.SceneId != null && (grid.SceneId.Contains("travel") || grid.SceneId.Contains("beach")))
                    if (p.Assets != null) mult *= p.Assets.GetTravelMultiplier();
                grid.BaseGoldReward = Mathf.RoundToInt(grid.BaseGoldReward * mult);
            }

            // Diary
            p.Diary?.RecordMilestone(age, grid.Title);

            // Enter grid world
            OnLandOnGrid(grid);
        }
    }

    void OnLandOnGrid(GridData grid)
    {
        Debug.Log($"[Board] Landed: age {grid.Age} - {grid.Title}");

        // Initialize social system for this world
        if (SocialSystem.Instance != null)
        {
            int energyMod = 0;
            var p = GameManager.Instance.Player;
            if (p.Health != null) energyMod += p.Health.GetEnergyModifier();
            SocialSystem.Instance.EnterWorld(grid.Age);
            SocialSystem.Instance.MaxUnlockEnergy = Mathf.Max(1, SocialSystem.Instance.MaxUnlockEnergy + energyMod);
            SocialSystem.Instance.CurrentUnlockEnergy = SocialSystem.Instance.MaxUnlockEnergy;
        }

        GameManager.Instance.EnterGridWorld(grid.Age);
    }

    void OnNaturalDeath()
    {
        var p = GameManager.Instance.Player;
        p.Diary?.RecordMilestone(100, "Lived a full century");
        GameManager.Instance.ChangeState(GameState.Death);
    }

    public void ResetBoard()
    {
        CurrentGridIndex = 0;
        if (PlayerToken != null) { Vector3 s = GetCellPosition(0); s.y += 1f; PlayerToken.position = s; }
    }

    GridData FindGridForAge(int age)
    {
        if (AllGrids == null) return null;
        foreach (var g in AllGrids) if (g.Age == age) return g;
        return null;
    }
}