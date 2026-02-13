using System.Runtime.CompilerServices;
using UnityEngine;
using Verse;

namespace MapLevelFramework
{
    /// <summary>
    /// 坐标转换工具 - 主地图坐标 <-> 子地图坐标。
    /// 
    /// 主地图上的 area 区域对应子地图的 (0,0) 起点。
    /// 例如 area.minX=50, area.minZ=50，则主地图 (52, 53) = 子地图 (2, 3)。
    /// </summary>
    public static class LevelCoordUtility
    {
        // ========== IntVec3 转换 ==========

        /// <summary>
        /// 主地图坐标 -> 子地图坐标。
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IntVec3 ToLevelCoord(this IntVec3 baseCell, LevelData level)
        {
            return new IntVec3(
                baseCell.x - level.area.minX,
                0,
                baseCell.z - level.area.minZ
            );
        }

        /// <summary>
        /// 子地图坐标 -> 主地图坐标。
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IntVec3 ToBaseCoord(this IntVec3 levelCell, LevelData level)
        {
            return new IntVec3(
                levelCell.x + level.area.minX,
                0,
                levelCell.z + level.area.minZ
            );
        }

        // ========== Vector3 转换 ==========

        /// <summary>
        /// 主地图世界坐标 -> 子地图世界坐标。
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 ToLevelCoord(this Vector3 basePos, LevelData level)
        {
            return new Vector3(
                basePos.x - level.area.minX,
                basePos.y,
                basePos.z - level.area.minZ
            );
        }

        /// <summary>
        /// 子地图世界坐标 -> 主地图世界坐标。
        /// 用于叠加渲染时的坐标偏移。
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 ToBaseCoord(this Vector3 levelPos, LevelData level)
        {
            return new Vector3(
                levelPos.x + level.area.minX,
                levelPos.y,
                levelPos.z + level.area.minZ
            );
        }

        /// <summary>
        /// 获取从子地图原点到主地图 area 起点的偏移向量。
        /// 用于 Graphics.DrawMesh 的 offset 参数。
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 GetDrawOffset(LevelData level)
        {
            return new Vector3(level.area.minX, 0f, level.area.minZ);
        }

        // ========== 查询工具 ==========

        /// <summary>
        /// 检查主地图坐标是否落在某个层级的覆盖区域内。
        /// 如果是，返回对应的 LevelData。
        /// </summary>
        public static bool TryGetLevelAtBaseCell(IntVec3 baseCell, Map baseMap, out LevelData level)
        {
            level = null;
            var mgr = LevelManager.GetManager(baseMap);
            if (mgr == null || !mgr.IsFocusingLevel) return false;

            var focused = mgr.GetLevel(mgr.FocusedElevation);
            if (focused != null && focused.ContainsBaseMapCell(baseCell))
            {
                level = focused;
                return true;
            }
            return false;
        }

        /// <summary>
        /// 判断一个 Thing 是否在某个层级子地图上。
        /// </summary>
        public static bool IsOnLevelMap(this Thing thing, out LevelManager manager, out LevelData level)
        {
            manager = null;
            level = null;
            if (thing?.Map == null) return false;
            return LevelManager.IsLevelMap(thing.Map, out manager, out level);
        }
    }
}
