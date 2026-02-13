using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace MapLevelFramework
{
    /// <summary>
    /// 层级地图的 MapParent，继承 PocketMapParent。
    /// 每个层级对应一个 LevelMapParent 实例，持有对宿主的引用。
    /// 
    /// 类比 VMF 的 MapParent_Vehicle。
    /// </summary>
    public class LevelMapParent : PocketMapParent
    {
        /// <summary>
        /// 该层级所属的宿主（主地图上的 MapComponent）。
        /// </summary>
        public LevelManager hostManager;

        /// <summary>
        /// 该层级的定义。
        /// </summary>
        public LevelDef levelDef;

        /// <summary>
        /// 该层级的高度序号（冗余存储，方便快速访问）。
        /// </summary>
        public int elevation;



        public override Material Material
        {
            get
            {
                return BaseContent.ClearMat;
            }
        }

        public override string Label
        {
            get
            {
                string tag = levelDef?.label ?? $"Level {elevation}";
                return $"{tag} ({hostManager?.map?.Parent?.Label ?? "?"})";
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            // hostManager is restored via LevelData -> mapParent linkage after load
            Scribe_Defs.Look(ref levelDef, "levelDef");
            Scribe_Values.Look(ref elevation, "elevation", 0);
        }
    }
}
