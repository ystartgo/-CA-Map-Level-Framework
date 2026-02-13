using HarmonyLib;
using Verse;

namespace MapLevelFramework.Patches
{
    /// <summary>
    /// Game.CurrentMap setter 补丁 - 防止直接切换到子地图。
    /// 如果尝试切换到层级子地图，改为切换到其宿主主地图并聚焦该层级。
    /// 
    /// 参照 VMF 的 Patch_Game_CurrentMap。
    /// </summary>
    [HarmonyPatch(typeof(Game), "CurrentMap", MethodType.Setter)]
    public static class Patch_Game_CurrentMap
    {
        public static void Prefix(ref Map value)
        {
            if (value == null) return;

            // 检查是否是层级子地图
            if (LevelManager.IsLevelMap(value, out var manager, out var levelData))
            {
                // 不要切换到子地图，而是切换到宿主主地图并聚焦该层级
                if (manager.map != null && manager.map.Index >= 0)
                {
                    value = manager.map;
                    manager.FocusLevel(levelData.elevation);
                }
            }
        }
    }
}
