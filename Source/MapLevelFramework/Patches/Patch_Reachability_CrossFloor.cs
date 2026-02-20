using System;
using HarmonyLib;
using Verse;
using Verse.AI;

namespace MapLevelFramework.CrossFloor
{
    /// <summary>
    /// [已禁用] 底层 Reachability patch 导致 "Called CanReach() with a pawn spawned not on this map"
    /// 以及 validator 链中的跨地图错误。
    /// 跨层工作改由 Patch_JobGiver_Work_CrossFloor 在 job 分配层面处理。
    /// </summary>
    // [HarmonyPatch(typeof(Reachability), nameof(Reachability.CanReach),
    //     new Type[] { typeof(IntVec3), typeof(LocalTargetInfo), typeof(PathEndMode), typeof(TraverseParms) })]
    public static class Patch_Reachability_CrossFloor
    {
        public static void Postfix(
            ref bool __result,
            IntVec3 start,
            LocalTargetInfo dest,
            PathEndMode peMode,
            TraverseParms traverseParams,
            Map ___map)
        {
            if (__result) return;
            if (CrossFloorReachabilityUtility.working) return;

            // 只处理 Thing 目标在其他楼层的情况
            Thing destThing = dest.Thing;
            if (destThing == null) return;

            Map thisMap = ___map;
            Map destMap = destThing.MapHeld;
            if (destMap == null || destMap == thisMap) return;

            // 确认两个地图属于同一楼层系统
            if (!thisMap.IsPartOfFloorSystem()) return;
            if (thisMap.GetBaseMap() != destMap.GetBaseMap()) return;

            __result = CrossFloorReachabilityUtility.CanReach(
                thisMap, start, destMap, destThing.PositionHeld,
                peMode, traverseParams);
        }
    }
}
