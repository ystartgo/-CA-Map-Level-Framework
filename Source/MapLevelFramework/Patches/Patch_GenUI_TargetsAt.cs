using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace MapLevelFramework.Patches
{
    /// <summary>
    /// GenUI.TargetsAt 补丁 - 聚焦层级时，从子地图获取点击目标。
    /// 使用 GetTopmostLevelAt 查询点击位置的最高可见层级。
    /// 同尺寸子地图方案：坐标一致，无需转换。
    /// </summary>
    [HarmonyPatch(typeof(GenUI), "TargetsAt")]
    public static class Patch_GenUI_TargetsAt
    {
        public static bool Prefix(Vector3 clickPos, TargetingParameters clickParams,
            bool thingsOnly, ITargetingSource source,
            ref IEnumerable<LocalTargetInfo> __result)
        {
            var baseMap = Find.CurrentMap;
            if (baseMap == null) return true;

            var mgr = LevelManager.GetManager(baseMap);
            if (mgr == null || !mgr.IsFocusingLevel) return true;

            IntVec3 cell = IntVec3Utility.ToIntVec3(clickPos);
            var topLevel = LevelManager.GetTopmostLevelAt(cell);
            if (topLevel?.LevelMap == null) return true;
            if (!cell.InBounds(topLevel.LevelMap)) return true;

            var results = new List<LocalTargetInfo>();

            foreach (Thing thing in topLevel.LevelMap.thingGrid.ThingsAt(cell))
            {
                if (clickParams.CanTarget(thing, source))
                {
                    results.Add(thing);
                }
            }

            if (!thingsOnly && results.Count == 0)
            {
                if (cell.Walkable(topLevel.LevelMap))
                {
                    results.Add(cell);
                }
            }

            __result = results;
            return false;
        }
    }
}
