using HarmonyLib;
using Verse;

namespace MapLevelFramework.Patches
{
    /// <summary>
    /// 温度同步补丁 - 子地图的室外温度和季节温度从基地图同步。
    ///
    /// 子地图是 pocketMap，没有真实的天气系统。
    /// 虽然我们在 GenerateLevelMap 中共享了 weatherManager，
    /// 但 MapTemperature 的 OutdoorTemp 和 SeasonalTemp 可能仍然不正确。
    /// 这个 patch 确保子地图的温度始终和基地图一致。
    /// </summary>
    public static class Patch_Temperature
    {
        private static bool accessingOutdoor;
        private static bool accessingSeasonal;

        [HarmonyPatch(typeof(MapTemperature), "get_OutdoorTemp")]
        public static class Patch_OutdoorTemp
        {
            public static bool Prefix(Map ___map, ref float __result)
            {
                if (accessingOutdoor) return true; // 防止递归

                if (!LevelManager.IsLevelMap(___map, out var manager, out _))
                    return true;

                accessingOutdoor = true;
                __result = manager.map.mapTemperature.OutdoorTemp;
                accessingOutdoor = false;
                return false;
            }
        }

        [HarmonyPatch(typeof(MapTemperature), "get_SeasonalTemp")]
        public static class Patch_SeasonalTemp
        {
            public static bool Prefix(Map ___map, ref float __result)
            {
                if (accessingSeasonal) return true;

                if (!LevelManager.IsLevelMap(___map, out var manager, out _))
                    return true;

                accessingSeasonal = true;
                __result = manager.map.mapTemperature.SeasonalTemp;
                accessingSeasonal = false;
                return false;
            }
        }
    }
}
