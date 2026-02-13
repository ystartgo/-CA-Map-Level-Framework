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

            // 鼠标在层级区域内时，始终返回我们的结果（即使为空），
            // 防止 fallthrough 到原版逻辑在主地图搜索（坐标已被转换，会搜错位置）
            __result = objects;
            return false;
        }
    }

    /// <summary>
    /// Selector.SelectInternal 补丁 - 防止选中子地图物体时切换地图和跳转视角。
    ///
    /// 原版逻辑：如果 thing.MapHeld != Find.CurrentMap，会切换到 thing 的地图并跳转摄像机。
    /// 子地图物体的 MapHeld 是子地图（不同于主地图），会触发地图切换 + 摄像机跳到本地坐标(0,0)。
    ///
    /// 修复：选中子地图物体时，直接设 Game.currentMapIndex（绕过 setter 的
    /// Notify_SwitchedMap 通知），使 map == Find.CurrentMap 为 true，跳过地图切换逻辑。
    /// 注意：Game.currentMapIndex 是 public sbyte 字段，可以直接访问。
    /// </summary>
    [HarmonyPatch(typeof(Selector), "SelectInternal")]
    public static class Patch_Selector_SelectInternal
    {
        private static sbyte savedMapIndex = -1;

        public static void Prefix(object obj)
        {
            savedMapIndex = -1;

            Thing thing = obj as Thing;
            if (thing?.Map == null) return;

            LevelManager manager;
            LevelData level;
            if (LevelManager.IsLevelMap(thing.Map, out manager, out level))
            {
                int subMapIndex = Find.Maps.IndexOf(thing.Map);
                if (subMapIndex >= 0)
                {
                    savedMapIndex = Current.Game.currentMapIndex;
                    Current.Game.currentMapIndex = (sbyte)subMapIndex;
                }
            }
        }

        public static void Finalizer()
        {
            if (savedMapIndex >= 0)
            {
                Current.Game.currentMapIndex = savedMapIndex;
                savedMapIndex = -1;
            }
        }
    }
}
