using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace MapLevelFramework
{
    /// <summary>
    /// 跨层电力传输管理器。
    /// 楼梯作为电池，同栋楼的所有楼梯共享存储电量。
    /// 每 60 tick 同步同栋楼梯的 storedEnergy（取平均值），
    /// 实现电力在楼层间自然流动。
    ///
    /// 默认模式：储电上限 = 电网总功率
    /// 超凡模式：储电上限 99999999，效率 350234%
    /// </summary>
    public class PowerRelayManager : MapComponent
    {
        private const int SyncInterval = 60;
        private const float TranscendentMax = 99999999f;
        private const float TranscendentEfficiency = 3502.34f;
        private const float NormalEfficiency = 1145.14f;
        private const float MinCapacity = 600f;

        // 复用容器
        private readonly Dictionary<string, List<CompPowerBattery>> buildingGroups
            = new Dictionary<string, List<CompPowerBattery>>();

        public PowerRelayManager(Map map) : base(map) { }

        public override void MapComponentTick()
        {
            if (LevelManager.IsLevelMap(map, out _, out _)) return;
            if (Find.TickManager.TicksGame % SyncInterval != 0) return;

            var mgr = LevelManager.GetManager(map);
            if (mgr == null || mgr.LevelCount == 0) return;

            SyncBatteries(mgr);
        }

        private void SyncBatteries(LevelManager mgr)
        {
            // 清理分组
            foreach (var kv in buildingGroups)
                kv.Value.Clear();

            // 收集所有楼梯电池 + 计算电网总功率
            float totalGridPower = 0f;
            totalGridPower += CollectFloor(map);
            foreach (var level in mgr.AllLevels)
            {
                if (level.LevelMap == null) continue;
                totalGridPower += CollectFloor(level.LevelMap);
            }

            // 根据模式设置电池参数
            bool transcendent = MapLevelFrameworkMod.Settings.transcendentPowerMode;
            float effectiveMax = transcendent ? TranscendentMax : Mathf.Max(totalGridPower, MinCapacity);
            float effectiveEff = transcendent ? TranscendentEfficiency : NormalEfficiency;

            // 同步每组的存储电量
            foreach (var kv in buildingGroups)
            {
                var comps = kv.Value;
                if (comps.Count == 0) continue;

                // 更新 Props（共享，改一个全改）
                var props = comps[0].Props;
                props.storedEnergyMax = effectiveMax;
                props.efficiency = effectiveEff;

                if (comps.Count < 2) continue;

                // 计算平均存储电量
                float total = 0f;
                for (int i = 0; i < comps.Count; i++)
                    total += comps[i].StoredEnergy;

                float avg = total / comps.Count;
                float pct = effectiveMax > 0f ? Mathf.Clamp01(avg / effectiveMax) : 0f;

                for (int i = 0; i < comps.Count; i++)
                    comps[i].SetStoredEnergyPct(pct);
            }
        }

        /// <summary>
        /// 收集楼层上的楼梯电池（按栋号分组），并返回该层非楼梯设备的总发电功率。
        /// </summary>
        private float CollectFloor(Map floorMap)
        {
            float power = 0f;
            var things = floorMap.listerThings.AllThings;
            for (int i = 0; i < things.Count; i++)
            {
                if (things[i] is Building_Stairs stairs)
                {
                    var comp = stairs.CompPowerBattery;
                    if (comp == null) continue;
                    string key = stairs.buildingLabel ?? "";
                    if (!buildingGroups.TryGetValue(key, out var list))
                    {
                        list = new List<CompPowerBattery>();
                        buildingGroups[key] = list;
                    }
                    list.Add(comp);
                }
                else if (things[i] is ThingWithComps twc)
                {
                    var pt = twc.GetComp<CompPowerTrader>();
                    if (pt != null && pt.PowerOn && pt.PowerOutput > 0f)
                        power += pt.PowerOutput;
                }
            }
            return power;
        }
    }
}