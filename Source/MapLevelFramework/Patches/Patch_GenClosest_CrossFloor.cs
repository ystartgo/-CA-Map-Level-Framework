using System;
using System.Collections.Generic;
using HarmonyLib;
using Verse;
using Verse.AI;

namespace MapLevelFramework.CrossFloor
{
    /// <summary>
    /// [已禁用] 底层 GenClosest patch 会破坏原版 WorkGiver 的同地图假设，
    /// 导致 "provided target but yielded no actual job" 和无限 job 循环。
    /// 跨层工作改由 Patch_JobGiver_Work_CrossFloor 在 job 分配层面处理。
    /// </summary>
    // [HarmonyPatch(typeof(GenClosest), nameof(GenClosest.ClosestThingReachable))]
    public static class Patch_GenClosest_CrossFloor
    {
        public static void Postfix(
            ref Thing __result,
            IntVec3 root,
            Map map,
            ThingRequest thingReq,
            PathEndMode peMode,
            TraverseParms traverseParams,
            float maxDistance,
            Predicate<Thing> validator,
            IEnumerable<Thing> customGlobalSearchSet)
        {
            if (__result != null) return;
            if (map == null) return;
            if (!map.IsPartOfFloorSystem()) return;

            // 有自定义搜索集时不跨层（调用者有特殊意图）
            if (customGlobalSearchSet != null) return;

            __result = GenClosestCrossFloor.ClosestThingOnOtherFloors(
                root, map, thingReq, peMode, traverseParams,
                maxDistance, validator);
        }
    }
}
