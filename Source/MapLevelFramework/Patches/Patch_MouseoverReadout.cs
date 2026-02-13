using HarmonyLib;
using Verse;

namespace MapLevelFramework.Patches
{
    /// <summary>
    /// MouseoverReadout.MouseoverReadoutOnGUI 补丁 - 聚焦层级时，
    /// 临时切换 currentMapIndex 到子地图，使底部信息栏显示子地图信息。
    ///
    /// MouseoverReadoutOnGUI 内部大量使用 Find.CurrentMap 和 UI.MouseCell()。
    /// UI.MouseCell() 已被 Patch_UI_MouseCell 转换到子地图坐标，
    /// 但 Find.CurrentMap 仍指向主地图，导致查询错误的地形/物体。
    ///
    /// 使用 Finalizer 确保即使发生异常也能恢复。
    /// </summary>
    [HarmonyPatch(typeof(MouseoverReadout), "MouseoverReadoutOnGUI")]
    public static class Patch_MouseoverReadout_OnGUI
    {
        private static sbyte savedMapIndex = -1;

        public static void Prefix()
        {
            savedMapIndex = -1;

            var baseMap = Find.CurrentMap;
            if (baseMap == null) return;

            var mgr = LevelManager.GetManager(baseMap);
            if (mgr == null || !mgr.IsFocusingLevel) return;

            var level = mgr.GetLevel(mgr.FocusedElevation);
            if (level?.LevelMap == null) return;

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
