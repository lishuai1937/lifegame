using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Asset/Property System
/// 
/// Assets affect gameplay, NPC relationships, events, and life outcomes.
/// Not just numbers - each asset changes how the world treats you.
/// </summary>
[Serializable]
public class AssetSystem
{
    // Owned assets
    public HousingLevel Housing = HousingLevel.None;
    public VehicleLevel Vehicle = VehicleLevel.None;
    public List<Business> Businesses = new List<Business>();
    public List<Investment> Investments = new List<Investment>();
    public List<Collectible> Collectibles = new List<Collectible>();

    // Total asset value (calculated)
    public int GetTotalAssetValue()
    {
        int total = 0;
        total += GetHousingValue();
        total += GetVehicleValue();
        foreach (var b in Businesses) total += b.Value;
        foreach (var i in Investments) total += i.CurrentValue;
        foreach (var c in Collectibles) total += c.CurrentValue;
        return total;
    }

    // ==================== HOUSING ====================

    public static readonly HousingInfo[] HousingData = {
        new HousingInfo(HousingLevel.None, "Homeless", 0, 0),
        new HousingInfo(HousingLevel.Rental, "Rental Apartment", 100, -10),    // monthly cost
        new HousingInfo(HousingLevel.SmallApartment, "Small Apartment", 500, -5),
        new HousingInfo(HousingLevel.House, "House", 2000, -8),
        new HousingInfo(HousingLevel.BigHouse, "Big House", 5000, -15),
        new HousingInfo(HousingLevel.Villa, "Villa", 15000, -30),
    };

    public int GetHousingValue()
    {
        foreach (var h in HousingData)
            if (h.Level == Housing) return h.BuyPrice;
        return 0;
    }

    public bool CanBuyHousing(HousingLevel level, int gold)
    {
        foreach (var h in HousingData)
            if (h.Level == level) return gold >= h.BuyPrice;
        return false;
    }

    public int BuyHousing(HousingLevel level, ref int gold)
    {
        foreach (var h in HousingData)
        {
            if (h.Level == level)
            {
                if (gold < h.BuyPrice) return -1;
                gold -= h.BuyPrice;
                Housing = level;
                return h.BuyPrice;
            }
        }
        return -1;
    }

    // ==================== VEHICLE ====================

    public static readonly VehicleInfo[] VehicleData = {
        new VehicleInfo(VehicleLevel.None, "Walking", 0),
        new VehicleInfo(VehicleLevel.Bicycle, "Bicycle", 20),
        new VehicleInfo(VehicleLevel.UsedCar, "Used Car", 200),
        new VehicleInfo(VehicleLevel.NiceCar, "Nice Car", 800),
        new VehicleInfo(VehicleLevel.LuxuryCar, "Luxury Car", 3000),
        new VehicleInfo(VehicleLevel.Supercar, "Supercar", 10000),
    };

    public int GetVehicleValue()
    {
        foreach (var v in VehicleData)
            if (v.Level == Vehicle) return v.BuyPrice;
        return 0;
    }

    public int BuyVehicle(VehicleLevel level, ref int gold)
    {
        foreach (var v in VehicleData)
        {
            if (v.Level == level)
            {
                if (gold < v.BuyPrice) return -1;
                gold -= v.BuyPrice;
                Vehicle = level;
                return v.BuyPrice;
            }
        }
        return -1;
    }

    // ==================== BUSINESS ====================

    public Business StartBusiness(string name, int investAmount, ref int gold)
    {
        if (gold < investAmount) return null;
        gold -= investAmount;
        var biz = new Business
        {
            Name = name,
            Value = investAmount,
            MonthlyIncome = Mathf.RoundToInt(investAmount * 0.05f), // 5% monthly return
            IsActive = true
        };
        Businesses.Add(biz);
        return biz;
    }

    // ==================== INVESTMENT ====================

    public Investment MakeInvestment(string name, int amount, ref int gold, InvestmentType type)
    {
        if (gold < amount) return null;
        gold -= amount;
        var inv = new Investment
        {
            Name = name,
            Type = type,
            InitialValue = amount,
            CurrentValue = amount
        };
        Investments.Add(inv);
        return inv;
    }

    /// <summary>
    /// Called each grid world - investments fluctuate, businesses earn income
    /// </summary>
    public int ProcessPassiveIncome(int playerAge, int luck)
    {
        int totalIncome = 0;
        var r = new System.Random(playerAge * 17 + luck);

        // Business income
        foreach (var b in Businesses)
        {
            if (b.IsActive)
            {
                totalIncome += b.MonthlyIncome;
                // 5% chance of business failing
                if (r.Next(100) < 5)
                {
                    b.IsActive = false;
                    Debug.Log($"[Asset] Business '{b.Name}' failed!");
                }
            }
        }

        // Investment fluctuation
        foreach (var inv in Investments)
        {
            float change;
            switch (inv.Type)
            {
                case InvestmentType.Stocks:
                    change = (float)(r.NextDouble() * 0.4 - 0.15); // -15% to +25%
                    break;
                case InvestmentType.RealEstate:
                    change = (float)(r.NextDouble() * 0.15 - 0.02); // -2% to +13%
                    break;
                case InvestmentType.Savings:
                    change = 0.03f; // steady 3%
                    break;
                case InvestmentType.Crypto:
                    change = (float)(r.NextDouble() * 1.0 - 0.4); // -40% to +60% volatile!
                    break;
                default:
                    change = 0;
                    break;
            }

            // Luck affects investment outcomes
            if (luck > 12) change += 0.05f;
            if (luck < 5) change -= 0.05f;

            inv.CurrentValue = Mathf.Max(0, Mathf.RoundToInt(inv.CurrentValue * (1 + change)));
        }

        // Housing maintenance cost
        foreach (var h in HousingData)
        {
            if (h.Level == Housing)
            {
                totalIncome += h.MonthlyCost; // negative = cost
                break;
            }
        }

        return totalIncome;
    }

    // ==================== COLLECTIBLES ====================

    public void AddCollectible(string name, int buyPrice, CollectibleType type)
    {
        Collectibles.Add(new Collectible
        {
            Name = name,
            Type = type,
            BuyPrice = buyPrice,
            CurrentValue = buyPrice,
            AcquiredAge = GameManager.Instance?.Player?.CurrentAge ?? 0
        });
    }

    /// <summary>
    /// Collectibles appreciate over time (especially in old age)
    /// </summary>
    public void AppreciateCollectibles(int playerAge)
    {
        foreach (var c in Collectibles)
        {
            int yearsOwned = playerAge - c.AcquiredAge;
            float appreciation = c.Type switch
            {
                CollectibleType.Painting => 0.08f,      // 8% per year
                CollectibleType.Antique => 0.10f,        // 10% per year
                CollectibleType.Jewelry => 0.05f,        // 5% per year
                CollectibleType.RareBook => 0.06f,       // 6% per year
                CollectibleType.Wine => 0.12f,           // 12% per year (ages well!)
                _ => 0.03f
            };
            c.CurrentValue = Mathf.RoundToInt(c.BuyPrice * (1 + appreciation * yearsOwned));
        }
    }

    // ==================== GAMEPLAY EFFECTS ====================

    /// <summary>
    /// Can player get married? Some NPCs require housing
    /// </summary>
    public bool MeetsMarriageRequirement()
    {
        return Housing >= HousingLevel.SmallApartment;
    }

    /// <summary>
    /// Travel event gold multiplier based on vehicle
    /// </summary>
    public float GetTravelMultiplier()
    {
        return Vehicle switch
        {
            VehicleLevel.None => 1.0f,
            VehicleLevel.Bicycle => 1.1f,
            VehicleLevel.UsedCar => 1.3f,
            VehicleLevel.NiceCar => 1.5f,
            VehicleLevel.LuxuryCar => 2.0f,
            VehicleLevel.Supercar => 2.5f,
            _ => 1.0f
        };
    }

    /// <summary>
    /// Does wealth attract gold-digger NPCs?
    /// </summary>
    public bool AttractsGoldDiggers()
    {
        return GetTotalAssetValue() > 5000 || Vehicle >= VehicleLevel.LuxuryCar || Housing >= HousingLevel.BigHouse;
    }

    /// <summary>
    /// NPC visit willingness based on housing
    /// </summary>
    public float GetVisitWillingness()
    {
        return Housing switch
        {
            HousingLevel.None => 0.1f,
            HousingLevel.Rental => 0.4f,
            HousingLevel.SmallApartment => 0.6f,
            HousingLevel.House => 0.8f,
            HousingLevel.BigHouse => 0.95f,
            HousingLevel.Villa => 1.0f,
            _ => 0.3f
        };
    }

    /// <summary>
    /// Get inheritance value (for NPC children when player dies)
    /// </summary>
    public int GetInheritanceValue()
    {
        return GetTotalAssetValue();
    }

    /// <summary>
    /// Economic crisis - assets lose value
    /// </summary>
    public void EconomicCrisis()
    {
        foreach (var inv in Investments)
            inv.CurrentValue = Mathf.RoundToInt(inv.CurrentValue * 0.5f); // lose 50%
        foreach (var b in Businesses)
        {
            if (UnityEngine.Random.value < 0.3f) b.IsActive = false; // 30% businesses fail
        }
        Debug.Log("[Asset] Economic crisis! Assets devalued.");
    }

    /// <summary>
    /// Bankruptcy - lose everything
    /// </summary>
    public void GoBankrupt()
    {
        Housing = HousingLevel.None;
        Vehicle = VehicleLevel.None;
        Businesses.Clear();
        Investments.Clear();
        // Collectibles might be seized too
        Collectibles.Clear();
        Debug.Log("[Asset] Bankruptcy! All assets lost.");
    }
}

// ==================== Data Types ====================

public enum HousingLevel { None, Rental, SmallApartment, House, BigHouse, Villa }
public enum VehicleLevel { None, Bicycle, UsedCar, NiceCar, LuxuryCar, Supercar }
public enum InvestmentType { Savings, Stocks, RealEstate, Crypto }
public enum CollectibleType { Painting, Antique, Jewelry, RareBook, Wine }

[Serializable]
public class HousingInfo
{
    public HousingLevel Level;
    public string Name;
    public int BuyPrice;
    public int MonthlyCost; // negative = expense
    public HousingInfo(HousingLevel l, string n, int p, int c)
    { Level = l; Name = n; BuyPrice = p; MonthlyCost = c; }
}

[Serializable]
public class VehicleInfo
{
    public VehicleLevel Level;
    public string Name;
    public int BuyPrice;
    public VehicleInfo(VehicleLevel l, string n, int p)
    { Level = l; Name = n; BuyPrice = p; }
}

[Serializable]
public class Business
{
    public string Name;
    public int Value;
    public int MonthlyIncome;
    public bool IsActive;
}

[Serializable]
public class Investment
{
    public string Name;
    public InvestmentType Type;
    public int InitialValue;
    public int CurrentValue;
}

[Serializable]
public class Collectible
{
    public string Name;
    public CollectibleType Type;
    public int BuyPrice;
    public int CurrentValue;
    public int AcquiredAge;
}