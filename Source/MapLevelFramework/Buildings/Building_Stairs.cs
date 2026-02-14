using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace MapLevelFramework
{
    /// <summary>
    /// 楼梯建筑 - 放置后扫描周围有屋顶的连通区域，创建/更新上层。
    /// 同时作为 pawn 上下楼的通道。
    ///
    /// targetElevation 表示这个楼梯连接到的目标层级：
    /// - 0 = 地面层（下楼）
    /// - 1 = 2F（上楼到二楼）
    /// - 2 = 3F（上楼到三楼）
    /// - ...以此类推
    /// </summary>
    public class Building_Stairs : Building
    {
        /// <summary>
        /// 连接的目标 elevation。
        /// 玩家在地面建造时默认 1（→2F），在 2F 建造时自动设为 2（→3F）。
        /// 自动生成的下楼楼梯 targetElevation = 当前层 - 1。
        /// </summary>
        public int targetElevation = 1;

        /// <summary>
        /// 是否是自动生成的楼梯（不触发层级创建）。
        /// </summary>
        private bool autoSpawned;

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);

            if (respawningAfterLoad) return;
            if (autoSpawned) return; // 自动生成的楼梯不触发层级创建

            // 先检查是否在子地图上建造（子地图也有 LevelManager MapComponent，必须先排除）
            if (LevelManager.IsLevelMap(map, out var parentMgr, out var levelData))
            {
                // 在子地图上建造 → 自动设置 targetElevation 为当前层 + 1
                targetElevation = levelData.elevation + 1;
                CreateOrUpdateLevel(parentMgr, map);
                return;
            }

            // 在基地图上建造 → 创建上层
            var mgr = LevelManager.GetManager(map);
            if (mgr != null)
            {
                CreateOrUpdateLevel(mgr, map);
            }
        }

        // ========== 右键菜单 ==========

        public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Pawn selPawn)
        {
            foreach (var opt in base.GetFloatMenuOptions(selPawn))
                yield return opt;

            // pawn 必须和楼梯在同一张地图
            if (selPawn.Map != this.Map) yield break;

            if (!StairTransferUtility.TryGetTransferTarget(this, out Map destMap, out IntVec3 destPos))
                yield break;

            // 判断上楼还是下楼
            int currentElevation = GetCurrentElevation();
            string label = targetElevation > currentElevation ? "上楼" : "下楼";

            yield return new FloatMenuOption(label, delegate
            {
                Job job = JobMaker.MakeJob(MLF_JobDefOf.MLF_UseStairs, this);
                selPawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
            });
        }

        /// <summary>
        /// 获取楼梯所在的当前层级 elevation。地面 = 0。
        /// </summary>
        public int GetCurrentElevation()
        {
            if (LevelManager.IsLevelMap(this.Map, out _, out var levelData))
                return levelData.elevation;
            return 0; // 地面
        }

        // ========== 层级创建 ==========

        /// <summary>
        /// 扫描有屋顶的连通区域，创建或更新层级。
        /// scanMap 是楼梯所在的地图（用于扫描屋顶），可能是基地图或子地图。
        /// </summary>
        public void CreateOrUpdateLevel(LevelManager mgr, Map scanMap)
        {
            HashSet<IntVec3> roofedCells = ScanRoofedArea(scanMap, Position);

            if (roofedCells.Count == 0)
            {
                Log.Warning("[MLF] Stairs placed but no roofed cells found nearby.");
                return;
            }

            // 计算包围矩形
            int minX = int.MaxValue, minZ = int.MaxValue;
            int maxX = int.MinValue, maxZ = int.MinValue;
            foreach (IntVec3 cell in roofedCells)
            {
                if (cell.x < minX) minX = cell.x;
                if (cell.z < minZ) minZ = cell.z;
                if (cell.x > maxX) maxX = cell.x;
                if (cell.z > maxZ) maxZ = cell.z;
            }
            CellRect bounds = new CellRect(minX, minZ, maxX - minX + 1, maxZ - minZ + 1);

            var existing = mgr.GetLevel(targetElevation);
            if (existing != null)
            {
                // 层级已存在：合并新区域到已有区域（不是替换）
                if (existing.usableCells == null)
                    existing.usableCells = new HashSet<IntVec3>();
                existing.usableCells.UnionWith(roofedCells);

                // 重新计算包围矩形（从所有 usableCells）
                existing.area = RecalcBounds(existing.usableCells);

                // 刷新子地图地形（新增的格子铺地板）
                RefreshLevelTerrain(existing, mgr.map);
                existing.RebuildActiveSections();

                // 在子地图同位置放置楼梯（如果还没有）
                SpawnStairsOnLevel(existing.LevelMap, Position);

                Log.Message($"[MLF] Updated level {targetElevation}: total {existing.usableCells.Count} cells, bounds={existing.area}");
            }
            else
            {
                // 创建新层级
                var level = mgr.RegisterLevel(targetElevation, bounds);
                if (level != null)
                {
                    level.usableCells = roofedCells;
                    level.RebuildActiveSections();

                    // 在子地图同位置放置楼梯
                    SpawnStairsOnLevel(level.LevelMap, Position);

                    Log.Message($"[MLF] Created level {targetElevation}: {roofedCells.Count} cells, bounds={bounds}");
                }
            }
        }

        /// <summary>
        /// 在子地图的指定位置放置楼梯（如果该位置还没有楼梯）。
        /// </summary>
        private void SpawnStairsOnLevel(Map levelMap, IntVec3 pos)
        {
            if (levelMap == null || !pos.InBounds(levelMap)) return;

            // 检查该位置是否已有楼梯
            var things = levelMap.thingGrid.ThingsListAtFast(pos);
            for (int i = 0; i < things.Count; i++)
            {
                if (things[i] is Building_Stairs) return;
            }

            var stairs = (Building_Stairs)ThingMaker.MakeThing(this.def, this.Stuff);
            stairs.autoSpawned = true;
            // 自动生成的楼梯指向回来的方向（当前楼梯所在层）
            stairs.targetElevation = GetCurrentElevation();
            GenSpawn.Spawn(stairs, pos, levelMap);
        }

        /// <summary>
        /// 从起点 FloodFill 扫描所有有屋顶的连通格子。
        /// </summary>
        private static HashSet<IntVec3> ScanRoofedArea(Map map, IntVec3 start)
        {
            var result = new HashSet<IntVec3>();
            if (!start.InBounds(map)) return result;

            var queue = new Queue<IntVec3>();
            var visited = new HashSet<IntVec3>();

            // 起点本身不要求有屋顶（楼梯口可以露天）
            queue.Enqueue(start);
            visited.Add(start);
            if (map.roofGrid.RoofAt(start) != null)
                result.Add(start);

            while (queue.Count > 0)
            {
                IntVec3 current = queue.Dequeue();
                for (int i = 0; i < 4; i++)
                {
                    IntVec3 neighbor = current + GenAdj.CardinalDirections[i];
                    if (!neighbor.InBounds(map)) continue;
                    if (visited.Contains(neighbor)) continue;
                    visited.Add(neighbor);

                    if (map.roofGrid.RoofAt(neighbor) != null)
                    {
                        result.Add(neighbor);
                        queue.Enqueue(neighbor);
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// 刷新已有层级的地形（屋顶变化后调用）。
        /// </summary>
        private static void RefreshLevelTerrain(LevelData level, Map hostMap)
        {
            Map levelMap = level.LevelMap;
            if (levelMap == null) return;

            TerrainDef floor = DefDatabase<TerrainDef>.GetNamedSilentFail("WoodPlankFloor")
                ?? TerrainDefOf.WoodPlankFloor;
            TerrainDef openAir = DefDatabase<TerrainDef>.GetNamedSilentFail("MLF_OpenAir");
            TerrainDef levelBase = DefDatabase<TerrainDef>.GetNamedSilentFail("MLF_LevelBase");
            if (openAir == null) return;

            // 获取 underGrid 用于设置底层地形
            var underGridField = typeof(TerrainGrid).GetField("underGrid",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            TerrainDef[] underGrid = underGridField?.GetValue(levelMap.terrainGrid) as TerrainDef[];

            foreach (IntVec3 cell in level.area)
            {
                if (!cell.InBounds(levelMap)) continue;

                if (level.usableCells != null && level.usableCells.Contains(cell))
                {
                    if (levelMap.terrainGrid.TerrainAt(cell) == openAir)
                    {
                        levelMap.terrainGrid.SetTerrain(cell, floor);
                    }
                    // 确保底层是 LevelBase（支持所有地板建造）
                    if (underGrid != null && levelBase != null)
                    {
                        int index = levelMap.cellIndices.CellToIndex(cell);
                        underGrid[index] = levelBase;
                    }
                }
                else
                {
                    levelMap.terrainGrid.SetTerrain(cell, openAir);
                }

                levelMap.mapDrawer.MapMeshDirty(cell, MapMeshFlagDefOf.Terrain);
            }
        }

        /// <summary>
        /// 从格子集合计算包围矩形。
        /// </summary>
        private static CellRect RecalcBounds(HashSet<IntVec3> cells)
        {
            int minX = int.MaxValue, minZ = int.MaxValue;
            int maxX = int.MinValue, maxZ = int.MinValue;
            foreach (IntVec3 cell in cells)
            {
                if (cell.x < minX) minX = cell.x;
                if (cell.z < minZ) minZ = cell.z;
                if (cell.x > maxX) maxX = cell.x;
                if (cell.z > maxZ) maxZ = cell.z;
            }
            return new CellRect(minX, minZ, maxX - minX + 1, maxZ - minZ + 1);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref targetElevation, "targetElevation", 1);
            Scribe_Values.Look(ref autoSpawned, "autoSpawned", false);
        }
    }
}
