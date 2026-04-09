using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Housing System - Buy house, choose layout, fill furniture slots
/// 
/// Flow: Buy housing level -> Pick floor plan -> Buy furniture for slots
/// Furniture gives stat bonuses, NPC visitors comment on your home
/// </summary>
[Serializable]
public class HousingSystem
{
    public HousingLevel Level = HousingLevel.None;
    public FloorPlan CurrentPlan;
    public List<PlacedFurniture> Furniture = new List<PlacedFurniture>();

    // ==================== Floor Plans ====================

    public static readonly FloorPlanTemplate[] AllPlans = {
        // Rental - 1 room, 2 slots
        new FloorPlanTemplate(HousingLevel.Rental, "Studio", new[] {
            new FurnitureSlot("Bed Area", SlotType.Bedroom),
            new FurnitureSlot("Living Corner", SlotType.Living)
        }),
        // Small Apartment - 2 rooms, 4 slots
        new FloorPlanTemplate(HousingLevel.SmallApartment, "1-Bedroom", new[] {
            new FurnitureSlot("Bedroom", SlotType.Bedroom),
            new FurnitureSlot("Living Room", SlotType.Living),
            new FurnitureSlot("Kitchen", SlotType.Kitchen),
            new FurnitureSlot("Bathroom", SlotType.Bathroom)
        }),
        new FloorPlanTemplate(HousingLevel.SmallApartment, "Open Plan", new[] {
            new FurnitureSlot("Main Space", SlotType.Living),
            new FurnitureSlot("Sleep Nook", SlotType.Bedroom),
            new FurnitureSlot("Kitchenette", SlotType.Kitchen),
            new FurnitureSlot("Balcony", SlotType.Outdoor)
        }),
        // House - 4 rooms, 6 slots
        new FloorPlanTemplate(HousingLevel.House, "Family Home", new[] {
            new FurnitureSlot("Master Bedroom", SlotType.Bedroom),
            new FurnitureSlot("Kid's Room", SlotType.Bedroom),
            new FurnitureSlot("Living Room", SlotType.Living),
            new FurnitureSlot("Kitchen", SlotType.Kitchen),
            new FurnitureSlot("Bathroom", SlotType.Bathroom),
            new FurnitureSlot("Garden", SlotType.Outdoor)
        }),
        new FloorPlanTemplate(HousingLevel.House, "Modern House", new[] {
            new FurnitureSlot("Bedroom", SlotType.Bedroom),
            new FurnitureSlot("Guest Room", SlotType.Bedroom),
            new FurnitureSlot("Open Living", SlotType.Living),
            new FurnitureSlot("Kitchen+Dining", SlotType.Kitchen),
            new FurnitureSlot("Study", SlotType.Study),
            new FurnitureSlot("Garage", SlotType.Outdoor)
        }),
        // Big House - 6 rooms, 8 slots
        new FloorPlanTemplate(HousingLevel.BigHouse, "Luxury Home", new[] {
            new FurnitureSlot("Master Suite", SlotType.Bedroom),
            new FurnitureSlot("Bedroom 2", SlotType.Bedroom),
            new FurnitureSlot("Bedroom 3", SlotType.Bedroom),
            new FurnitureSlot("Grand Living", SlotType.Living),
            new FurnitureSlot("Gourmet Kitchen", SlotType.Kitchen),
            new FurnitureSlot("Home Office", SlotType.Study),
            new FurnitureSlot("Bathroom", SlotType.Bathroom),
            new FurnitureSlot("Backyard", SlotType.Outdoor)
        }),
        // Villa - 8 rooms, 10 slots
        new FloorPlanTemplate(HousingLevel.Villa, "Estate", new[] {
            new FurnitureSlot("Master Suite", SlotType.Bedroom),
            new FurnitureSlot("Guest Suite 1", SlotType.Bedroom),
            new FurnitureSlot("Guest Suite 2", SlotType.Bedroom),
            new FurnitureSlot("Grand Hall", SlotType.Living),
            new FurnitureSlot("Entertainment Room", SlotType.Living),
            new FurnitureSlot("Chef Kitchen", SlotType.Kitchen),
            new FurnitureSlot("Library", SlotType.Study),
            new FurnitureSlot("Spa Bathroom", SlotType.Bathroom),
            new FurnitureSlot("Pool Area", SlotType.Outdoor),
            new FurnitureSlot("Wine Cellar", SlotType.Storage)
        })
    };

    // ==================== Furniture Catalog ====================

    public static readonly FurnitureItem[] Catalog = {
        // Bedroom
        new FurnitureItem("Basic Bed", SlotType.Bedroom, 20, new StatBonus(StatType.Resilience, 0), 1, 0),
        new FurnitureItem("Comfy Bed", SlotType.Bedroom, 80, new StatBonus(StatType.Resilience, 0), 3, 2),
        new FurnitureItem("Luxury Bed", SlotType.Bedroom, 300, new StatBonus(StatType.Resilience, 0), 5, 5),
        // Living
        new FurnitureItem("Old Sofa", SlotType.Living, 30, new StatBonus(StatType.Empathy, 0), 1, 0),
        new FurnitureItem("Nice Sofa", SlotType.Living, 120, new StatBonus(StatType.Empathy, 0), 2, 1),
        new FurnitureItem("TV", SlotType.Living, 100, new StatBonus(StatType.Charisma, 0), 0, 3),
        new FurnitureItem("Big Screen TV", SlotType.Living, 400, new StatBonus(StatType.Charisma, 0), 0, 5),
        new FurnitureItem("Game Console", SlotType.Living, 150, new StatBonus(StatType.Charisma, 0), 0, 4),
        // Kitchen
        new FurnitureItem("Basic Stove", SlotType.Kitchen, 40, new StatBonus(StatType.Resilience, 0), 2, 0),
        new FurnitureItem("Full Kitchen Set", SlotType.Kitchen, 200, new StatBonus(StatType.Resilience, 0), 4, 2),
        new FurnitureItem("Chef Kitchen", SlotType.Kitchen, 600, new StatBonus(StatType.Charisma, 1), 5, 3),
        // Study
        new FurnitureItem("Desk", SlotType.Study, 50, new StatBonus(StatType.Willpower, 0), 0, 0),
        new FurnitureItem("Bookshelf", SlotType.Study, 80, new StatBonus(StatType.Empathy, 1), 0, 0),
        new FurnitureItem("Computer", SlotType.Study, 200, new StatBonus(StatType.Charisma, 0), 0, 2),
        new FurnitureItem("Library Wall", SlotType.Study, 500, new StatBonus(StatType.Empathy, 2), 0, 0),
        // Bathroom
        new FurnitureItem("Basic Bathroom", SlotType.Bathroom, 30, new StatBonus(StatType.Resilience, 0), 1, 0),
        new FurnitureItem("Nice Bathroom", SlotType.Bathroom, 150, new StatBonus(StatType.Resilience, 0), 3, 2),
        // Outdoor
        new FurnitureItem("Small Garden", SlotType.Outdoor, 60, new StatBonus(StatType.Empathy, 1), 2, 1),
        new FurnitureItem("BBQ Set", SlotType.Outdoor, 100, new StatBonus(StatType.Charisma, 1), 1, 3),
        new FurnitureItem("Swimming Pool", SlotType.Outdoor, 800, new StatBonus(StatType.Charisma, 2), 3, 5),
        // Storage
        new FurnitureItem("Wine Rack", SlotType.Storage, 200, new StatBonus(StatType.Charisma, 1), 0, 2),
    };

    // ==================== Methods ====================

    public void SetPlan(FloorPlanTemplate template)
    {
        CurrentPlan = new FloorPlan { Name = template.Name, Slots = new List<FurnitureSlot>(template.Slots) };
        Furniture.Clear();
    }

    public bool PlaceFurniture(int slotIndex, FurnitureItem item, ref int gold)
    {
        if (CurrentPlan == null || slotIndex >= CurrentPlan.Slots.Count) return false;
        var slot = CurrentPlan.Slots[slotIndex];
        if (slot.Type != item.FitSlot && item.FitSlot != SlotType.Any) return false;
        if (gold < item.Price) return false;

        gold -= item.Price;
        // Remove old furniture in this slot
        Furniture.RemoveAll(f => f.SlotIndex == slotIndex);
        Furniture.Add(new PlacedFurniture { SlotIndex = slotIndex, Item = item });
        return true;
    }

    /// <summary>
    /// Total health recovery bonus from furniture (applied per grid world)
    /// </summary>
    public int GetHealthBonus()
    {
        int total = 0;
        foreach (var f in Furniture) total += f.Item.HealthBonus;
        return total;
    }

    /// <summary>
    /// Total mental health bonus
    /// </summary>
    public int GetMentalBonus()
    {
        int total = 0;
        foreach (var f in Furniture) total += f.Item.MentalBonus;
        return total;
    }

    /// <summary>
    /// Get all stat bonuses from furniture
    /// </summary>
    public List<StatBonus> GetStatBonuses()
    {
        var bonuses = new List<StatBonus>();
        foreach (var f in Furniture)
            if (f.Item.Bonus.Value > 0) bonuses.Add(f.Item.Bonus);
        return bonuses;
    }

    /// <summary>
    /// NPC impression when visiting (affects closeness gain)
    /// </summary>
    public string GetNPCImpression()
    {
        int totalValue = 0;
        foreach (var f in Furniture) totalValue += f.Item.Price;
        int slotsFilled = Furniture.Count;
        int totalSlots = CurrentPlan != null ? CurrentPlan.Slots.Count : 0;

        if (totalSlots == 0) return "You don't have a home to invite anyone to.";
        float fillRate = (float)slotsFilled / totalSlots;

        if (totalValue > 2000 && fillRate > 0.8f) return "Wow, your place is amazing!";
        if (totalValue > 800 && fillRate > 0.6f) return "Nice place! Very cozy.";
        if (fillRate > 0.4f) return "It's a decent place.";
        if (fillRate > 0) return "It's a bit empty, but it's yours.";
        return "There's... nothing here.";
    }

    /// <summary>
    /// Closeness bonus when NPC visits based on home quality
    /// </summary>
    public int GetVisitClosenessBonus()
    {
        int totalValue = 0;
        foreach (var f in Furniture) totalValue += f.Item.Price;
        if (totalValue > 2000) return 5;
        if (totalValue > 800) return 3;
        if (totalValue > 200) return 1;
        return 0;
    }
}

// ==================== Data Types ====================

[Serializable]
public class FloorPlan
{
    public string Name;
    public List<FurnitureSlot> Slots;
}

[Serializable]
public class FloorPlanTemplate
{
    public HousingLevel MinLevel;
    public string Name;
    public FurnitureSlot[] Slots;
    public FloorPlanTemplate(HousingLevel l, string n, FurnitureSlot[] s) { MinLevel = l; Name = n; Slots = s; }
}

[Serializable]
public class FurnitureSlot
{
    public string Name;
    public SlotType Type;
    public FurnitureSlot(string n, SlotType t) { Name = n; Type = t; }
}

[Serializable]
public class FurnitureItem
{
    public string Name;
    public SlotType FitSlot;
    public int Price;
    public StatBonus Bonus;
    public int HealthBonus;     // physical health recovery
    public int MentalBonus;     // mental health recovery
    public FurnitureItem(string n, SlotType s, int p, StatBonus b, int h, int m)
    { Name = n; FitSlot = s; Price = p; Bonus = b; HealthBonus = h; MentalBonus = m; }
}

[Serializable]
public class StatBonus
{
    public StatType Stat;
    public int Value;
    public StatBonus(StatType s, int v) { Stat = s; Value = v; }
}

[Serializable]
public class PlacedFurniture
{
    public int SlotIndex;
    public FurnitureItem Item;
}

public enum SlotType { Bedroom, Living, Kitchen, Bathroom, Study, Outdoor, Storage, Any }