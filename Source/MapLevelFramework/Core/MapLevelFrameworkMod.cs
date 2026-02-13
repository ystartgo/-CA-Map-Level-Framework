using HarmonyLib;
using Verse;

namespace MapLevelFramework
{
    /// <summary>
    /// Map Level Framework - 地图层级框架
    /// 为 RimWorld 提供垂直维度扩展能力：多层建筑、地下、天空、水底等。
    /// 
    /// 核心思路（参照 VMF）：
    /// - 每个层级 = 一张真实的 PocketMap，拥有完整原版系统
    /// - 交互重定向：聚焦某层时，鼠标/建造/选择全部指向该层的 Map
    /// - 叠加渲染：子地图内容通过坐标变换画在主地图的指定区域上
    /// </summary>
    public class MapLevelFrameworkMod : Mod
    {
        public static MapLevelFrameworkMod Instance { get; private set; }
        public static Harmony HarmonyInstance { get; private set; }

        public MapLevelFrameworkMod(ModContentPack content) : base(content)
        {
            Instance = this;
            HarmonyInstance = new Harmony("CA.MapLevelFramework");
            HarmonyInstance.PatchAll();
            Log.Message("[MapLevelFramework] Initialized.");
        }
    }
}
