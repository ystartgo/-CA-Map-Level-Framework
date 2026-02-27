using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;
using MapLevelFramework.CrossFloor;

namespace MapLevelFramework
{
    /// <summary>
    /// 封装跨层材料投递 JobDriver。
    /// 全程手持材料：拿起 → 走楼梯 → 传送 → 交给原版 HaulToContainer 完成投递。
    ///
    /// TargetA = 材料（当前层）
    /// TargetB = 楼梯（当前层）
    /// targetC = 蓝图/框架位置编码 (x=destElevation, y=blueprintPos.x, z=blueprintPos.z)
    /// count   = 搬运数量
    /// </summary>
    public class JobDriver_DeliverResourcesCrossFloor : JobDriver
    {
        private Thing Material => job.targetA.Thing;
        private Building_Stairs Stairs => (Building_Stairs)job.targetB.Thing;

        private int DestElevation => job.targetC.IsValid ? job.targetC.Cell.x : 0;
        private IntVec3 BlueprintPos => job.targetC.IsValid
            ? new IntVec3(job.targetC.Cell.y, 0, job.targetC.Cell.z)
            : IntVec3.Invalid;

        private static bool DebugLog =>
            MapLevelFrameworkMod.Settings?.debugPathfindingAndJob ?? false;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(job.targetA, job, 1, -1, null, errorOnFailed);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDestroyedNullOrForbidden(TargetIndex.A);
            this.FailOnDespawnedOrNull(TargetIndex.B);

            // 1. 走到材料
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.ClosestTouch);

            // 2. 拿起材料
            yield return Toils_Haul.StartCarryThing(TargetIndex.A, false, true, false, true);

            // 3. 走到楼梯
            yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.OnCell);

            // 4. 传送到目标层 + 交给原版 HaulToContainer
            Toil transferAndDeliver = ToilMaker.MakeToil("MLF_TransferAndDeliver");
            transferAndDeliver.initAction = delegate
            {
                Building_Stairs stairs = Stairs;
                if (stairs == null) return;

                Thing carried = pawn.carryTracker.CarriedThing;
                if (carried == null) return;

                int destElev = DestElevation;
                IntVec3 bpPos = BlueprintPos;

                if (!StairTransferUtility.TryGetTransferTarget(
                        stairs, destElev, out Map destMap, out IntVec3 destPos))
                {
                    if (DebugLog)
                        Log.Message($"【MLF】跨层投递-{pawn.LabelShort}—传送失败: 无法到达 elev={destElev}");
                    return;
                }

                if (DebugLog)
                    Log.Message($"【MLF】跨层投递-{pawn.LabelShort}—传送: {pawn.Map.uniqueID}→{destMap.uniqueID}, 携带{carried.LabelShort}x{carried.stackCount}");

                // 传送 pawn（携带物品会自动保存/恢复）
                StairTransferUtility.TransferPawn(pawn, destMap, destPos);

                // 在目标层找蓝图/框架
                Thing blueprint = FindConstructibleAt(destMap, bpPos);
                if (blueprint == null)
                {
                    // 无蓝图 → Bill 原料投递：丢在工作台附近，然后封装 DoBill job
                    IntVec3 dropPos = bpPos.IsValid && bpPos.InBounds(destMap) ? bpPos : pawn.Position;
                    pawn.carryTracker.TryDropCarriedThing(dropPos, ThingPlaceMode.Near, out _);

                    // 找工作台，尝试直接开始 DoBill
                    Thing workbench = FindBillGiverAt(destMap, bpPos);
                    if (workbench != null)
                    {
                        Job billJob = TryCreateBillJob(pawn, workbench);
                        if (billJob != null)
                        {
                            if (DebugLog)
                                Log.Message($"【MLF】跨层投递-{pawn.LabelShort}—Bill封装: 丢下材料后开始 DoBill at {workbench.LabelShort}");
                            pawn.jobs.StartJob(billJob, JobCondition.None, null, false, true);
                            return;
                        }
                    }

                    if (DebugLog)
                        Log.Message($"【MLF】跨层投递-{pawn.LabelShort}—目标位置无蓝图/工作台 at {bpPos}，丢下材料");
                    return;
                }

                // 创建原版 HaulToContainer job
                // pawn 手里已有材料 → HaulToContainer 的 JumpIf 会跳过拾取步骤
                Thing carriedNow = pawn.carryTracker.CarriedThing;
                if (carriedNow == null) return;

                Job haulJob = JobMaker.MakeJob(JobDefOf.HaulToContainer);
                haulJob.targetA = carriedNow;
                haulJob.targetB = blueprint;
                haulJob.targetC = blueprint; // primaryDest
                haulJob.count = carriedNow.stackCount;
                haulJob.haulMode = HaulMode.ToContainer;

                if (DebugLog)
                    Log.Message($"【MLF】跨层投递-{pawn.LabelShort}—交接原版 HaulToContainer: {carriedNow.LabelShort}→{blueprint.LabelShort}");

                pawn.jobs.StartJob(haulJob, JobCondition.None, null, false, true);
            };
            transferAndDeliver.defaultCompleteMode = ToilCompleteMode.Instant;
            yield return transferAndDeliver;
        }

        /// <summary>
        /// 在目标地图指定位置找蓝图或框架。
        /// </summary>
        private static Thing FindConstructibleAt(Map map, IntVec3 pos)
        {
            if (!pos.InBounds(map)) return null;

            var things = map.thingGrid.ThingsListAtFast(pos);
            for (int i = 0; i < things.Count; i++)
            {
                Thing t = things[i];
                if ((t is Blueprint || t is Frame) && t.Spawned)
                    return t;
            }

            // 蓝图可能变成了框架，位置可能偏移一格
            foreach (IntVec3 cell in GenAdj.CellsAdjacent8Way(pos, Rot4.North, IntVec2.One))
            {
                if (!cell.InBounds(map)) continue;
                var nearby = map.thingGrid.ThingsListAtFast(cell);
                for (int i = 0; i < nearby.Count; i++)
                {
                    Thing t = nearby[i];
                    if ((t is Blueprint || t is Frame) && t.Spawned)
                        return t;
                }
            }

            return null;
        }

        /// <summary>
        /// 在指定位置找 IBillGiver（工作台）。
        /// </summary>
        private static Thing FindBillGiverAt(Map map, IntVec3 pos)
        {
            if (!pos.InBounds(map)) return null;
            var things = map.thingGrid.ThingsListAtFast(pos);
            for (int i = 0; i < things.Count; i++)
            {
                if (things[i] is IBillGiver && things[i].Spawned)
                    return things[i];
            }
            return null;
        }

        /// <summary>
        /// 通过原版 BillUtility + WorkGiver_DoBill 创建 DoBill job。
        /// 材料刚丢在工作台附近，TryFindBestBillIngredients 会搜索到它。
        /// </summary>
        private static Job TryCreateBillJob(Pawn pawn, Thing workbench)
        {
            try
            {
                IBillGiver bg = workbench as IBillGiver;
                if (bg == null) return null;

                WorkGiverDef wgDef = BillUtility.GetWorkgiver(bg);
                if (wgDef == null) return null;

                WorkGiver_DoBill wg = wgDef.Worker as WorkGiver_DoBill;
                if (wg == null) return null;

                return wg.JobOnThing(pawn, workbench);
            }
            catch (System.Exception ex)
            {
                Log.Warning($"【MLF】TryCreateBillJob failed: {ex.Message}");
                return null;
            }
        }
    }
}
