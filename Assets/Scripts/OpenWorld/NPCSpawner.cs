using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Generates NPCs in bulk for a grid world scene
/// Each grid world gets ~100 NPCs with unique personalities
/// NPCs are distributed by role based on scene type and player age
/// </summary>
public class NPCSpawner : MonoBehaviour
{
    public static NPCSpawner Instance { get; private set; }

    // Name pools
    static readonly string[] MaleNames = {
        "Wei","Jun","Hao","Ming","Lei","Feng","Chao","Peng","Bo","Kai",
        "Tao","Jie","Xin","Yang","Zhi","Long","Shan","Rui","Yong","Qiang",
        "Bin","Gang","Liang","Dong","Fei","Nan","Hai","Wen","Zhong","Da",
        "Guo","Ping","An","Cheng","Xiang","Yi","Zhen","Kang","Bao","Sheng",
        "Hua","Jian","Shuai","Hang","Ran","Xu","Chen","Lin","Song","Ye"
    };
    static readonly string[] FemaleNames = {
        "Mei","Ling","Xia","Yan","Fang","Jing","Hui","Yue","Xue","Qing",
        "Li","Na","Ying","Zhen","Rong","Lan","Yun","Shan","Wen","Xin",
        "Dan","Hong","Juan","Min","Shu","Ting","Yu","Zhi","Ai","Bei",
        "Cui","Die","Er","Fen","Ge","Han","Jia","Ke","Lu","Man",
        "Ni","Ou","Pei","Qi","Ru","Si","Tian","Wan","Xi","Yi"
    };
    static readonly string[] Surnames = {
        "Wang","Li","Zhang","Liu","Chen","Yang","Zhao","Huang","Zhou","Wu",
        "Xu","Sun","Ma","Zhu","Hu","Guo","Lin","He","Gao","Luo",
        "Zheng","Liang","Xie","Tang","Han","Cao","Deng","Xiao","Feng","Cheng"
    };

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    /// <summary>
    /// Generate NPC profiles for a grid world (no GameObjects yet)
    /// </summary>
    public List<NPCProfile> GenerateNPCPool(string sceneId, int playerAge, int count = 100)
    {
        var pool = new List<NPCProfile>();
        var r = new System.Random(sceneId.GetHashCode() + playerAge * 7);

        // Determine role distribution based on scene
        var roles = GetRoleDistribution(sceneId, playerAge);

        for (int i = 0; i < count; i++)
        {
            // Pick role
            NPCRole role = roles[r.Next(roles.Count)];

            // Generate name
            int gender = r.Next(2);
            string firstName = gender == 0
                ? MaleNames[r.Next(MaleNames.Length)]
                : FemaleNames[r.Next(FemaleNames.Length)];
            string surname = Surnames[r.Next(Surnames.Length)];
            string fullName = surname + " " + firstName;

            // Generate profile with seeded random
            var profile = new NPCProfile
            {
                Id = $"npc_{sceneId}_{playerAge}_{i}",
                Name = fullName,
                Gender = gender,
                Age = playerAge + r.Next(-10, 11), // NPC age varies around player age
                Kindness = r.Next(0, 11),
                Ambition = r.Next(0, 11),
                Humor = r.Next(0, 11),
                Loyalty = r.Next(0, 11),
                Temper = r.Next(0, 11),
                Introversion = r.Next(0, 11),
                Role = role
            };

            // Clamp NPC age to reasonable range
            if (profile.Age < 1) profile.Age = 1;
            if (role == NPCRole.Child) profile.Age = Mathf.Min(profile.Age, 12);
            if (role == NPCRole.Elder) profile.Age = Mathf.Max(profile.Age, 60);
            if (role == NPCRole.Authority) profile.Age = Mathf.Max(profile.Age, playerAge + 5);

            pool.Add(profile);
        }

        Debug.Log($"[NPCSpawner] Generated {count} NPCs for {sceneId} age {playerAge}");
        return pool;
    }

    List<NPCRole> GetRoleDistribution(string sceneId, int age)
    {
        var roles = new List<NPCRole>();

        if (sceneId.Contains("school") || sceneId.Contains("kindergarten") || sceneId.Contains("university"))
        {
            // School: mostly classmates + some teachers
            for (int i = 0; i < 60; i++) roles.Add(NPCRole.Classmate);
            for (int i = 0; i < 10; i++) roles.Add(NPCRole.Authority); // teachers
            for (int i = 0; i < 10; i++) roles.Add(NPCRole.Stranger);
            for (int i = 0; i < 10; i++) roles.Add(NPCRole.Rival);
            for (int i = 0; i < 5; i++) roles.Add(NPCRole.Romantic);
            for (int i = 0; i < 5; i++) roles.Add(NPCRole.Family);
        }
        else if (sceneId.Contains("city") || sceneId.Contains("office") || sceneId.Contains("startup"))
        {
            for (int i = 0; i < 40; i++) roles.Add(NPCRole.Colleague);
            for (int i = 0; i < 15; i++) roles.Add(NPCRole.Stranger);
            for (int i = 0; i < 10; i++) roles.Add(NPCRole.Authority); // boss
            for (int i = 0; i < 10; i++) roles.Add(NPCRole.Rival);
            for (int i = 0; i < 10; i++) roles.Add(NPCRole.Romantic);
            for (int i = 0; i < 10; i++) roles.Add(NPCRole.Neighbor);
            for (int i = 0; i < 5; i++) roles.Add(NPCRole.Family);
        }
        else if (sceneId.Contains("hospital"))
        {
            for (int i = 0; i < 30; i++) roles.Add(NPCRole.Stranger); // patients
            for (int i = 0; i < 25; i++) roles.Add(NPCRole.Authority); // doctors/nurses
            for (int i = 0; i < 20; i++) roles.Add(NPCRole.Family);
            for (int i = 0; i < 15; i++) roles.Add(NPCRole.Neighbor);
            for (int i = 0; i < 10; i++) roles.Add(NPCRole.Elder);
        }
        else if (sceneId.Contains("countryside") || sceneId.Contains("farm"))
        {
            for (int i = 0; i < 30; i++) roles.Add(NPCRole.Neighbor);
            for (int i = 0; i < 20; i++) roles.Add(NPCRole.Elder);
            for (int i = 0; i < 20; i++) roles.Add(NPCRole.Family);
            for (int i = 0; i < 15; i++) roles.Add(NPCRole.Stranger);
            for (int i = 0; i < 10; i++) roles.Add(NPCRole.Child);
            for (int i = 0; i < 5; i++) roles.Add(NPCRole.Authority);
        }
        else if (sceneId.Contains("home"))
        {
            for (int i = 0; i < 30; i++) roles.Add(NPCRole.Family);
            for (int i = 0; i < 30; i++) roles.Add(NPCRole.Neighbor);
            for (int i = 0; i < 15; i++) roles.Add(NPCRole.Stranger);
            for (int i = 0; i < 10; i++) roles.Add(NPCRole.Child);
            for (int i = 0; i < 10; i++) roles.Add(NPCRole.Elder);
            for (int i = 0; i < 5; i++) roles.Add(NPCRole.Authority);
        }
        else
        {
            // Default: mixed
            for (int i = 0; i < 30; i++) roles.Add(NPCRole.Stranger);
            for (int i = 0; i < 20; i++) roles.Add(NPCRole.Neighbor);
            for (int i = 0; i < 15; i++) roles.Add(NPCRole.Colleague);
            for (int i = 0; i < 10; i++) roles.Add(NPCRole.Family);
            for (int i = 0; i < 10; i++) roles.Add(NPCRole.Elder);
            for (int i = 0; i < 5; i++) roles.Add(NPCRole.Child);
            for (int i = 0; i < 5; i++) roles.Add(NPCRole.Romantic);
            for (int i = 0; i < 5; i++) roles.Add(NPCRole.Rival);
        }

        return roles;
    }
}