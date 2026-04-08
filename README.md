# LifeGame - 人生模拟器

大富翁式桌游 + 开放世界叙事的人生体验游戏。

## 架构概览

```
Assets/Scripts/
├── Core/               # 核心系统
│   ├── GameManager     # 游戏总管理器（单例，状态机，场景调度）
│   ├── GameEnums       # 全局枚举定义
│   └── SaveSystem      # JSON存档系统
├── Data/               # 数据结构
│   ├── PlayerData      # 玩家数据（年龄、金币、善恶值、家庭）
│   └── GridData        # 格子数据定义
├── BoardGame/          # 桌游层
│   ├── BoardManager    # 棋盘管理（格子导航、骰子触发）
│   ├── DiceSystem      # 骰子系统（快/慢模式）
│   ├── FamilyGenerator # 家庭背景随机生成
│   └── KarmaJudge      # 善恶判定 & 转世系统
├── OpenWorld/          # 开放世界层
│   ├── PlayerController# 第三人称角色控制
│   ├── IInteractable   # 交互接口
│   └── EventTrigger    # 事件触发器
├── UI/
│   └── UIManager       # UI面板管理
└── Audio/
    └── AudioManager    # BGM/音效管理（按年龄阶段切换）
```

## 核心循环

主菜单 → 摇骰子(桌游层) → 进入格子世界(开放世界) → 完成事件 → 返回桌游层 → 循环
                                                    ↓ (死亡)
                                              善恶判定 → 天堂/地狱 → 转世

## 技术选型

- 引擎: Unity 2022 LTS
- 语言: C#
- 存档: JSON (JsonUtility)
- 格子数据: JSON配置驱动
