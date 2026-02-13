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

            var offset = LevelCoordUtility.GetDrawOffset(level);

            // 叠加渲染子地图的静态 mesh
            Render.LevelRenderer.DrawLevelMapMesh(level.LevelMap, offset);

            // 叠加渲染子地图的动态 Thing
            Render.LevelRenderer.DrawLevelDynamicThings(level.LevelMap, level);
        }
    }
}
