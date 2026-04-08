using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// Inventory system - special items + assets
/// 
/// Special items:
/// - Regret Pill: go back to a previous grid, re-enter that world, then return
/// - Time Rewind: extend time in current grid world
/// 
/// Assets:
/// - Houses, cars, etc. that affect Charm stat
/// </summary>
public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }

    [Header("Special Items")]
    public int RegretPills = 0;
    public int TimeRewinds = 0;

    [Header("Assets")]
    public List<OwnedAsset> Assets = new List<OwnedAsset>();

    [Header("Stats")]
    public int Charm = 0; // influenced by assets, affects NPC interactions

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    // ==================== Special Items ====================

    public void AddRegretPill()
    {
        RegretPills++;
        Debug.Log($"[Inventory] Got Regret Pill! Total: {RegretPills}");
    }

    public void AddTimeRewind()
    {
        TimeRewinds++;
        Debug.Log($"[Inventory] Got Time Rewind! Total: {TimeRewinds}");
    }

    /// <summary>
    /// Use Regret Pill on the board
    /// Returns true if successful
    /// targetAge: which grid to go back to
    /// </summary>
    public bool UseRegretPill(int targetAge, BoardManager board)
    {
        if (RegretPills <= 0) return false;
        if (board == null || GameManager.Instance == null) return false;

        var player = GameManager.Instance.Player;
        int currentAge = player.CurrentAge;

        if (targetAge >= currentAge || targetAge < 1) return false;

        RegretPills--;

        // Store return point (the grid BEFORE current)
        int returnAge = currentAge;

        // Go back to target age
        player.CurrentAge = targetAge;
        board.CurrentGridIndex = targetAge;

        Debug.Log($"[Inventory] Regret Pill used! Back to age {targetAge}, will return to {returnAge}");

        // The flow: enter target grid world -> on exit, return to returnAge
        // Cannot re-enter current grid world
        // This needs to be handled by GameFlowController

        return true;
    }

    /// <summary>
    /// Use Time Rewind in grid world
    /// Extends the forced exit timer
    /// </summary>
    public bool UseTimeRewind(float extraSeconds = 60f)
    {
        if (TimeRewinds <= 0) return false;

        TimeRewinds--;
        Debug.Log($"[Inventory] Time Rewind used! +{extraSeconds}s in grid world");

        // The actual timer extension is handled by GridWorldTimer
        return true;
    }

    // ==================== Assets ====================

    public bool BuyAsset(string assetId, string assetName, int price, int charmBonus)
    {
        var player = GameManager.Instance.Player;
        if (player.Gold < price) return false;

        player.Gold -= price;
        Assets.Add(new OwnedAsset
        {
            AssetId = assetId,
            AssetName = assetName,
            PurchasePrice = price,
            CharmBonus = charmBonus
        });

        RecalculateCharm();
        Debug.Log($"[Inventory] Bought {assetName} for {price}g. Charm now: {Charm}");
        return true;
    }

    public bool SellAsset(string assetId)
    {
        for (int i = 0; i < Assets.Count; i++)
        {
            if (Assets[i].AssetId == assetId)
            {
                int sellPrice = Assets[i].PurchasePrice / 2; // sell at half price
                GameManager.Instance.Player.Gold += sellPrice;
                Assets.RemoveAt(i);
                RecalculateCharm();
                Debug.Log($"[Inventory] Sold {assetId} for {sellPrice}g");
                return true;
            }
        }
        return false;
    }

    public bool OwnsAsset(string assetId)
    {
        foreach (var a in Assets)
            if (a.AssetId == assetId) return true;
        return false;
    }

    void RecalculateCharm()
    {
        Charm = 0;
        foreach (var a in Assets)
            Charm += a.CharmBonus;
    }

    public void Reset()
    {
        RegretPills = 0;
        TimeRewinds = 0;
        Assets.Clear();
        Charm = 0;
    }
}

[Serializable]
public class OwnedAsset
{
    public string AssetId;
    public string AssetName;
    public int PurchasePrice;
    public int CharmBonus;
}