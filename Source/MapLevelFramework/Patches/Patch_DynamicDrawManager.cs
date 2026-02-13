using HarmonyLib;
using RimWorld;
using Verse;

namespace MapLevelFramework.Patches
{
    /// <summary>
    /// SectionLayer_ThingsGeneral.TakePrintFrom 补丁 -
    /// 聚焦层级时，跳过主地图上位于层级 area 内的物体，
    /// 防止它们被烘焙进 SectionLayer mesh（建筑、地板装饰等）。
    ///
    /// 这比 patch Thing.Print 更可靠，因为 TakePrintFrom 是所有
    /// 静态物体渲染的统一入口，不受 Thing 子类 override 影响。
    /// </summary>
    [HarmonyPatch(typeof(SectionLayer_ThingsGeneral), "TakePrintFrom")]
    public static class Patch_SectionLayer_ThingsGeneral_TakePrintFrom
    {
        public static bool Prefix(Thing t)
        {
            var filter = LevelManager.ActiveRenderFilter;
            if (filter == null) return true;
            if (filter.hostMap != t.Map) return true;
            return !filter.ContainsBaseMapCell(t.Position);
        }
    }

    /// <summary>
    /// Thing.DynamicDrawPhase 补丁 -
    /// 聚焦层级时，跳过主地图上位于层级 area 内的动态物体（Pawn、物品等）。
    /// DynamicDrawManager.DrawDynamicThings 直接调用此方法。
    /// </summary>
    [HarmonyPatch(typeof(Thing), nameof(Thing.DynamicDrawPhase))]
    public static class Patch_Thing_DynamicDrawPhase
    {
        public static bool Prefix(Thing __instance)
        {
            var filter = LevelManager.ActiveRenderFilter;
            if (filter == null) return true;
            if (filter.hostMap != __instance.Map) return true;
            return !filter.ContainsBaseMapCell(__instance.Position);
        }
    }
}
