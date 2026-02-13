using HarmonyLib;
using Verse;

namespace MapLevelFramework.Patches
{
    /// <summary>
    /// MapDrawer 补丁 - 在主地图渲染完成后，叠加渲染聚焦层级的内容。
    /// </summary>
    [HarmonyPatch(typeof(MapDrawer), "DrawMapMesh")]
    public static class Patch_MapDrawer_DrawMapMesh
    {
        public static void Postfix(MapDrawer __instance)
        {
            Map baseMap = Traverse.Create(__instance).Field("map").GetValue<Map>();
            if (baseMap == null) return;

            var mgr = LevelManager.GetManager(baseMap);
            if (mgr == null || !mgr.IsFocusingLevel) return;

            var level = mgr.GetLevel(mgr.FocusedElevation);
            if (level?.LevelMap == null) return;

            // 子地图的 MapDrawer 不在主循环中更新，手动触发 section 重建
            Render.LevelRenderer.UpdateLevelMapSections(level.LevelMap);

            var offset = LevelCoordUtility.GetDrawOffset(level);

            // 叠加渲染子地图的静态 mesh（Y 偏移确保覆盖主地图地形）
            Render.LevelRenderer.DrawLevelMapMesh(level.LevelMap, offset);

            // 叠加渲染子地图的动态 Thing
            Render.LevelRenderer.DrawLevelDynamicThings(level.LevelMap, level);

            // 叠加渲染子地图的覆盖层（designations、overlays、flecks 等）
            Render.LevelRenderer.DrawLevelOverlays(level.LevelMap, level);
        }
    }
}
