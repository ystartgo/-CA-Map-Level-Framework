using System.Collections.Generic;
using System.Reflection;
using RimWorld;
using Verse;

namespace MapLevelFramework
{
    /// <summary>
    /// 地下层地图的 GenStep。
    /// 全图铺 MLF_OpenAir + 厚岩石顶，楼梯位置铺 MLF_DirtFloor。
    /// </summary>
    public class GenStep_UndergroundInterior : GenStep
    {
        private static readonly FieldInfo underGridField =
            typeof(TerrainGrid).GetField("underGrid", BindingFlags.Instance | BindingFlags.NonPublic);

        public override int SeedPart => 7654322;

        public override void Generate(Map map, GenStepParams parms)
        {
            var lmp = map.Parent as LevelMapParent;
            if (lmp == null) return;

            TerrainDef openAir = DefDatabase<TerrainDef>.GetNamedSilentFail("MLF_OpenAir");
            TerrainDef dirtFloor = DefDatabase<TerrainDef>.GetNamedSilentFail("MLF_DirtFloor");
            TerrainDef levelBase = DefDatabase<TerrainDef>.GetNamedSilentFail("MLF_LevelBase");

            if (openAir == null || dirtFloor == null) return;

            TerrainDef[] underGrid = underGridField?.GetValue(map.terrainGrid) as TerrainDef[];

            // 全图铺 OpenAir（虚空）+ 厚岩石顶（地下层始终在山体内）
            foreach (IntVec3 cell in map.AllCells)
            {
                map.terrainGrid.SetTerrain(cell, openAir);
                map.roofGrid.SetRoof(cell, RoofDefOf.RoofRockThick);
            }

            // 楼梯位置（由 LevelMapParent 传入的 area 中心）
            IntVec3 stairPos = lmp.area.CenterCell;

            // 楼梯那一格铺泥地
            if (stairPos.InBounds(map))
            {
                map.terrainGrid.SetTerrain(stairPos, dirtFloor);
                if (underGrid != null && levelBase != null)
                {
                    int idx = map.cellIndices.CellToIndex(stairPos);
                    underGrid[idx] = levelBase;
                }
            }
        }
    }
}
