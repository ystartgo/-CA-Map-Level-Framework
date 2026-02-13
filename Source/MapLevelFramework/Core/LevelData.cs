using RimWorld.Planet;
using Verse;

namespace MapLevelFramework
{
    /// <summary>
    /// 单个层级的运行时数据。
    /// </summary>
    public class LevelData : IExposable
    {
        /// <summary>
        /// 层级高度序号。
        /// </summary>
        public int elevation;

        /// <summary>
        /// 该层级在主地图上覆盖的区域（主地图坐标）。
        /// </summary>
        public CellRect area;

        /// <summary>
        /// 当前层被聚焦时，是否将该层 Pawn 合并到主图的 AllPawnsSpawned 视图中。
        /// 这样原版 UI（鼠标提示、检查器、部分选择逻辑）能看到子层 Pawn。
        /// </summary>
        public bool includePawnsInBaseMap = true;

        /// <summary>
        /// 层级定义（可选）。
        /// </summary>
        public LevelDef levelDef;

        /// <summary>
        /// 宿主主地图。
        /// </summary>
        public Map hostMap;

        /// <summary>
        /// 该层级的 MapParent。
        /// </summary>
        public LevelMapParent mapParent;

        /// <summary>
        /// 该层级的子地图。
        /// </summary>
        public Map LevelMap
        {
            get
            {
                if (mapParent == null) return null;
                return mapParent.Map;
            }
            set
            {
                // LevelMap 由 MapGenerator 设置，通过 mapParent 间接持有
                // 这个 setter 仅用于初始化阶段
            }
        }

        /// <summary>
        /// 不规则区域的可用格子集合（子地图坐标）。
        /// 如果为 null，则整个子地图都可用。
        /// </summary>
        public System.Collections.Generic.HashSet<IntVec3> usableCells;

        /// <summary>
        /// 检查子地图坐标是否在可用区域内。
        /// </summary>
        public bool IsCellUsable(IntVec3 levelCell)
        {
            if (usableCells == null) return true; // 全部可用
            return usableCells.Contains(levelCell);
        }

        /// <summary>
        /// 检查主地图坐标是否在该层级的覆盖区域内。
        /// </summary>
        public bool ContainsBaseMapCell(IntVec3 baseCell)
        {
            return area.Contains(baseCell);
        }

        public void ExposeData()
        {
            Scribe_Values.Look(ref elevation, "elevation", 0);
            Scribe_Values.Look(ref area, "area");
            Scribe_Values.Look(ref includePawnsInBaseMap, "includePawnsInBaseMap", true);
            Scribe_Defs.Look(ref levelDef, "levelDef");
            Scribe_References.Look(ref mapParent, "mapParent");
            // hostMap 由 LevelManager 在加载后重新设置
        }
    }
}
