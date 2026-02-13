using HarmonyLib;
using Verse;

namespace MapLevelFramework.Patches
{
    /// <summary>
    /// GenSpawn.Spawn 补丁 - 聚焦层级时，将 Projectile 和越界 Mote 重定向到主地图。
    /// 
    /// 参照 VMF 的 Patch_GenSpawn_Spawn。
    /// </summary>
    [HarmonyPatch(typeof(GenSpawn), "Spawn", new[]
    {
        typeof(Thing), typeof(IntVec3), typeof(Map), typeof(Rot4),
        typeof(WipeMode), typeof(bool), typeof(bool)
    })]
    public static class Patch_GenSpawn_Spawn
    {
        public static void Prefix(Thing newThing, ref Map map, IntVec3 loc)
        {
            if (map == null) return;

            // 检查是否在层级子地图上
            if (!LevelManager.IsLevelMap(map, out var manager, out var level))
                return;

            // Projectile 应该在主地图上生成（跨层射击等）
            if (newThing is Projectile)
            {
                map = manager.map;
                return;
            }

            // 越界的 Mote 重定向到主地图
            if (newThing is Mote && !loc.InBounds(map))
            {
                map = manager.map;
            }
        }
    }
}
