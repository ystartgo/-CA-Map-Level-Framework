using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using Verse;

namespace MapLevelFramework.Patches
{
    /// <summary>
    /// Designator.Map 补丁 - 聚焦层级时，让建造指示器指向子地图。
    /// 
    /// 原版 Designator.Map 返回 Find.CurrentMap。
    /// 聚焦层级时需要返回子地图，这样建造操作会在子地图上执行。
    /// 
    /// 参照 VMF 的 Patch_Designator_Map。
    /// </summary>
    [HarmonyPatch(typeof(Designator), "Map", MethodType.Getter)]
    public static class Patch_Designator_Map
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var findCurrentMap = AccessTools.PropertyGetter(typeof(Find), "CurrentMap");
            var ourCurrentMap = AccessTools.PropertyGetter(typeof(LevelManager), "CurrentInteractionMap");

            return Transpilers.MethodReplacer(instructions, findCurrentMap, ourCurrentMap);
        }
    }
}
