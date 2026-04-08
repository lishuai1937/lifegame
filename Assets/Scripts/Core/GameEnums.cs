/// <summary>
/// 游戏主状态
/// </summary>
public enum GameState
{
    MainMenu,       // 主菜单
    BoardGame,      // 桌游层（摇骰子阶段）
    OpenWorld,      // 开放世界（格子内部）
    CG,             // CG过场
    Death,          // 死亡判定
    Reincarnation   // 转世
}

/// <summary>
/// 世界层级（天堂/人间/地狱）
/// </summary>
public enum WorldRealm
{
    Heaven,     // 天堂
    Mortal,     // 人间
    Hell        // 地狱
}

/// <summary>
/// 年龄阶段
/// </summary>
public enum AgePhase
{
    Childhood,  // 童年 1-12
    Youth,      // 少年 13-17
    Young,      // 青年 18-30
    Prime,      // 壮年 31-50
    Middle,     // 中年 51-65
    Elder       // 老年 66-100
}

/// <summary>
/// 骰子速度选择
/// </summary>
public enum DiceSpeed
{
    Slow,   // 慢（小点数）
    Fast    // 快（大点数）
}

/// <summary>
/// 格子类型
/// </summary>
public enum GridType
{
    Normal,     // 普通格子
    Event,      // 事件格子
    Death,      // 死亡格子
    Milestone   // 里程碑格子（如高考、结婚等）
}
