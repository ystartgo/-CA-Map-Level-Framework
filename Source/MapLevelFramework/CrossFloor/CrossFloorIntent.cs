using Verse;
using System.Collections.Generic;

namespace MapLevelFramework.CrossFloor
{
    /// <summary>
    /// 跨层意图传递系统。
    /// 记住 pawn 为什么跨层，到达后直接执行目标 job，而不是让原版重新随机扫描。
    /// 只用于工作类（建造/搬运/清洁等），需求类（食物/床/娱乐）不需要——可替代。
    /// 不持久化，UseStairs 是短时 job，存档重载后 ThinkTree 自然重新评估。
    /// </summary>
    public static class CrossFloorIntent
    {
        public struct Intent
        {
            public int destMapId;
            public IntVec3 targetPos;
            public ThingDef targetDef;
            public int createdTick;
        }

        // pawnId → intent
        private static readonly Dictionary<int, Intent> intents = new Dictionary<int, Intent>();
        private const int ExpireTicks = 2500; // ~1 分钟

        public static void Set(Pawn pawn, int destMapId, IntVec3 targetPos, ThingDef targetDef)
        {
            int tick = Find.TickManager?.TicksGame ?? 0;
            intents[pawn.thingIDNumber] = new Intent
            {
                destMapId = destMapId,
                targetPos = targetPos,
                targetDef = targetDef,
                createdTick = tick
            };
        }

        public static bool TryGet(Pawn pawn, out Intent intent)
        {
            if (!intents.TryGetValue(pawn.thingIDNumber, out intent))
                return false;

            int tick = Find.TickManager?.TicksGame ?? 0;
            if (tick - intent.createdTick > ExpireTicks)
            {
                intents.Remove(pawn.thingIDNumber);
                return false;
            }
            return true;
        }

        public static void Clear(Pawn pawn)
        {
            intents.Remove(pawn.thingIDNumber);
        }

        /// <summary>
        /// 清除所有意图（地图卸载时调用）。
        /// </summary>
        public static void ClearAll()
        {
            intents.Clear();
        }
    }
}
