using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace MapLevelFramework
{
    /// <summary>
    /// 上下楼 JobDriver - 走到楼梯后将 pawn 转移到目标地图。
    /// </summary>
    public class JobDriver_UseStairs : JobDriver
    {
        private Building_Stairs Stairs => (Building_Stairs)job.targetA.Thing;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            // 楼梯不需要预约，多个 pawn 可以同时使用
            return true;
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedOrNull(TargetIndex.A);

            // 走到楼梯
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.OnCell);

            // 转移到目标地图
            Toil transfer = ToilMaker.MakeToil("MLF_Transfer");
            transfer.initAction = delegate
            {
                Building_Stairs stairs = Stairs;
                if (stairs == null) return;

                if (StairTransferUtility.TryGetTransferTarget(stairs, out Map destMap, out IntVec3 destPos))
                {
                    StairTransferUtility.TransferPawn(pawn, destMap, destPos);
                }
            };
            transfer.defaultCompleteMode = ToilCompleteMode.Instant;
            yield return transfer;
        }
    }
}
