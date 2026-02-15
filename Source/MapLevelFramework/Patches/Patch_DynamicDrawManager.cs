using HarmonyLib;
using RimWorld;
using Verse;

namespace MapLevelFramework.Patches
{
    /// <summary>
    /// SectionLayer_ThingsGeneral.TakePrintFrom 补丁 -
    /// 聚焦层级时：
    /// 1. 跳过主地图上位于任何中间层级 area 内的物体
    /// 2. 跳过中间层子地图上被更高层覆盖的物体
    /// 防止它们被烘焙进 SectionLayer mesh（建筑、地板装饰、物品等）。
    /// </summary>
    [HarmonyPatch(typeof(SectionLayer_ThingsGeneral), "TakePrintFrom")]
    public static class Patch_SectionLayer_ThingsGeneral_TakePrintFrom
    {
        public static bool Prefix(Thing t)
        {
            var filter = LevelManager.ActiveRenderFilter;
            if (filter == null) return true;

            // 基地图物体：跳过位于任何层级区域内的
            if (filter.hostMap == t.Map)
                return !LevelManager.IsInActiveRenderArea(t.Position);

            // 子地图物体：跳过被更高层覆盖的
            return !LevelManager.IsCoveredByHigherLevel(t.Map, t.Position);
        }
    }

    /// <summary>
    /// Thing.DynamicDrawPhase 补丁 -
    /// 聚焦层级时，跳过主地图上位于任何中间层级 area 内的动态物体（Pawn、物品等）。
    /// </summary>
    [HarmonyPatch(typeof(Thing), nameof(Thing.DynamicDrawPhase))]
    public static class Patch_Thing_DynamicDrawPhase
    {
        public static bool Prefix(Thing __instance)
        {
            var filter = LevelManager.ActiveRenderFilter;
            if (filter == null) return true;
            if (filter.hostMap != __instance.Map) return true;
            return !LevelManager.IsInActiveRenderArea(__instance.Position);
        }
    }
}
