using UnityEngine;
using System.Collections;

/// <summary>
/// 2D Board Manager - Side-scrolling Mario-style board
/// Grid cells laid out left-to-right with platforms
/// Player token is a 2D sprite that hops between cells
/// </summary>
public class BoardManager : MonoBehaviour
{
    [Header("Config")]
    public TextAsset GridDataJson;

    [Header("2D Board Layout")]
    public float CellSpacing = 2.5f;       // horizontal distance between cells
    public float PlatformHeight = 0.5f;     // cell platform thickness
    public int CellsPerRow = 10;            // cells before going up a level
    public float RowHeight = 3f;            // vertical gap between rows
    public Transform PlayerToken;           // 2D sprite token
    public float TokenMoveSpeed = 8f;       // hop animation speed

    [Header("Runtime")]
    public GridData[] AllGrids;
    public int CurrentGridIndex = 0;

    private bool isMoving = false;

    void Start()
    {
        LoadGridData();
    }

    void LoadGridData()
    {
        if (GridDataJson != null)
        {
            var collection = JsonUtility.FromJson<GridDataCollection>(GridDataJson.text);
            AllGrids = collection.Grids;
            Debug.Log($"[BoardManager] Loaded {AllGrids.Length} grid cells");
        }
        else
        {
            Debug.LogWarning("[BoardManager] No grid data JSON assigned");
        }
    }

    /// <summary>
    /// Get world position for a grid index (Mario-style zigzag layout)
    /// Row 0: left to right, Row 1: right to left, Row 2: left to right...
    /// </summary>
    public Vector3 GetCellPosition(int index)
    {
        int row = index / CellsPerRow;
        int col = index % CellsPerRow;

        // Zigzag: even rows go right, odd rows go left
        bool goingRight = (row % 2 == 0);
        float x = goingRight ? col * CellSpacing : (CellsPerRow - 1 - col) * CellSpacing;
        float y = row * RowHeight;

        return new Vector3(x, y, 0);
    }

    /// <summary>
    /// Roll dice and animate token movement
    /// </summary>
    public void RollDice()
    {
        if (isMoving) return;

        var player = GameManager.Instance.Player;
        int diceValue = DiceSystem.Roll(player.CurrentDiceSpeed);
        Debug.Log($"[BoardManager] Rolled: {diceValue}");

        StartCoroutine(MoveTokenSteps(diceValue));
    }

    /// <summary>
    /// Animate token hopping one cell at a time
    /// </summary>
    IEnumerator MoveTokenSteps(int steps)
    {
        isMoving = true;
        var player = GameManager.Instance.Player;

        for (int i = 0; i < steps; i++)
        {
            CurrentGridIndex++;
            player.CurrentAge = CurrentGridIndex;

            if (CurrentGridIndex > 100)
            {
                OnNaturalDeath();
                isMoving = false;
                yield break;
            }

            // Animate hop to next cell
            if (PlayerToken != null)
            {
                Vector3 target = GetCellPosition(CurrentGridIndex - 1);
                target.y += 1f; // token sits on top of platform

                Vector3 start = PlayerToken.position;
                Vector3 peak = (start + target) / 2f + Vector3.up * 1.5f; // arc peak

                float t = 0;
                while (t < 1f)
                {
                    t += Time.deltaTime * TokenMoveSpeed;
                    t = Mathf.Min(t, 1f);
                    // Quadratic bezier for hop arc
                    Vector3 a = Vector3.Lerp(start, peak, t);
                    Vector3 b = Vector3.Lerp(peak, target, t);
                    PlayerToken.position = Vector3.Lerp(a, b, t);
                    yield return null;
                }
            }

            yield return new WaitForSeconds(0.15f); // brief pause between hops
        }

        isMoving = false;

        // Land on grid
        GridData landedGrid = FindGridForAge(CurrentGridIndex);
        if (landedGrid != null)
        {
            OnLandOnGrid(landedGrid);
        }
        else
        {
            // No event at this age, just a normal step
            Debug.Log($"[BoardManager] Age {CurrentGridIndex}: nothing happens");
        }
    }

    GridData FindGridForAge(int age)
    {
        if (AllGrids == null) return null;
        foreach (var g in AllGrids)
        {
            if (g.Age == age) return g;
        }
        return null;
    }

    void OnLandOnGrid(GridData grid)
    {
        Debug.Log($"[BoardManager] Landed: age {grid.Age} - {grid.Title}");

        var player = GameManager.Instance.Player;

        // Speed choice every 20 grids
        if (player.CurrentAge >= player.NextSpeedChoiceAge)
        {
            player.NextSpeedChoiceAge += 20;
            if (UIManager.Instance != null)
                UIManager.Instance.ShowSpeedChoice(null);
            Debug.Log("[BoardManager] Speed choice available");
        }

        // Family background at age 6
        if (player.CurrentAge >= 6 && player.Family == null)
        {
            player.Family = FamilyGenerator.Generate();
            player.Gold = player.Family.InitialGold;
            Debug.Log($"[BoardManager] Family generated, initial gold: {player.Gold}");
        }

        GameManager.Instance.EnterGridWorld(grid.Age);
    }

    void OnNaturalDeath()
    {
        Debug.Log("[BoardManager] Natural death at 100+");
        GameManager.Instance.ChangeState(GameState.Death);
    }

    /// <summary>
    /// Reset token to start
    /// </summary>
    public void ResetBoard()
    {
        CurrentGridIndex = 0;
        if (PlayerToken != null)
        {
            Vector3 startPos = GetCellPosition(0);
            startPos.y += 1f;
            PlayerToken.position = startPos;
        }
    }
}