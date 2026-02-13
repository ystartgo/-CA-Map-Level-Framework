using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace MapLevelFramework.Patches
{
    /// <summary>
    /// GhostDrawer.DrawGhostThing 补丁 - 聚焦层级时，将蓝图幽灵的绘制位置
    /// 从子地图本地坐标偏移到主地图对应位置。
    ///
    /// UI.MouseCell() 已被转换为子地图坐标（用于正确放置建筑），
    /// 但幽灵绘制需要在主地图坐标系中显示。
    /// </summary>
    [HarmonyPatch(typeof(GhostDrawer), "DrawGhostThing")]
    public static class Patch_GhostDrawer_DrawGhostThing
    {
        public static void Prefix(ref IntVec3 center)
        {
            var baseMap = Find.CurrentMap;
            if (baseMap == null) return;

            var mgr = LevelManager.GetManager(baseMap);
            if (mgr == null || !mgr.IsFocusingLevel) return;

            var level = mgr.GetLevel(mgr.FocusedElevation);
            if (level == null) return;

            // 子地图坐标 -> 主地图坐标
            center = center.ToBaseCoord(level);
        }
    }

    /// <summary>
    /// GenDraw.DrawFieldEdges 补丁 - 聚焦层级时，将格子高亮的坐标
    /// 从子地图本地坐标偏移到主地图对应位置。
    /// </summary>
    [HarmonyPatch(typeof(GenDraw), "DrawFieldEdges",
        typeof(List<IntVec3>), typeof(Color), typeof(float?),
        typeof(HashSet<IntVec3>), typeof(int))]
    public static class Patch_GenDraw_DrawFieldEdges
    {
        public static void Prefix(List<IntVec3> cells)
        {
            var baseMap = Find.CurrentMap;
            if (baseMap == null) return;

            var mgr = LevelManager.GetManager(baseMap);
            if (mgr == null || !mgr.IsFocusingLevel) return;

            var level = mgr.GetLevel(mgr.FocusedElevation);
            if (level == null) return;

            // 原地偏移所有格子坐标
            for (int i = 0; i < cells.Count; i++)
            {
                cells[i] = cells[i].ToBaseCoord(level);
            }
        }
    }

    /// <summary>
    /// SelectionDrawer.DrawSelectionBracketFor 补丁 - 子地图物体的选中白框
    /// 需要偏移到主地图对应位置。
    ///
    /// 原版用 thing.DrawPos（子地图本地坐标）计算框位置，
    /// 导致框画在主地图的错误位置（如子地图 0,0 → 主地图 0,0）。
    /// </summary>
    [HarmonyPatch(typeof(SelectionDrawer), "DrawSelectionBracketFor")]
    public static class Patch_SelectionDrawer_DrawSelectionBracketFor
    {
        private static readonly FieldInfo bracketLocsField =
            typeof(SelectionDrawer).GetField("bracketLocs", BindingFlags.Static | BindingFlags.NonPublic);
        private static readonly FieldInfo selectTimesField =
            typeof(SelectionDrawer).GetField("selectTimes", BindingFlags.Static | BindingFlags.NonPublic);
        private static readonly FieldInfo selBracketMatField =
            typeof(SelectionDrawer).GetField("SelectionBracketMat", BindingFlags.Static | BindingFlags.NonPublic);

        public static bool Prefix(object obj, Material overrideMat)
        {
            Thing thing = obj as Thing;
            if (thing == null) return true;

            LevelManager manager;
            LevelData level;
            if (!LevelManager.IsLevelMap(thing.Map, out manager, out level))
                return true;

            if (bracketLocsField == null || selectTimesField == null || selBracketMatField == null)
                return true;

            Vector3 offset = LevelCoordUtility.GetDrawOffset(level);
            Vector3[] bracketLocs = (Vector3[])bracketLocsField.GetValue(null);
            var selectTimes = (Dictionary<object, float>)selectTimesField.GetValue(null);
            Material mat = overrideMat ?? (Material)selBracketMatField.GetValue(null);

            // 计算偏移后的中心位置
            Vector3 drawPos;
            Vector3? held = thing.DrawPosHeld;
            if (held != null)
            {
                drawPos = held.Value + offset;
            }
            else
            {
                return true; // 无法确定位置，让原版处理
            }

            // 计算选中框位置
            SelectionDrawerUtility.CalculateSelectionBracketPositionsWorld<object>(
                bracketLocs, thing, drawPos,
                thing.RotatedSize.ToVector2(), selectTimes,
                Vector2.one, 1f, thing.def.deselectedSelectionBracketFactor);

            // 绘制 4 个角的选中框
            float scale = thing.MultipleItemsPerCellDrawn() ? 0.8f : 1f;
            float extraScale = 1f;
            CameraDriver cam = Find.CameraDriver;
            float zoomFade = Mathf.Clamp01(Mathf.InverseLerp(
                cam.config.sizeRange.max * 0.85f,
                cam.config.sizeRange.max, cam.ZoomRootSize));
            if (thing is Pawn)
            {
                if (thing.def.Size == IntVec2.One)
                    scale *= Mathf.Min(1f + zoomFade / 2f, 2f);
                else
                    extraScale = Mathf.Min(1f + zoomFade / 2f, 2f);
            }

            int angle = 0;
            for (int i = 0; i < 4; i++)
            {
                Quaternion rot = Quaternion.AngleAxis((float)angle, Vector3.up);
                Vector3 pos = (bracketLocs[i] - drawPos) * scale + drawPos;
                Graphics.DrawMesh(MeshPool.plane10,
                    Matrix4x4.TRS(pos, rot, new Vector3(scale, 1f, scale) * extraScale),
                    mat, 0);
                angle -= 90;
            }

            return false;
        }
    }
}
