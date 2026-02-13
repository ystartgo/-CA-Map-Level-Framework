using System;
using System.Collections.Generic;
using System.Reflection;
using RimWorld;
using UnityEngine;
using Verse;

namespace MapLevelFramework.Render
{
    /// <summary>
    /// 层级叠加渲染器 - 将子地图的内容渲染到主地图的指定区域上。
    ///
    /// 核心原理：
    /// 1. 获取子地图的 Section mesh 数据
    /// 2. 用 Graphics.DrawMesh + offset 画到主地图对应位置（Y 偏移防止 z-fighting）
    /// 3. 动态 Thing 也通过 offset 重新定位绘制
    /// 4. 主地图在 area 内的动态物体由 Patch_DynamicDrawManager 跳过
    /// </summary>
    public static class LevelRenderer
    {
        // 反射缓存
        private static FieldInfo sectionsField;
        private static FieldInfo layersField;
        private static FieldInfo dirtyFlagsField;

        // 层级内容的 Y 偏移，确保渲染在主地图地形之上
        private const float YOffset = 0.5f;

        // 跟踪哪些子地图已完成首次全量重建
        private static HashSet<int> initializedMaps = new HashSet<int>();

        static LevelRenderer()
        {
            sectionsField = typeof(MapDrawer).GetField("sections",
                BindingFlags.Instance | BindingFlags.NonPublic);
            layersField = typeof(Section).GetField("layers",
                BindingFlags.Instance | BindingFlags.NonPublic);
            dirtyFlagsField = typeof(Section).GetField("dirtyFlags",
                BindingFlags.Instance | BindingFlags.NonPublic);
        }

        /// <summary>
        /// 更新子地图的 section mesh。
        /// 原版 MapMeshDrawerUpdate_First 用摄像机 ViewRect 做裁剪，
        /// 但子地图的本地坐标和摄像机视口不重叠，导致 dirty section 永远不会重建。
        /// 这里手动用覆盖整个子地图的 rect 来触发更新。
        /// </summary>
        public static void UpdateLevelMapSections(Map levelMap)
        {
            if (levelMap?.mapDrawer == null) return;

            Section[,] sections = sectionsField?.GetValue(levelMap.mapDrawer) as Section[,];
            if (sections == null) return;

            int mapId = levelMap.uniqueID;

            // 首次聚焦时全量重建
            if (!initializedMaps.Contains(mapId))
            {
                levelMap.mapDrawer.RegenerateEverythingNow();
                initializedMaps.Add(mapId);
                return;
            }

            // 后续帧：用覆盖整个子地图的 rect 更新 dirty section
            CellRect fullRect = new CellRect(0, 0, levelMap.Size.x, levelMap.Size.z);
            for (int x = 0; x < sections.GetLength(0); x++)
            {
                for (int z = 0; z < sections.GetLength(1); z++)
                {
                    Section section = sections[x, z];
                    if (section == null) continue;
                    section.TryUpdate(fullRect);
                }
            }
        }

        /// <summary>
        /// 渲染子地图的静态 mesh（地形、建筑等 SectionLayer）。
        /// </summary>
        public static void DrawLevelMapMesh(Map levelMap, Vector3 drawOffset)
        {
            if (levelMap?.mapDrawer == null) return;
            if (sectionsField == null || layersField == null) return;

            drawOffset.y += YOffset;

            Section[,] sections = sectionsField.GetValue(levelMap.mapDrawer) as Section[,];
            if (sections == null) return;

            for (int x = 0; x < sections.GetLength(0); x++)
            {
                for (int z = 0; z < sections.GetLength(1); z++)
                {
                    Section section = sections[x, z];
                    if (section == null) continue;

                    List<SectionLayer> layers = layersField.GetValue(section) as List<SectionLayer>;
                    if (layers == null) continue;

                    foreach (SectionLayer layer in layers)
                    {
                        if (layer == null) continue;

                        string layerName = layer.GetType().Name;
                        if (ShouldSkipLayer(layerName)) continue;

                        foreach (LayerSubMesh subMesh in layer.subMeshes)
                        {
                            if (!subMesh.finalized || subMesh.disabled) continue;
                            if (subMesh.mesh == null || subMesh.material == null ||
                                subMesh.mesh.vertexCount <= 0) continue;

                            Graphics.DrawMesh(
                                subMesh.mesh,
                                Matrix4x4.TRS(drawOffset, Quaternion.identity, Vector3.one),
                                subMesh.material,
                                0
                            );
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 渲染子地图的动态 Thing（Pawn、物品等）。
        /// 参照原版 DynamicDrawManager.DrawDynamicThings 的三阶段流程。
        /// </summary>
        public static void DrawLevelDynamicThings(Map levelMap, LevelData level)
        {
            if (levelMap?.dynamicDrawManager == null) return;

            Vector3 offset = LevelCoordUtility.GetDrawOffset(level);
            offset.y += YOffset;

            IReadOnlyList<Thing> drawThings = levelMap.dynamicDrawManager.DrawThings;
            int count = drawThings.Count;
            if (count == 0) return;

            // Phase 1: EnsureInitialized
            for (int i = 0; i < count; i++)
            {
                try
                {
                    drawThings[i].DynamicDrawPhase(DrawPhase.EnsureInitialized);
                }
                catch (Exception) { }
            }

            // Phase 2: ParallelPreDraw (单线程执行，避免跨地图线程问题)
            for (int i = 0; i < count; i++)
            {
                try
                {
                    drawThings[i].DynamicDrawPhase(DrawPhase.ParallelPreDraw);
                }
                catch (Exception) { }
            }

            // Phase 3: Draw + Shadows
            for (int i = 0; i < count; i++)
            {
                Thing thing = drawThings[i];
                try
                {
                    Vector3 drawLoc = thing.DrawPos + offset;
                    thing.DynamicDrawPhaseAt(DrawPhase.Draw, drawLoc, false);

                    // Pawn 阴影
                    Pawn pawn = thing as Pawn;
                    if (pawn != null)
                    {
                        pawn.DrawShadowAt(drawLoc);
                    }
                }
                catch (Exception) { }
            }
        }

        /// <summary>
        /// 渲染子地图的覆盖层：designations、overlays、flecks 等。
        /// </summary>
        public static void DrawLevelOverlays(Map levelMap, LevelData level)
        {
            if (levelMap == null) return;

            try
            {
                levelMap.designationManager?.DrawDesignations();
            }
            catch (Exception) { }

            try
            {
                levelMap.overlayDrawer?.DrawAllOverlays();
            }
            catch (Exception) { }

            try
            {
                levelMap.flecks?.FleckManagerDraw();
            }
            catch (Exception) { }

            try
            {
                levelMap.temporaryThingDrawer?.Draw();
            }
            catch (Exception) { }
        }

        /// <summary>
        /// 判断是否应该跳过某个 SectionLayer。
        /// </summary>
        private static bool ShouldSkipLayer(string layerName)
        {
            switch (layerName)
            {
                case "SectionLayer_FogOfWar":
                case "SectionLayer_Darkness":
                case "SectionLayer_LightingOverlay":
                case "SectionLayer_Snow":
                    return true;
                default:
                    return false;
            }
        }
    }
}
