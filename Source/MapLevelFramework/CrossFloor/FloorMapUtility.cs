using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace MapLevelFramework.CrossFloor
{
    /// <summary>
    /// 楼层地图工具方法。
    /// 类似 VMF 的 VehicleMapUtility，提供跨楼层地图的基础查询。
    /// </summary>
    public static class FloorMapUtility
    {
        /// <summary>
        /// 获取基地图。如果已是基地图则返回自身。
        /// </summary>
        public static Map GetBaseMap(this Map map)
        {
            if (map == null) return null;
            if (LevelManager.IsLevelMap(map, out var parentMgr, out _))
                return parentMgr.map;
            return map;
        }

        /// <summary>
        /// 获取基地图 + 所有楼层子地图。
        /// </summary>
        public static IEnumerable<Map> BaseMapAndFloorMaps(this Map map)
        {
            Map baseMap = map.GetBaseMap();
            if (baseMap == null) yield break;

            yield return baseMap;

            LevelManager mgr = LevelManager.GetManager(baseMap);
            if (mgr == null) yield break;

            foreach (var level in mgr.AllLevels)
            {
                if (level.LevelMap != null)
                    yield return level.LevelMap;
            }
        }

        /// <summary>
        /// 是否属于楼层系统（基地图有 LevelManager，或自身是子地图）。
        /// </summary>
        public static bool IsPartOfFloorSystem(this Map map)
        {
            if (map == null) return false;
            if (LevelManager.IsLevelMap(map, out _, out _)) return true;
            LevelManager mgr = LevelManager.GetManager(map);
            return mgr != null && CrossLevelUtility.HasLevels(map);
        }

        /// <summary>
        /// 获取地图的楼层高度。基地图=0，子地图=对应 elevation。
        /// </summary>
        public static int GetMapElevation(Map map)
        {
            if (map == null) return 0;
            if (LevelManager.IsLevelMap(map, out _, out var levelData))
                return levelData.elevation;
            return 0;
        }

        /// <summary>
        /// 在指定地图上找到通往目标楼层的最近可达楼梯。
        /// </summary>
        public static Building_Stairs FindStairsToElevation(Pawn pawn, Map map, int targetElevation)
        {
            var stairs = StairsCache.GetStairs(map, targetElevation);
            if (stairs == null || stairs.Count == 0) return null;

            Building_Stairs best = null;
            float bestDist = float.MaxValue;

            for (int i = 0; i < stairs.Count; i++)
            {
                var s = stairs[i];
                if (!s.Spawned) continue;
                if (!pawn.CanReach(s, PathEndMode.OnCell, Danger.Deadly)) continue;

                float dist = s.Position.DistanceToSquared(pawn.Position);
                if (dist < bestDist)
                {
                    best = s;
                    bestDist = dist;
                }
            }

            return best;
        }
    }
}
