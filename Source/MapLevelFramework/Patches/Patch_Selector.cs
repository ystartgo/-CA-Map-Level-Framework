using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace MapLevelFramework.Patches
{
    /// <summary>
    /// Selector.SelectableObjectsUnderMouse 补丁 - 聚焦层级时，从子地图获取可选物体。
    /// 
    /// 参照 VMF 的 Patch_Selector_SelectableObjectsUnderMouse。
    /// </summary>
    [HarmonyPatch(typeof(Selector), "SelectableObjectsUnderMouse")]
    public static class Patch_Selector_SelectableObjectsUnderMouse
    {
        public static bool Prefix(ref IEnumerable<object> __result)
        {
            var baseMap = Find.CurrentMap;
            if (baseMap == null) return true;

            var mgr = LevelManager.GetManager(baseMap);
            if (mgr == null || !mgr.IsFocusingLevel) return true;

            var level = mgr.GetLevel(mgr.FocusedElevation);
            if (level?.LevelMap == null) return true;

            Vector3 mousePos = UI.MouseMapPosition();
            IntVec3 baseCell = IntVec3Utility.ToIntVec3(mousePos);

            if (!level.ContainsBaseMapCell(baseCell)) return true;

            // 转换到子地图坐标
            IntVec3 levelCell = baseCell.ToLevelCoord(level);
            if (!levelCell.InBounds(level.LevelMap)) return true;

            // 收集子地图上的可选物体
            var objects = new List<object>();

            // Pawns（按距离排序）
            Vector3 levelMousePos = mousePos.ToLevelCoord(level);
            foreach (Pawn pawn in level.LevelMap.mapPawns.AllPawnsSpawned)
            {
                float dist = (pawn.DrawPos - levelMousePos).MagnitudeHorizontal();
                if (dist < 0.4f)
                {
                    objects.Add(pawn);
                }
            }

            // Things at cell
            foreach (Thing thing in level.LevelMap.thingGrid.ThingsAt(levelCell))
            {
                if (!objects.Contains(thing))
                {
                    objects.Add(thing);
                }
            }

            if (objects.Count > 0)
            {
                __result = objects;
                return false;
            }

            return true;
        }
    }
}
