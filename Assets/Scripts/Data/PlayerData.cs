using System;

/// <summary>
/// 玩家存档数据
/// </summary>
[Serializable]
public class PlayerData
{
    // 基础信息
    public string PlayerName = "旅人";
    public int Gender = 0; // 0=男 1=女

    // 人生进度
    public int CurrentAge = 0;          // 当前年龄（=格子位置）
    public WorldRealm CurrentRealm = WorldRealm.Mortal;
    public int ReincarnationCount = 0;  // 转世次数

    // 经济
    public int Gold = 0;

    // 善恶值（决定死后去天堂还是地狱）
    public int KarmaValue = 0;  // 正=善 负=恶

    // 骰子
    public DiceSpeed CurrentDiceSpeed = DiceSpeed.Slow;
    public int NextSpeedChoiceAge = 20; // 下次可选快慢的年龄

    // 家庭背景（6岁后生成）
    public FamilyBackground Family;

    // 成就/物品
    public string[] UnlockedItems = Array.Empty<string>();
    public string[] Achievements = Array.Empty<string>();

    // 灵魂记忆（转世后继承）
    public string[] SoulMemories = Array.Empty<string>();

    /// <summary>
    /// 获取当前年龄阶段
    /// </summary>
    public AgePhase GetAgePhase()
    {
        if (CurrentAge <= 12) return AgePhase.Childhood;
        if (CurrentAge <= 17) return AgePhase.Youth;
        if (CurrentAge <= 30) return AgePhase.Young;
        if (CurrentAge <= 50) return AgePhase.Prime;
        if (CurrentAge <= 65) return AgePhase.Middle;
        return AgePhase.Elder;
    }
}

/// <summary>
/// 家庭背景数据
/// </summary>
[Serializable]
public class FamilyBackground
{
    public int WealthLevel = 1;     // 1-5 家庭财富等级
    public int FamilySize = 3;      // 家庭人数
    public string FamilyTrait = ""; // 家庭特质（如"书香门第""经商世家"）
    public int InitialGold = 0;     // 初始资金（6岁后根据家庭生成）
}
