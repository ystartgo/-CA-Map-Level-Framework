using System.Collections.Generic;
using System.Reflection;
using LudeonTK;
using UnityEngine;
using Verse;

namespace MapLevelFramework
{
    /// <summary>
    /// 调试工具 - 通过开发者模式菜单测试框架功能。
    /// </summary>
    public static class MLF_DebugActions
    {
        [DebugAction("Map Level Framework", "Create Test Level (1F)",
            actionType = DebugActionType.Action,
            allowedGameStates = AllowedGameStates.PlayingOnMap)]
        public static void CreateTestLevel1F()
        {
            CreateTestLevel(1, "1F Test");
        }

        [DebugAction("Map Level Framework", "Create Test Level (B1)",
            actionType = DebugActionType.Action,
            allowedGameStates = AllowedGameStates.PlayingOnMap)]
        public static void CreateTestLevelB1()
        {
            CreateTestLevel(-1, "B1 Test");
        }

        [DebugAction("Map Level Framework", "Focus 1F",
            actionType = DebugActionType.Action,
            allowedGameStates = AllowedGameStates.PlayingOnMap)]
        public static void Focus1F()
        {
            var mgr = LevelManager.GetManager(Find.CurrentMap);
            if (mgr == null) return;
            mgr.FocusLevel(1);
        }

        [DebugAction("Map Level Framework", "Focus Ground (0)",
            actionType = DebugActionType.Action,
            allowedGameStates = AllowedGameStates.PlayingOnMap)]
        public static void FocusGround()
        {
            var mgr = LevelManager.GetManager(Find.CurrentMap);
            if (mgr == null)
            {
                Log.Warning("[MLF Debug] No LevelManager on current map.");
                return;
            }
            mgr.FocusLevel(0);
        }

        [DebugAction("Map Level Framework", "List All Levels",
            actionType = DebugActionType.Action,
            allowedGameStates = AllowedGameStates.PlayingOnMap)]
        public static void ListAllLevels()
        {
            var mgr = LevelManager.GetManager(Find.CurrentMap);
            if (mgr == null)
            {
                Log.Message("[MLF Debug] No LevelManager on current map.");
                return;
            }

            Log.Message($"[MLF Debug] Focused elevation: {mgr.FocusedElevation}");
            foreach (var level in mgr.AllLevels)
            {
                Log.Message($"  Elevation {level.elevation}: area={level.area}, " +
                            $"map={level.LevelMap?.uniqueID ?? -1}, " +
                            $"tag={level.levelDef?.levelTag ?? "none"}");
            }
        }

        [DebugAction("Map Level Framework", "Remove All Levels",
            actionType = DebugActionType.Action,
            allowedGameStates = AllowedGameStates.PlayingOnMap)]
        public static void RemoveAllLevels()
        {
            var mgr = LevelManager.GetManager(Find.CurrentMap);
            if (mgr == null) return;

            var elevations = new List<int>(mgr.AllElevations);
            foreach (int e in elevations)
            {
                mgr.RemoveLevel(e);
            }
            Log.Message("[MLF Debug] All levels removed.");
        }

        [DebugAction("Map Level Framework", "Force Regen 2F",
            actionType = DebugActionType.Action,
            allowedGameStates = AllowedGameStates.PlayingOnMap)]
        public static void ForceRegen2F()
        {
            var mgr = LevelManager.GetManager(Find.CurrentMap);
            if (mgr == null) return;
            var level = mgr.GetLevel(mgr.FocusedElevation);
            if (level?.LevelMap == null)
            {
                Log.Warning("[MLF Debug] No focused level map.");
                return;
            }
            // 清除 initializedMaps 缓存，下一帧会触发 RegenerateEverythingNow
            Render.LevelRenderer.ClearInitializedMap(level.LevelMap.uniqueID);
            Log.Message("[MLF Debug] Cleared initialized cache. Will RegenerateEverythingNow next frame.");
        }

        [DebugAction("Map Level Framework", "Debug Terrain Mesh",
            actionType = DebugActionType.Action,
            allowedGameStates = AllowedGameStates.PlayingOnMap)]
        public static void DebugTerrainMesh()
        {
            var baseMap = Find.CurrentMap;
            var mgr = LevelManager.GetManager(baseMap);
            if (mgr == null || !mgr.IsFocusingLevel) return;
            var level = mgr.GetLevel(mgr.FocusedElevation);
            if (level?.LevelMap == null) return;

            Map levelMap = level.LevelMap;
            IntVec3 cell = UI.MouseCell();

            // 1. 两张地图的地形（topGrid + underGrid）
            TerrainDef baseTerrain = baseMap.terrainGrid.TerrainAt(cell);
            TerrainDef levelTerrain = cell.InBounds(levelMap)
                ? levelMap.terrainGrid.TerrainAt(cell) : null;

            var underGridField = typeof(TerrainGrid).GetField("underGrid",
                BindingFlags.Instance | BindingFlags.NonPublic);
            TerrainDef[] baseUnder = underGridField?.GetValue(baseMap.terrainGrid) as TerrainDef[];
            TerrainDef[] levelUnder = underGridField?.GetValue(levelMap.terrainGrid) as TerrainDef[];
            int idx = cell.InBounds(levelMap) ? levelMap.cellIndices.CellToIndex(cell) : -1;

            string baseUnderName = (baseUnder != null && cell.InBounds(baseMap))
                ? baseUnder[baseMap.cellIndices.CellToIndex(cell)]?.defName ?? "null" : "?";
            string levelUnderName = (levelUnder != null && idx >= 0)
                ? levelUnder[idx]?.defName ?? "null" : "?";

            Log.Message($"[MLF Debug] Cell {cell}:");
            Log.Message($"  Base  top={baseTerrain?.defName}, under={baseUnderName}");
            Log.Message($"  Level top={levelTerrain?.defName}, under={levelUnderName}");
            Log.Message($"  Level affordances: {(levelTerrain != null ? string.Join(",", levelTerrain.affordances) : "none")}");
            Log.Message($"  usable={level.IsCellUsable(cell)}, inArea={level.area.Contains(cell)}");

            // 2. Section 信息
            var sectionsField = typeof(MapDrawer).GetField("sections",
                BindingFlags.Instance | BindingFlags.NonPublic);
            var layersField = typeof(Section).GetField("layers",
                BindingFlags.Instance | BindingFlags.NonPublic);
            Section[,] sections = sectionsField?.GetValue(levelMap.mapDrawer) as Section[,];
            if (sections == null) { Log.Warning("[MLF Debug] sections is null"); return; }

            int sx = cell.x / 17;
            int sz = cell.z / 17;
            if (sx >= sections.GetLength(0) || sz >= sections.GetLength(1))
            { Log.Warning("[MLF Debug] section index out of range"); return; }

            Section section = sections[sx, sz];
            if (section == null) { Log.Warning("[MLF Debug] section is null"); return; }

            Log.Message($"  Section[{sx},{sz}] dirtyFlags={section.dirtyFlags}, active={level.IsSectionActive(sx, sz)}");

            List<SectionLayer> layers = layersField?.GetValue(section) as List<SectionLayer>;
            if (layers == null) return;

            foreach (SectionLayer layer in layers)
            {
                string name = layer.GetType().Name;
                int totalVerts = 0;
                int subCount = layer.subMeshes.Count;
                int finalizedCount = 0;
                foreach (var sm in layer.subMeshes)
                {
                    if (sm.finalized) finalizedCount++;
                    if (sm.mesh != null) totalVerts += sm.mesh.vertexCount;
                }
                Log.Message($"  Layer {name}: subMeshes={subCount}, finalized={finalizedCount}/{subCount}, totalVerts={totalVerts}");
            }
        }

        [DebugAction("Map Level Framework", "Fix Level Terrain",
            actionType = DebugActionType.Action,
            allowedGameStates = AllowedGameStates.PlayingOnMap)]
        public static void FixLevelTerrain()
        {
            var baseMap = Find.CurrentMap;
            var mgr = LevelManager.GetManager(baseMap);
            if (mgr == null)
            {
                Log.Warning("[MLF Debug] No LevelManager on current map.");
                return;
            }

            TerrainDef levelBase = DefDatabase<TerrainDef>.GetNamedSilentFail("MLF_LevelBase");
            if (levelBase == null)
            {
                Log.Error("[MLF Debug] MLF_LevelBase terrain not found!");
                return;
            }

            var underGridField = typeof(TerrainGrid).GetField("underGrid",
                BindingFlags.Instance | BindingFlags.NonPublic);

            int totalFixed = 0;
            foreach (var level in mgr.AllLevels)
            {
                Map levelMap = level.LevelMap;
                if (levelMap == null) continue;

                TerrainDef[] underGrid = underGridField?.GetValue(levelMap.terrainGrid) as TerrainDef[];
                if (underGrid == null) continue;

                TerrainDef openAir = DefDatabase<TerrainDef>.GetNamedSilentFail("MLF_OpenAir");
                int fixedCount = 0;

                foreach (IntVec3 cell in levelMap.AllCells)
                {
                    if (level.usableCells != null && !level.usableCells.Contains(cell)) continue;
                    if (!level.area.Contains(cell)) continue;

                    int idx = levelMap.cellIndices.CellToIndex(cell);
                    TerrainDef top = levelMap.terrainGrid.TerrainAt(cell);

                    // 如果 topGrid 是可行走的地板，确保 underGrid 是 LevelBase
                    if (top != openAir && top.passability != Traversability.Impassable)
                    {
                        if (underGrid[idx] != levelBase)
                        {
                            underGrid[idx] = levelBase;
                            fixedCount++;
                        }
                    }
                }
                totalFixed += fixedCount;
                Log.Message($"[MLF Debug] Level {level.elevation}: fixed {fixedCount} underGrid cells to MLF_LevelBase");
            }
            Log.Message($"[MLF Debug] Total fixed: {totalFixed} cells");
        }

        private static void CreateTestLevel(int elevation, string name)
        {
            var map = Find.CurrentMap;
            if (map == null) return;

            var mgr = LevelManager.GetManager(map);
            if (mgr == null)
            {
                Log.Warning("[MLF Debug] No LevelManager on current map. It should be auto-added.");
                return;
            }

            // 在地图中心创建一个 13x13 的测试层级
            int cx = map.Size.x / 2;
            int cz = map.Size.z / 2;
            int halfSize = 6;
            CellRect area = new CellRect(cx - halfSize, cz - halfSize, 13, 13);

            var level = mgr.RegisterLevel(elevation, area);
            if (level != null)
            {
                Log.Message($"[MLF Debug] Created test level '{name}' at elevation {elevation}");
                mgr.FocusLevel(elevation);
            }
        }
    }
}
