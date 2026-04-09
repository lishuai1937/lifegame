using System;
using UnityEngine;

/// <summary>
/// Health System - Physical health, mental health, injuries, diseases
/// 
/// Health affects: death probability, social energy, movement speed, event options
/// Mental health affects: willpower checks, NPC interaction quality, world event outcomes
/// </summary>
[Serializable]
public class HealthSystem
{
    // Physical (0-100)
    public int PhysicalHealth = 100;
    public int MentalHealth = 100;

    // Conditions
    public bool HasChronicIllness = false;
    public bool IsInjured = false;
    public bool IsDepressed = false;
    public bool IsAddicted = false;
    public string ChronicIllnessName = "";

    /// <summary>
    /// Natural aging decay - called each grid world
    /// </summary>
    public void AgeDecay(int age)
    {
        if (age > 40) PhysicalHealth = Math.Max(0, PhysicalHealth - 1);
        if (age > 60) PhysicalHealth = Math.Max(0, PhysicalHealth - 2);
        if (age > 80) PhysicalHealth = Math.Max(0, PhysicalHealth - 3);

        // Depression from isolation or hardship
        if (MentalHealth < 30) IsDepressed = true;
        if (MentalHealth > 50) IsDepressed = false;
    }

    public void TakeDamage(int physical, int mental)
    {
        PhysicalHealth = Math.Max(0, PhysicalHealth - physical);
        MentalHealth = Math.Max(0, MentalHealth - mental);
    }

    public void Heal(int physical, int mental)
    {
        PhysicalHealth = Math.Min(100, PhysicalHealth + physical);
        MentalHealth = Math.Min(100, MentalHealth + mental);
    }

    public void DevelopChronicIllness(string name)
    {
        HasChronicIllness = true;
        ChronicIllnessName = name;
        PhysicalHealth = Math.Max(0, PhysicalHealth - 20);
    }

    /// <summary>
    /// Death probability modifier from health
    /// </summary>
    public float GetDeathModifier()
    {
        if (PhysicalHealth < 20) return 0.15f;
        if (PhysicalHealth < 40) return 0.08f;
        if (PhysicalHealth < 60) return 0.03f;
        return 0f;
    }

    /// <summary>
    /// Social energy modifier from health
    /// </summary>
    public int GetEnergyModifier()
    {
        if (IsDepressed) return -2;
        if (PhysicalHealth < 30) return -1;
        if (MentalHealth > 80 && PhysicalHealth > 80) return 1;
        return 0;
    }
}