using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using Verse;

namespace MapLevelFramework.Patches
{
    /// <summary>
    /// 各 Designator_ZoneAdd 子类的 MakeNewZone 直接用 Find.CurrentMap.zoneManager，
    /// 绕过了 Designator.Map 的 patch。统一替换为 CurrentInteractionMap。
    /// </summary>
    [HarmonyPatch]
    public static class Patch_ZoneAdd_MakeNewZone
    {
        static IEnumerable<System.Reflection.MethodBase> TargetMethods()
        {
            yield return AccessTools.Method(typeof(Designator_ZoneAddStockpile), "MakeNewZone");
            yield return AccessTools.Method(typeof(Designator_ZoneAdd_Growing), "MakeNewZone");

            // Fishing zone（Odyssey DLC，可能不存在）
            var fishingType = AccessTools.TypeByName("RimWorld.Designator_ZoneAdd_Fishing");
            if (fishingType != null)
            {
                var m = AccessTools.Method(fishingType, "MakeNewZone");
                if (m != null) yield return m;
            }
        }

        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var findCurrentMap = AccessTools.PropertyGetter(typeof(Find), "CurrentMap");
            var ourCurrentMap = AccessTools.PropertyGetter(typeof(LevelManager), "CurrentInteractionMap");

            return Transpilers.MethodReplacer(instructions, findCurrentMap, ourCurrentMap);
        }
    }
}
