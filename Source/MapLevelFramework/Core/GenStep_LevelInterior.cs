using RimWorld;
using Verse;

namespace MapLevelFramework
{
    /// <summary>
    /// 层级地图的 GenStep - 为子地图铺设默认地形。
    /// </summary>
    public class GenStep_LevelInterior : GenStep
    {
        public override int SeedPart => 7654321;

        public override void Generate(Map map, GenStepParams parms)
        {
            // 确定默认地形
            string terrainDefName = "Concrete";

            if (map.Parent is LevelMapParent lmp && lmp.levelDef != null)
            {
                terrainDefName = lmp.levelDef.defaultTerrain ?? "Concrete";
            }

            TerrainDef terrain = DefDatabase<TerrainDef>.GetNamedSilentFail(terrainDefName);
            if (terrain == null)
            {
                terrain = TerrainDefOf.Concrete;
            }

            TerrainGrid terrainGrid = map.terrainGrid;
            foreach (IntVec3 cell in map.AllCells)
            {
                if (cell.InBounds(map))
                {
                    terrainGrid.SetTerrain(cell, terrain);
                }
            }
        }
    }
}
