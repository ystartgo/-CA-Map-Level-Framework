using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;
using Verse;

namespace MapLevelFramework.Patches
{
    /// <summary>
    /// UI.MouseCell() 补丁 - 聚焦层级时，将鼠标坐标转换到子地图坐标系。
    /// 
    /// 原版 MouseCell 返回主地图坐标。
    /// 聚焦层级时，如果鼠标在 area 内，需要返回子地图坐标。
    /// 
    /// 参照 VMF 的 Patch_UI_MouseCell。
    /// </summary>
    [HarmonyPatch(typeof(Verse.UI), "MouseCell")]
    public static class Patch_UI_MouseCell
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var list = instructions.ToList();
            var toIntVec3 = AccessTools.Method(typeof(IntVec3Utility), "ToIntVec3", new[] { typeof(Vector3) });
            var convertMethod = AccessTools.Method(typeof(Patch_UI_MouseCell), "ConvertToLevelCoord");

            int idx = list.FindIndex(c => c.opcode == OpCodes.Call && c.OperandIs(toIntVec3));
            if (idx >= 0)
            {
                // 在 ToIntVec3 调用之前插入坐标转换
                list.Insert(idx, new CodeInstruction(OpCodes.Call, convertMethod));
            }

            return list;
        }

        /// <summary>
        /// 如果聚焦层级且鼠标在 area 内，将世界坐标转换到子地图坐标。
        /// </summary>
        public static Vector3 ConvertToLevelCoord(Vector3 worldPos)
        {
            var baseMap = Find.CurrentMap;
            if (baseMap == null) return worldPos;

            var mgr = LevelManager.GetManager(baseMap);
            if (mgr == null || !mgr.IsFocusingLevel) return worldPos;

            var level = mgr.GetLevel(mgr.FocusedElevation);
            if (level == null) return worldPos;

            // 检查鼠标是否在 area 内
            IntVec3 baseCell = IntVec3Utility.ToIntVec3(worldPos);
            if (level.ContainsBaseMapCell(baseCell))
            {
                return worldPos.ToLevelCoord(level);
            }

            return worldPos;
        }
    }
}
