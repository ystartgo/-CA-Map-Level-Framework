using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace MapLevelFramework
{
    /// <summary>
    /// 搬运材料到楼梯 JobDriver。
    /// pawn 捡起材料 → 走到楼梯 → 材料通过楼梯传送到目标楼层。
    /// pawn 留在原地，材料出现在目标楼层的楼梯位置。
    /// </summary>
    public class JobDriver_HaulToStairs : JobDriver
    {
        private Thing Material => job.targetA.Thing;
        private Building_Stairs Stairs => (Building_Stairs)job.targetB.Thing;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(job.targetA, job, 1, -1, null, errorOnFailed);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDestroyedNullOrForbidden(TargetIndex.A);
            this.FailOnDespawnedOrNull(TargetIndex.B);

            // 走到材料
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.ClosestTouch);

            // 拾取
            yield return Toils_Haul.StartCarryThing(TargetIndex.A, false, true, false, true);

            // 走到楼梯
            yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.OnCell);

            // 材料通过楼梯传送到目标楼层
            Toil teleportItem = ToilMaker.MakeToil("MLF_TeleportItem");
            teleportItem.initAction = delegate
            {
                Thing carried = pawn.carryTracker.CarriedThing;
                if (carried == null) return;

                Building_Stairs stairs = Stairs;
                if (stairs == null) return;

                if (!StairTransferUtility.TryGetTransferTarget(
                        stairs, out Map destMap, out IntVec3 destPos))
                    return;

                // 从 pawn 手中取出
                pawn.carryTracker.innerContainer.Remove(carried);

                // 在目标楼层楼梯位置生成
                GenSpawn.Spawn(carried, destPos, destMap);
            };
            teleportItem.defaultCompleteMode = ToilCompleteMode.Instant;
            yield return teleportItem;
        }
    }
}
