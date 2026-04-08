using UnityEngine;

/// <summary>
/// 家庭背景生成器
/// 6岁时触发，随机生成家庭背景并决定初始资金
/// </summary>
public static class FamilyGenerator
{
    private static readonly string[] FamilyTraits = new[]
    {
        "书香门第", "经商世家", "务农之家", "军人家庭",
        "艺术世家", "医学世家", "普通家庭", "单亲家庭",
        "孤儿", "富豪之家"
    };

    private static readonly int[] WealthToGold = new[]
    {
        50,     // 等级1: 贫困
        200,    // 等级2: 普通
        500,    // 等级3: 小康
        1500,   // 等级4: 富裕
        5000    // 等级5: 富豪
    };

    public static FamilyBackground Generate()
    {
        var family = new FamilyBackground();

        // 随机家庭财富等级（加权，中间等级概率更高）
        float roll = Random.value;
        if (roll < 0.15f) family.WealthLevel = 1;
        else if (roll < 0.45f) family.WealthLevel = 2;
        else if (roll < 0.75f) family.WealthLevel = 3;
        else if (roll < 0.92f) family.WealthLevel = 4;
        else family.WealthLevel = 5;

        family.FamilySize = Random.Range(2, 7);
        family.FamilyTrait = FamilyTraits[Random.Range(0, FamilyTraits.Length)];
        family.InitialGold = WealthToGold[family.WealthLevel - 1];

        Debug.Log($"[FamilyGenerator] 生成家庭: {family.FamilyTrait}, 财富等级{family.WealthLevel}, 初始资金{family.InitialGold}");
        return family;
    }
}
