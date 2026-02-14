using HarmonyLib;
using RimWorld;
using Verse;

namespace MapLevelFramework.Patches
{
    /// <summary>
    /// 禁止子地图自动扩展 Home Area。
    /// 上层地图不需要自动 Home Area（由玩家手动管理）。
    /// </summary>
    [HarmonyPatch(typeof(AutoHomeAreaMaker), "Notify_BuildingSpawned")]
    public static class Patch_AutoHomeArea_BuildingSpawned
    {
        public static bool Prefix(Thing b)
        {
            if (b?.Map == null) return false;
            return !LevelManager.IsLevelMap(b.Map, out _, out _);
        }
    }

    [HarmonyPatch(typeof(AutoHomeAreaMaker), "Notify_BuildingClaimed")]
    public static class Patch_AutoHomeArea_BuildingClaimed
    {
        public static bool Prefix(Thing b)
        {
            if (b?.Map == null) return false;
            return !LevelManager.IsLevelMap(b.Map, out _, out _);
        }
    }
}
