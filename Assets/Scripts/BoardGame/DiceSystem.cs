using UnityEngine;

/// <summary>
/// 骰子系统
/// 根据玩家选择的快慢模式，生成不同范围的点数
/// </summary>
public static class DiceSystem
{
    /// <summary>
    /// 摇骰子
    /// </summary>
    /// <param name="speed">快慢模式</param>
    /// <returns>骰子点数</returns>
    public static int Roll(DiceSpeed speed)
    {
        switch (speed)
        {
            case DiceSpeed.Slow:
                return Random.Range(1, 4);  // 1-3
            case DiceSpeed.Fast:
                return Random.Range(3, 7);  // 3-6
            default:
                return Random.Range(1, 7);  // 1-6
        }
    }
}
