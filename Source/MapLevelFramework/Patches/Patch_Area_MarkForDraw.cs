using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using Verse;

namespace MapLevelFramework.Patches
{
    /// <summary>
    /// Area.MarkForDraw 补丁 - 聚焦层级时，让子地图的 Area 正确显示。
    ///
    /// 原版 Area.MarkForDraw 检查 this.Map == Find.CurrentMap。
    /// 聚焦层级时 Find.CurrentMap 仍是基地图，导致子地图 Area 不显示。
    /// 替换为 CurrentInteractionMap 后，子地图 Area 可以正确绘制。
    /// </summary>
    [HarmonyPatch(typeof(Area), "MarkForDraw")]
    public static class Patch_Area_MarkForDraw
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var findCurrentMap = AccessTools.PropertyGetter(typeof(Find), "CurrentMap");
            var ourCurrentMap = AccessTools.PropertyGetter(typeof(LevelManager), "CurrentInteractionMap");
            return Transpilers.MethodReplacer(instructions, findCurrentMap, ourCurrentMap);
        }
    }
}
