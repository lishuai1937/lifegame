using UnityEngine;

/// <summary>
/// 善恶判定系统
/// 死亡后根据KarmaValue决定去天堂还是地狱
/// </summary>
public static class KarmaJudge
{
    /// <summary>
    /// 判定死后去向
    /// </summary>
    public static WorldRealm Judge(PlayerData player)
    {
        // 善恶值 > 0 去天堂，< 0 去地狱，= 0 随机
        if (player.KarmaValue > 0)
        {
            Debug.Log($"[KarmaJudge] 善恶值{player.KarmaValue}，判定: 天堂");
            return WorldRealm.Heaven;
        }
        else if (player.KarmaValue < 0)
        {
            Debug.Log($"[KarmaJudge] 善恶值{player.KarmaValue}，判定: 地狱");
            return WorldRealm.Hell;
        }
        else
        {
            var result = Random.value > 0.5f ? WorldRealm.Heaven : WorldRealm.Hell;
            Debug.Log($"[KarmaJudge] 善恶值为0，随机判定: {result}");
            return result;
        }
    }

    /// <summary>
    /// 转世 - 重置数据但保留灵魂记忆
    /// </summary>
    public static PlayerData Reincarnate(PlayerData oldPlayer)
    {
        var newPlayer = new PlayerData
        {
            ReincarnationCount = oldPlayer.ReincarnationCount + 1,
            SoulMemories = oldPlayer.Achievements, // 成就变为灵魂记忆
            KarmaValue = 0,
            Gold = 0,
            CurrentAge = 0,
            CurrentRealm = WorldRealm.Mortal
        };

        Debug.Log($"[KarmaJudge] 转世成功，第{newPlayer.ReincarnationCount}世");
        return newPlayer;
    }
}
