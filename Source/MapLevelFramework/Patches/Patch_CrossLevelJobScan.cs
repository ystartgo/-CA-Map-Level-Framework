using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace MapLevelFramework.Patches
{
    /// <summary>
    /// 跨层级工作扫描 - 让 pawn 在当前地图找不到工作时，自动去其他楼层找工作。
    ///
    /// 核心机制：
    /// 1. Postfix JobGiver_Work.TryIssueJobPackage
    /// 2. 当前地图没工作时，临时"传送"pawn 到其他层级
    /// 3. 调用原版 TryIssueJobPackage 看有没有工作
    /// 4. 有工作就创建 MLF_UseStairs job，让 pawn 走到楼梯上/下楼
    /// 5. 到了另一层后 pawn 的 AI 自然会找到工作
    /// </summary>
    [HarmonyPatch(typeof(JobGiver_Work), "TryIssueJobPackage")]
    public static class Patch_CrossLevelJobScan
    {
        private static bool scanning;

        // 反射字段：Thing.mapIndexOrState 和 Thing.positionInt
        private static readonly FieldInfo mapIndexField =
            typeof(Thing).GetField("mapIndexOrState",
                BindingFlags.Instance | BindingFlags.NonPublic);
        private static readonly FieldInfo positionField =
            typeof(Thing).GetField("positionInt",
                BindingFlags.Instance | BindingFlags.NonPublic);

        public static void Postfix(
            ref ThinkResult __result,
            JobGiver_Work __instance,
            Pawn pawn,
            JobIssueParams jobParams)
        {
            // 防止递归
            if (scanning) return;

            // 只在没找到工作时触发
            if (__result != ThinkResult.NoJob) return;

            // pawn 必须在地图上
            if (pawn?.Map == null || !pawn.Spawned) return;

            // 获取层级信息
            Map pawnMap = pawn.Map;
            LevelManager mgr;
            Map baseMap;

            if (LevelManager.IsLevelMap(pawnMap, out var parentMgr, out _))
            {
                mgr = parentMgr;
                baseMap = parentMgr.map;
            }
            else
            {
                mgr = LevelManager.GetManager(pawnMap);
                baseMap = pawnMap;
            }

            if (mgr == null || !mgr.AllLevels.Any()) return;

            int currentElev = GetMapElevation(pawnMap, mgr, baseMap);

            // 收集要扫描的其他层级地图，按距离当前层远近排序
            var otherMaps = new List<(Map map, int elevation)>();
            if (pawnMap != baseMap)
                otherMaps.Add((baseMap, 0));
            foreach (var level in mgr.AllLevels)
            {
                if (level.LevelMap != null && level.LevelMap != pawnMap)
                    otherMaps.Add((level.LevelMap, level.elevation));
            }

            if (otherMaps.Count == 0) return;

            // 按距离当前层的远近排序（优先扫描相邻层）
            otherMaps.Sort((a, b) =>
                System.Math.Abs(a.elevation - currentElev)
                    .CompareTo(System.Math.Abs(b.elevation - currentElev)));

            // 保存 pawn 原始状态
            sbyte origMapIndex = (sbyte)mapIndexField.GetValue(pawn);
            IntVec3 origPos = (IntVec3)positionField.GetValue(pawn);

            scanning = true;
            try
            {
                foreach (var (otherMap, targetElev) in otherMaps)
                {
                    // 确定方向：一次走一层
                    int nextElev = targetElev > currentElev
                        ? currentElev + 1
                        : currentElev - 1;

                    // 先找当前地图上通往该方向的楼梯
                    Building_Stairs stairs = FindStairsToElevation(
                        pawn, pawnMap, nextElev);
                    if (stairs == null) continue;

                    // 用楼梯位置作为临时坐标（楼梯在两张地图上同位置，且一定可通行）
                    IntVec3 stairPos = stairs.Position;
                    if (!stairPos.InBounds(otherMap)) continue;

                    // 临时传送 pawn 到目标地图的楼梯位置
                    sbyte destMapIndex = (sbyte)Find.Maps.IndexOf(otherMap);
                    if (destMapIndex < 0) continue;

                    mapIndexField.SetValue(pawn, destMapIndex);
                    positionField.SetValue(pawn, stairPos);

                    try
                    {
                        ThinkResult result = __instance.TryIssueJobPackage(pawn, jobParams);
                        if (result != ThinkResult.NoJob)
                        {
                            // 找到工作了！恢复 pawn 位置，派去楼梯
                            mapIndexField.SetValue(pawn, origMapIndex);
                            positionField.SetValue(pawn, origPos);

                            Job stairJob = JobMaker.MakeJob(
                                MLF_JobDefOf.MLF_UseStairs, stairs);
                            __result = new ThinkResult(
                                stairJob, __instance, result.Tag, false);
                            return;
                        }
                    }
                    catch (System.Exception ex)
                    {
                        Log.Warning($"[MLF] Cross-level job scan error on map {otherMap.uniqueID}: {ex.Message}");
                    }
                }
            }
            finally
            {
                // 确保恢复 pawn 原始状态
                mapIndexField.SetValue(pawn, origMapIndex);
                positionField.SetValue(pawn, origPos);
                scanning = false;
            }
        }

        private static int GetMapElevation(Map map, LevelManager mgr, Map baseMap)
        {
            if (map == baseMap) return 0;
            var level = mgr.GetLevelForMap(map);
            return level?.elevation ?? 0;
        }

        /// <summary>
        /// 找到当前地图上通往指定 elevation 的最近楼梯。
        /// </summary>
        private static Building_Stairs FindStairsToElevation(Pawn pawn, Map map, int targetElevation)
        {
            Building_Stairs best = null;
            float bestDist = float.MaxValue;

            var things = map.listerThings.ThingsOfDef(
                DefDatabase<ThingDef>.GetNamedSilentFail("MLF_Stairs"));
            if (things == null) return null;

            foreach (Thing t in things)
            {
                if (t is Building_Stairs stairs && stairs.Spawned
                    && stairs.targetElevation == targetElevation)
                {
                    float dist = stairs.Position.DistanceToSquared(pawn.Position);
                    if (dist < bestDist && pawn.CanReach(stairs, PathEndMode.OnCell, Danger.Some))
                    {
                        best = stairs;
                        bestDist = dist;
                    }
                }
            }
            return best;
        }
    }
}
