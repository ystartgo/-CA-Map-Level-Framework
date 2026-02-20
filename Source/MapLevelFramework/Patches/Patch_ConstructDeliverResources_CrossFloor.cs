using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace MapLevelFramework.CrossFloor
{
    /// <summary>
    /// 跨层材料自动传送（已禁用 — 改用 HaulToStairs 物理搬运）。
    /// 当原版 ResourceDeliverJobFor 找不到材料时，从其他楼层传送过来。
    /// 参考 Digital Storage 的做法：Postfix 在材料搜索失败后介入，
    /// 从其他楼层 DeSpawn → Spawn 到蓝图附近，然后创建正常的 HaulToContainer job。
    /// </summary>
    //[HarmonyPatch(typeof(WorkGiver_ConstructDeliverResources),
    //    "ResourceDeliverJobFor")]
    public static class Patch_ConstructDeliverResources_CrossFloor
    {
        public static void Postfix(
            ref Job __result,
            Pawn pawn,
            IConstructible c)
        {
            if (__result != null) return;

            Thing constructible = c as Thing;
            if (constructible == null) return;

            Map buildMap = constructible.Map;
            if (buildMap == null) return;
            if (!buildMap.IsPartOfFloorSystem()) return;

            // 遍历材料需求
            foreach (var cost in c.TotalMaterialCost())
            {
                int needed = c.ThingCountNeeded(cost.thingDef);
                if (needed <= 0) continue;

                // 从其他楼层找材料
                Thing source = FindMaterialOnOtherFloors(
                    buildMap, cost.thingDef, needed);
                if (source == null) continue;

                // 传送到蓝图附近
                Thing spawned = TeleportMaterial(
                    source, needed, buildMap, constructible.Position);
                if (spawned == null) continue;

                // 创建正常的 HaulToContainer job
                Job job = JobMaker.MakeJob(JobDefOf.HaulToContainer);
                job.targetA = spawned;
                job.targetB = constructible;
                job.targetC = constructible;
                job.count = spawned.stackCount;
                job.haulMode = HaulMode.ToContainer;
                __result = job;
                return;
            }
        }

        /// <summary>
        /// 在其他楼层搜索指定材料。
        /// </summary>
        private static Thing FindMaterialOnOtherFloors(
            Map buildMap, ThingDef matDef, int minCount)
        {
            Thing best = null;

            foreach (Map otherMap in buildMap.BaseMapAndFloorMaps())
            {
                if (otherMap == buildMap) continue;

                var things = otherMap.listerThings.ThingsOfDef(matDef);
                for (int i = 0; i < things.Count; i++)
                {
                    Thing t = things[i];
                    if (!t.Spawned) continue;
                    if (t.IsForbidden(Faction.OfPlayer)) continue;

                    // 优先选数量足够的、离楼梯近的
                    if (best == null || t.stackCount > best.stackCount)
                    {
                        best = t;
                    }
                }
            }

            return best;
        }

        /// <summary>
        /// 将材料从源位置传送到目标地图的指定位置附近。
        /// 如果源堆叠大于需求量，只拆分需要的部分。
        /// </summary>
        private static Thing TeleportMaterial(
            Thing source, int needed, Map destMap, IntVec3 destPos)
        {
            int toTake = System.Math.Min(needed, source.stackCount);

            Thing toSpawn;
            if (toTake >= source.stackCount)
            {
                // 整堆传送
                source.DeSpawn(DestroyMode.Vanish);
                toSpawn = source;
            }
            else
            {
                // 拆分
                toSpawn = source.SplitOff(toTake);
            }

            // 在蓝图附近找一个可用位置
            IntVec3 spawnPos = destPos;
            if (!spawnPos.Standable(destMap) ||
                spawnPos.GetFirstItem(destMap) != null)
            {
                // 找附近空位
                if (!CellFinder.TryFindRandomCellNear(
                    destPos, destMap, 3,
                    cell => cell.Standable(destMap) &&
                            cell.GetFirstItem(destMap) == null,
                    out spawnPos))
                {
                    spawnPos = destPos; // fallback
                }
            }

            return GenSpawn.Spawn(toSpawn, spawnPos, destMap);
        }
    }
}