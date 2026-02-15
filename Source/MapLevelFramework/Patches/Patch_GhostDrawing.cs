using HarmonyLib;
using Verse;

namespace MapLevelFramework.Patches
{
    /// <summary>
    /// Designator 系列补丁 - 聚焦层级时，临时切换 Find.CurrentMap 到子地图，
    /// 让 Designator.Map（= Find.CurrentMap）返回子地图。
    ///
    /// 影响范围：
    /// - DesignatorManagerUpdate → Designator.SelectedUpdate → 蓝图幽灵绘制、放置检查
    /// - DesignationManagerOnGUI → DrawMouseAttachments → 鼠标附件（深层资源等）
    /// - ProcessInputEvents → CanDesignateCell / DesignateSingleCell → 实际放置
    ///
    /// 使用 Patch_Overlays 中的共用 SwapToLevelMap/RestoreLevelMap。
    /// </summary>
    public static class Patch_GhostDrawing
    {
        /// <summary>
        /// DesignatorManagerUpdate 调用 Designator.SelectedUpdate，
        /// 其中 base.Map = Find.CurrentMap 用于蓝图幽灵颜色判定和交互格绘制。
        /// </summary>
        [HarmonyPatch(typeof(DesignatorManager), "DesignatorManagerUpdate")]
        public static class Patch_DesignatorManagerUpdate
        {
            public static void Prefix() => Patch_Overlays.SwapToLevelMap();
            public static void Finalizer() => Patch_Overlays.RestoreLevelMap();
        }

        /// <summary>
        /// DesignationManagerOnGUI 调用 Designator.DrawMouseAttachments，
        /// 其中 Find.CurrentMap 用于深层资源网格和距离数字显示。
        /// </summary>
        [HarmonyPatch(typeof(DesignatorManager), "DesignationManagerOnGUI")]
        public static class Patch_DesignationManagerOnGUI
        {
            public static void Prefix() => Patch_Overlays.SwapToLevelMap();
            public static void Finalizer() => Patch_Overlays.RestoreLevelMap();
        }

        /// <summary>
        /// ProcessInputEvents 调用 CanDesignateCell 和 DesignateSingleCell，
        /// 确保蓝图放置在子地图上而非基地图。
        /// </summary>
        [HarmonyPatch(typeof(DesignatorManager), "ProcessInputEvents")]
        public static class Patch_ProcessInputEvents
        {
            public static void Prefix() => Patch_Overlays.SwapToLevelMap();
            public static void Finalizer() => Patch_Overlays.RestoreLevelMap();
        }
    }
}
