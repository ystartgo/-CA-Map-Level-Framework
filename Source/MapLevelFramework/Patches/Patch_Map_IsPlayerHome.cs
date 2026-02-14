using HarmonyLib;
using Verse;

namespace MapLevelFramework.Patches
{
    /// <summary>
    /// Map.IsPlayerHome 补丁 - 让子地图继承宿主地图的 IsPlayerHome 状态。
    ///
    /// 子地图是 pocketMap，原版 IsPlayerHome 返回 false。
    /// 很多系统（alert、hauling、工作分配、思想等）依赖 IsPlayerHome。
    /// 如果宿主地图是玩家基地，子地图也应该被视为玩家基地。
    /// </summary>
    [HarmonyPatch(typeof(Map), "get_IsPlayerHome")]
    public static class Patch_Map_IsPlayerHome
    {
        public static void Postfix(Map __instance, ref bool __result)
        {
            if (__result) return; // 已经是 true，不需要修改

            if (LevelManager.IsLevelMap(__instance, out var manager, out _))
            {
                __result = manager.map.IsPlayerHome;
            }
        }
    }
}
