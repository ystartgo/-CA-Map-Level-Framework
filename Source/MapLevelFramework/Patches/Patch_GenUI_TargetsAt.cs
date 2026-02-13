using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace MapLevelFramework.Patches
{
    /// <summary>
    /// GenUI.TargetsAt 补丁 - 聚焦层级时，从子地图获取点击目标。
    /// 
    /// 参照 VMF 的 Patch_GenUI_TargetsAt。
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

            var level = mgr.GetLevel(mgr.FocusedElevation);
            if (level?.LevelMap == null) return true;

            // 检查点击位置是否在 area 内
            IntVec3 baseCell = IntVec3Utility.ToIntVec3(clickPos);
            if (!level.ContainsBaseMapCell(baseCell)) return true;

            // 转换坐标到子地图
            Vector3 levelPos = clickPos.ToLevelCoord(level);
            IntVec3 levelCell = IntVec3Utility.ToIntVec3(levelPos);

            if (!levelCell.InBounds(level.LevelMap)) return true;

            // 从子地图获取目标
            var results = new List<LocalTargetInfo>();

            // 获取该格子的 Things
            foreach (Thing thing in level.LevelMap.thingGrid.ThingsAt(levelCell))
            {
                if (clickParams.CanTarget(thing, source))
                {
                    results.Add(thing);
                }
            }

            // 如果不是仅 Things，也返回格子本身
            if (!thingsOnly && results.Count == 0)
            {
                if (levelCell.InBounds(level.LevelMap) && levelCell.Walkable(level.LevelMap))
                {
                    results.Add(levelCell);
                }
            }

            __result = results;
            return false;
        }
    }
}
