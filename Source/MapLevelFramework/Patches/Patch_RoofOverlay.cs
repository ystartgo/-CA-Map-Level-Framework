using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace MapLevelFramework.Patches
{
    /// <summary>
    /// 屋顶覆盖层补丁 - 聚焦层级时，屋顶覆盖层显示子地图的屋顶数据。
    ///
    /// RoofGrid 实现 ICellBoolGiver，CellBoolDrawer 用 GetCellBool/GetCellExtraColor
    /// 来决定哪些格子显示屋顶覆盖。聚焦层级时，层级区域内的格子应显示子地图的屋顶。
    /// </summary>
    public static class Patch_RoofOverlay
    {
        [HarmonyPatch(typeof(RoofGrid), "GetCellBool")]
        public static class Patch_GetCellBool
        {
            public static void Postfix(int index, ref bool __result, Map ___map)
            {
                var filter = LevelManager.ActiveRenderFilter;
                if (filter?.LevelMap == null) return;

                var mgr = LevelManager.GetManager(___map);
                if (mgr == null || !mgr.IsFocusingLevel) return;

                IntVec3 cell = ___map.cellIndices.IndexToCell(index);
                if (!filter.ContainsBaseMapCell(cell)) return;

                // 用子地图的屋顶数据替换
                __result = filter.LevelMap.roofGrid.Roofed(index);
            }
        }

        [HarmonyPatch(typeof(RoofGrid), "GetCellExtraColor")]
        public static class Patch_GetCellExtraColor
        {
            public static void Postfix(int index, ref Color __result, Map ___map)
            {
                var filter = LevelManager.ActiveRenderFilter;
                if (filter?.LevelMap == null) return;

                var mgr = LevelManager.GetManager(___map);
                if (mgr == null || !mgr.IsFocusingLevel) return;

                IntVec3 cell = ___map.cellIndices.IndexToCell(index);
                if (!filter.ContainsBaseMapCell(cell)) return;

                RoofDef roof = filter.LevelMap.roofGrid.RoofAt(index);
                __result = (RoofDefOf.RoofRockThick != null && roof == RoofDefOf.RoofRockThick)
                    ? Color.gray
                    : Color.white;
            }
        }
    }
}
