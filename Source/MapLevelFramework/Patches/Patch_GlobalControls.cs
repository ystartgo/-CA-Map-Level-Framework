using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;

namespace MapLevelFramework.Patches
{
    /// <summary>
    /// GlobalControls.TemperatureString 补丁 - 聚焦层级时温度显示使用子地图数据。
    ///
    /// TemperatureString() 大量使用 Find.CurrentMap 获取鼠标位置的房间/温度。
    /// 聚焦层级时需要使用子地图来获取正确的温度信息。
    /// </summary>
    [HarmonyPatch(typeof(GlobalControls), "TemperatureString")]
    public static class Patch_GlobalControls_TemperatureString
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var findCurrentMap = AccessTools.PropertyGetter(typeof(Find), "CurrentMap");
            var ourCurrentMap = AccessTools.PropertyGetter(typeof(LevelManager), "CurrentInteractionMap");
            return Transpilers.MethodReplacer(instructions, findCurrentMap, ourCurrentMap);
        }
    }
}
