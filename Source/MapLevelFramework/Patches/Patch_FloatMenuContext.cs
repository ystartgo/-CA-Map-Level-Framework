using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace MapLevelFramework.Patches
{
    /// <summary>
    /// FloatMenuMakerMap.GetOptions 补丁 - 聚焦层级时，重定向右键菜单到子地图。
    ///
    /// GetOptions 内部使用 Find.CurrentMap 和 clickPos。
    /// 聚焦层级时需要：
    /// 1. 将 clickPos 转换到子地图坐标
    /// 2. 临时切换 currentMapIndex 到子地图
    /// </summary>
    [HarmonyPatch(typeof(FloatMenuMakerMap), "GetOptions",
        new[] { typeof(List<Pawn>), typeof(Vector3), typeof(FloatMenuContext) },
        new[] { ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Out })]
    public static class Patch_FloatMenuMakerMap_GetOptions
    {
        private static sbyte savedMapIndex = -1;

        public static void Prefix(ref Vector3 clickPos)
        {
            savedMapIndex = -1;

            var baseMap = Find.CurrentMap;
            if (baseMap == null) return;

            var mgr = LevelManager.GetManager(baseMap);
            if (mgr == null || !mgr.IsFocusingLevel) return;

            var level = mgr.GetLevel(mgr.FocusedElevation);
            if (level?.LevelMap == null) return;

            IntVec3 baseCell = IntVec3Utility.ToIntVec3(clickPos);
            if (!level.ContainsBaseMapCell(baseCell)) return;

            // 转换坐标到子地图
            clickPos = clickPos.ToLevelCoord(level);

            // 临时切换到子地图（直接设 currentMapIndex，绕过 Notify_SwitchedMap）
            int subMapIndex = Find.Maps.IndexOf(level.LevelMap);
            if (subMapIndex >= 0)
            {
                savedMapIndex = Current.Game.currentMapIndex;
                Current.Game.currentMapIndex = (sbyte)subMapIndex;
            }
        }

        public static void Finalizer()
        {
            if (savedMapIndex >= 0)
            {
                Current.Game.currentMapIndex = (sbyte)savedMapIndex;
                savedMapIndex = -1;
            }
        }
    }
}
