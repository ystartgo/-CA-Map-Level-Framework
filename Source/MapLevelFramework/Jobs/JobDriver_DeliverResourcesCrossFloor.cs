using System;
using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;
using MapLevelFramework.CrossFloor;

namespace MapLevelFramework
{
    /// <summary>
    /// 封装跨层材料投递 JobDriver。
    ///
    /// 单材料模式（建造/燃料/单原料 Bill）：
    ///   TargetA = 材料, TargetB = 楼梯, targetC = 位置编码, count = 数量
    ///   流程：手持 → 走楼梯 → 传送 → HaulToContainer 或丢下
    ///
    /// 多材料模式（多原料 Bill，参考 Pick Up And Haul）：
    ///   targetQueueA + countQueue = 多个材料, TargetB = 楼梯, targetC = 位置编码
    ///   流程：依次捡进背包 → 走楼梯 → 传送 → 全部丢在工作台旁 → DoBill
    /// </summary>
    public class JobDriver_DeliverResourcesCrossFloor : JobDriver
    {
        private Thing Material => job.targetA.Thing;
        private Building_Stairs Stairs => (Building_Stairs)job.targetB.Thing;

        private int DestElevation => job.targetC.IsValid ? job.targetC.Cell.x : 0;
        private IntVec3 BlueprintPos => job.targetC.IsValid
            ? new IntVec3(job.targetC.Cell.y, 0, job.targetC.Cell.z)
            : IntVec3.Invalid;

        private bool IsMultiMaterial => job.targetQueueA != null && job.targetQueueA.Count > 0;

        // 多材料模式：追踪塞进背包的物品，传送后只丢这些
        private List<Thing> pickedUpThings = new List<Thing>();
        // 传送中标记：防止 DeSpawn→StopAll→FinishAction 误丢背包物品
        private bool isTransferring;

        private static bool DebugLog =>
            MapLevelFrameworkMod.Settings?.debugPathfindingAndJob ?? false;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            if (IsMultiMaterial)
            {
                pawn.ReserveAsManyAsPossible(job.targetQueueA, job);
                return true;
            }
            return pawn.Reserve(job.targetA, job, 1, -1, null, errorOnFailed);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedOrNull(TargetIndex.B);

            if (IsMultiMaterial)
            {
                // ===== 多材料 Bill 投递（inventory-based）=====
                foreach (Toil t in MakeMultiMaterialToils())
                    yield return t;
                yield break;
            }

            // ===== 单材料投递（carry-based，原有逻辑）=====
            this.FailOnDestroyedNullOrForbidden(TargetIndex.A);

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

                StairTransferUtility.TransferPawn(pawn, destMap, destPos);

                Thing blueprint = FindConstructibleAt(destMap, bpPos);
                if (blueprint == null)
                {
                    IntVec3 dropPos = bpPos.IsValid && bpPos.InBounds(destMap) ? bpPos : pawn.Position;
                    pawn.carryTracker.TryDropCarriedThing(dropPos, ThingPlaceMode.Near, out _);

                    Thing workbench = FindBillGiverNear(destMap, bpPos);
                    if (DebugLog)
                    {
                        Log.Message($"【MLF】跨层投递-{pawn.LabelShort}—单材料FindBillGiverNear({bpPos}): {(workbench != null ? $"{workbench.def.defName}@{workbench.Position}" : "null")}");
                        // 列出 bpPos 附近所有 thing
                        if (bpPos.InBounds(destMap))
                        {
                            var nearby = destMap.thingGrid.ThingsListAtFast(bpPos);
                            Log.Message($"【MLF】跨层投递-{pawn.LabelShort}—bpPos({bpPos})上的things: {nearby.Count}个");
                            for (int ti = 0; ti < nearby.Count; ti++)
                                Log.Message($"  [{ti}] {nearby[ti].def.defName} ({nearby[ti].GetType().Name})");
                        }
                    }
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

                Thing carriedNow = pawn.carryTracker.CarriedThing;
                if (carriedNow == null) return;

                Job haulJob = JobMaker.MakeJob(JobDefOf.HaulToContainer);
                haulJob.targetA = carriedNow;
                haulJob.targetB = blueprint;
                haulJob.targetC = blueprint;
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
        /// 多材料 Bill 投递 toils：依次捡进背包 → 走楼梯 → 传送 → 全丢下 → DoBill。
        /// </summary>
        private IEnumerable<Toil> MakeMultiMaterialToils()
        {
            // 全局清理：job 被打断时丢出背包里的材料
            this.AddFinishAction(delegate { DropAllPickedUp(); });
            // 1. 从队列取下一个目标
            Toil extractTarget = Toils_JobTransforms.ExtractNextTargetFromQueue(TargetIndex.A);
            yield return extractTarget;

            // 2. 走到材料
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.ClosestTouch)
                .FailOnDestroyedNullOrForbidden(TargetIndex.A);

            // 3. 捡进背包（bill 原料必须送达，不做严格负重限制）
            Toil pickUp = ToilMaker.MakeToil("MLF_PickUpToInventory");
            pickUp.initAction = delegate
            {
                Thing thing = job.GetTarget(TargetIndex.A).Thing;
                if (thing == null || !thing.Spawned) return;

                int countToPickUp = Math.Min(job.count, thing.stackCount);
                if (countToPickUp <= 0) return;

                Thing splitThing = thing.SplitOff(countToPickUp);
                pawn.inventory.GetDirectlyHeldThings().TryAdd(splitThing, false);
                pickedUpThings.Add(splitThing);

                if (DebugLog)
                    Log.Message($"【MLF】跨层投递-{pawn.LabelShort}—拾取到背包: {splitThing.LabelShort}x{splitThing.stackCount}");
            };
            yield return pickUp;

            // 4. 队列还有 → 回到步骤 1
            yield return Toils_Jump.JumpIf(extractTarget, () => !job.targetQueueA.NullOrEmpty());

            // 5. 走到楼梯
            yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.OnCell);

            // 6. 传送 + 全部丢下（Forbid 防抢）+ DoBill
            Toil transferAndDropAll = ToilMaker.MakeToil("MLF_TransferAndDropAll");
            transferAndDropAll.initAction = delegate
            {
                TransferAndDropAllAction();
            };
            transferAndDropAll.defaultCompleteMode = ToilCompleteMode.Instant;
            yield return transferAndDropAll;
        }

        /// <summary>
        /// job 中断时把背包里捡的东西丢出来，防止材料卡在背包里。
        /// </summary>
        public override void Notify_PatherFailed()
        {
            DropAllPickedUp();
            base.Notify_PatherFailed();
        }

        private void DropAllPickedUp()
        {
            if (isTransferring) return; // 传送中，不丢
            if (pickedUpThings == null || pickedUpThings.Count == 0) return;
            for (int i = pickedUpThings.Count - 1; i >= 0; i--)
            {
                Thing t = pickedUpThings[i];
                if (t != null && pawn.inventory.innerContainer.Contains(t))
                {
                    pawn.inventory.innerContainer.TryDrop(t, pawn.Position,
                        pawn.Map, ThingPlaceMode.Near, out _);
                }
            }
            pickedUpThings.Clear();
        }

        /// <summary>
        /// 多材料传送+丢下+DoBill 的核心逻辑，带详细日志。
        /// </summary>
        private void TransferAndDropAllAction()
        {
            Building_Stairs stairs = Stairs;
            if (stairs == null) return;

            int destElev = DestElevation;
            IntVec3 bpPos = BlueprintPos;

            if (!StairTransferUtility.TryGetTransferTarget(
                    stairs, destElev, out Map destMap, out IntVec3 destPos))
            {
                if (DebugLog)
                    Log.Message($"【MLF】跨层投递-{pawn.LabelShort}—多材料传送失败: elev={destElev}");
                DropAllPickedUp();
                return;
            }

            if (DebugLog)
            {
                string items = "";
                for (int i = 0; i < pickedUpThings.Count; i++)
                {
                    if (i > 0) items += "+";
                    Thing pt = pickedUpThings[i];
                    items += pt != null ? $"{pt.LabelShort}x{pt.stackCount}" : "null";
                }
                Log.Message($"【MLF】跨层投递-{pawn.LabelShort}—多材料传送: →{destMap.uniqueID}, 背包[{items}]");
            }

            isTransferring = true;
            StairTransferUtility.TransferPawn(pawn, destMap, destPos);
            isTransferring = false;

            // 找工作台
            Thing workbench = FindBillGiverNear(destMap, bpPos);
            if (DebugLog)
                Log.Message($"【MLF】跨层投递-{pawn.LabelShort}—FindBillGiverNear({bpPos}): {(workbench != null ? $"{workbench.LabelShort}@{workbench.Position}" : "null")}");

            // 丢在工作台旁 + Forbid 防抢
            List<Thing> droppedThings = new List<Thing>();
            IntVec3 dropCenter = bpPos.IsValid && bpPos.InBounds(destMap) ? bpPos : pawn.Position;
            for (int i = pickedUpThings.Count - 1; i >= 0; i--)
            {
                Thing t = pickedUpThings[i];
                if (t == null) continue;
                bool inInv = pawn.inventory.innerContainer.Contains(t);
                if (DebugLog)
                    Log.Message($"【MLF】跨层投递-{pawn.LabelShort}—丢下[{i}]: {t.LabelShort}x{t.stackCount}, inInventory={inInv}");
                if (!inInv) continue;

                Thing dropped;
                pawn.inventory.innerContainer.TryDrop(t, dropCenter,
                    pawn.Map, ThingPlaceMode.Near, out dropped);
                if (dropped != null)
                {
                    droppedThings.Add(dropped);
                    if (DebugLog)
                        Log.Message($"【MLF】跨层投递-{pawn.LabelShort}—丢下OK: {dropped.LabelShort}x{dropped.stackCount}@{dropped.Position}, Forbid it");
                    // Forbid 防止其他 pawn 搬走
                    dropped.SetForbidden(true, false);
                }
                else
                {
                    if (DebugLog)
                        Log.Message($"【MLF】跨层投递-{pawn.LabelShort}—丢下失败: TryDrop returned null");
                }
            }
            pickedUpThings.Clear();

            if (workbench == null)
            {
                if (DebugLog)
                    Log.Message($"【MLF】跨层投递-{pawn.LabelShort}—目标位置无工作台 at {bpPos}，材料已Forbid");
                return;
            }

            // 列出工作台的 bills 信息
            if (DebugLog)
            {
                IBillGiver bg = workbench as IBillGiver;
                if (bg?.BillStack != null)
                {
                    for (int bi = 0; bi < bg.BillStack.Count; bi++)
                    {
                        Bill b = bg.BillStack[bi];
                        string ingStr = "";
                        foreach (var ing in b.recipe.ingredients)
                            ingStr += $"[{ing.filter.Summary}x{ing.GetBaseCount()}] ";
                        Log.Message($"【MLF】跨层投递-{pawn.LabelShort}—Bill[{bi}]: {b.recipe.defName}, ShouldDoNow={b.ShouldDoNow()}, PawnAllowed={b.PawnAllowedToStartAnew(pawn)}, ingredients={ingStr}");
                    }
                }
            }

            // 直接构建 DoBill job
            Job billJob = TryCreateDirectBillJob(pawn, workbench, droppedThings);
            if (billJob != null)
            {
                // Unforbid 材料（DoBill 需要 pawn 能拿到）
                for (int i = 0; i < droppedThings.Count; i++)
                    droppedThings[i]?.SetForbidden(false, false);
                if (DebugLog)
                    Log.Message($"【MLF】跨层投递-{pawn.LabelShort}—直接DoBill成功: {workbench.LabelShort}, Unforbid {droppedThings.Count}个材料");
                pawn.jobs.StartJob(billJob, JobCondition.None, null, false, true);
                return;
            }

            if (DebugLog)
                Log.Message($"【MLF】跨层投递-{pawn.LabelShort}—直接DoBill失败，尝试原版搜索...");

            // fallback: 原版搜索
            // 先 unforbid 让原版能找到
            for (int i = 0; i < droppedThings.Count; i++)
                droppedThings[i]?.SetForbidden(false, false);

            Job fallbackJob = TryCreateBillJob(pawn, workbench);
            if (fallbackJob != null)
            {
                if (DebugLog)
                    Log.Message($"【MLF】跨层投递-{pawn.LabelShort}—原版DoBill成功: {workbench.LabelShort}");
                pawn.jobs.StartJob(fallbackJob, JobCondition.None, null, false, true);
                return;
            }

            // 都失败了 → 重新 Forbid 材料防抢，等下趟补齐
            for (int i = 0; i < droppedThings.Count; i++)
                droppedThings[i]?.SetForbidden(true, false);
            if (DebugLog)
                Log.Message($"【MLF】跨层投递-{pawn.LabelShort}—DoBill全部失败，材料已Forbid等待下趟");
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
        /// 在指定位置及附近找 IBillGiver（工作台可能是多格建筑）。
        /// </summary>
        private static Thing FindBillGiverNear(Map map, IntVec3 pos)
        {
            if (!pos.InBounds(map)) return null;

            // 精确位置
            var things = map.thingGrid.ThingsListAtFast(pos);
            for (int i = 0; i < things.Count; i++)
            {
                if (things[i] is IBillGiver && things[i].Spawned)
                    return things[i];
            }

            // 多格建筑：搜索相邻格子
            foreach (IntVec3 cell in GenAdj.CellsAdjacent8Way(pos, Rot4.North, IntVec2.One))
            {
                if (!cell.InBounds(map)) continue;
                var nearby = map.thingGrid.ThingsListAtFast(cell);
                for (int i = 0; i < nearby.Count; i++)
                {
                    if (nearby[i] is IBillGiver && nearby[i].Spawned)
                        return nearby[i];
                }
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
        /// <summary>
        /// 直接构建 DoBill job，跳过 TryFindBestBillIngredients。
        /// 把刚丢下的材料直接塞进 job.targetQueueB，模仿原版 TryStartNewDoBillJob。
        /// </summary>
        private static Job TryCreateDirectBillJob(Pawn pawn, Thing workbench, List<Thing> droppedThings)
        {
            try
            {
                IBillGiver bg = workbench as IBillGiver;
                if (bg == null) return null;

                BillStack bills = bg.BillStack;
                if (bills == null || bills.Count == 0) return null;

                for (int bi = 0; bi < bills.Count; bi++)
                {
                    Bill bill = bills[bi];
                    if (!bill.ShouldDoNow()) continue;
                    if (!bill.PawnAllowedToStartAnew(pawn)) continue;

                    // 检查丢下的材料是否满足这个 bill 的所有 ingredient
                    List<ThingCount> chosen = TryMatchIngredients(bill, droppedThings, workbench);
                    if (chosen == null) continue;

                    if (DebugLog)
                    {
                        string chosenList = "";
                        for (int i = 0; i < chosen.Count; i++)
                        {
                            if (i > 0) chosenList += ", ";
                            chosenList += $"{chosen[i].Thing.def.defName}x{chosen[i].Count}";
                        }
                        Log.Message($"【MLF】跨层投递-{pawn.LabelShort}—直接DoBill成功: {workbench.LabelShort}, Unforbid {chosen.Count}个材料: [{chosenList}]");
                    }

                    // 构建 DoBill job（模仿 WorkGiver_DoBill.TryStartNewDoBillJob）
                    Job job = JobMaker.MakeJob(JobDefOf.DoBill, workbench);
                    job.targetQueueB = new List<LocalTargetInfo>(chosen.Count);
                    job.countQueue = new List<int>(chosen.Count);
                    for (int i = 0; i < chosen.Count; i++)
                    {
                        job.targetQueueB.Add(chosen[i].Thing);
                        job.countQueue.Add(chosen[i].Count);
                    }
                    job.haulMode = HaulMode.ToCellNonStorage;
                    job.bill = bill;

                    // Unforbid 选中的材料
                    for (int i = 0; i < chosen.Count; i++)
                    {
                        chosen[i].Thing.SetForbidden(false, false);
                    }

                    return job;
                }
            }
            catch (Exception ex)
            {
                Log.Warning($"【MLF】TryCreateDirectBillJob failed: {ex.Message}");
            }
            return null;
        }

        /// <summary>
        /// 尝试用 droppedThings 匹配 bill 的所有 ingredient。
        /// 返回 null 表示材料不齐。
        /// </summary>
        private static List<ThingCount> TryMatchIngredients(Bill bill, List<Thing> droppedThings, Thing workbench)
        {
            bool dbg = MapLevelFrameworkMod.Settings?.debugPathfindingAndJob ?? false;
            List<ThingCount> chosen = new List<ThingCount>();

            float[] remaining = new float[droppedThings.Count];
            for (int i = 0; i < droppedThings.Count; i++)
                remaining[i] = droppedThings[i]?.stackCount ?? 0;

            // 按 filter 宽度排序：窄 filter（AllowedThingDefs 少）优先匹配
            // 防止宽 filter（如"金属"）先消耗掉窄 filter（如"钢铁"）需要的材料
            List<IngredientCount> sortedIngs = new List<IngredientCount>(bill.recipe.ingredients);
            sortedIngs.Sort((a, b) =>
            {
                int countA = 0, countB = 0;
                foreach (var _ in a.filter.AllowedThingDefs) countA++;
                foreach (var _ in b.filter.AllowedThingDefs) countB++;
                return countA.CompareTo(countB);
            });

            int ingIdx = 0;
            foreach (IngredientCount ing in sortedIngs)
            {
                float needed = ing.GetBaseCount();
                bool satisfied = false;

                if (dbg)
                    Log.Message($"【MLF】TryMatch—ing[{ingIdx}]: filter={ing.filter.Summary}, needed={needed}");

                // 先从 droppedThings 匹配
                for (int i = 0; i < droppedThings.Count; i++)
                {
                    Thing t = droppedThings[i];
                    if (t == null || !t.Spawned) continue;
                    if (remaining[i] <= 0) continue;

                    bool filterOk = ing.filter.Allows(t);
                    // FixedIngredient 豁免 ingredientFilter（原版设计）
                    bool userFilterOk = ing.IsFixedIngredient || bill.ingredientFilter.Allows(t.def);
                    bool billOk = bill.IsFixedOrAllowedIngredient(t);
                    if (dbg && (!filterOk || !billOk || !userFilterOk))
                        Log.Message($"【MLF】TryMatch—  dropped[{i}] {t.def.defName}x{t.stackCount}: filterAllow={filterOk}, userFilter={userFilterOk}, billAllow={billOk}");
                    if (!filterOk || !billOk || !userFilterOk) continue;

                    float countValue = bill.recipe.IngredientValueGetter.ValuePerUnitOf(t.def);
                    if (dbg)
                        Log.Message($"【MLF】TryMatch—  dropped[{i}] {t.def.defName}x{remaining[i]}: valuePerUnit={countValue}");
                    if (countValue <= 0) continue;

                    int toUse = (int)Math.Ceiling(needed / countValue);
                    toUse = Math.Min(toUse, (int)remaining[i]);
                    if (toUse <= 0) continue;

                    chosen.Add(new ThingCount(t, toUse));
                    remaining[i] -= toUse;
                    needed -= toUse * countValue;

                    if (dbg)
                        Log.Message($"【MLF】TryMatch—  → 使用 {t.def.defName}x{toUse}, 剩余needed={needed}");

                    if (needed <= 0.001f) { satisfied = true; break; }
                }

                // 搜索工作台附近已有材料
                if (!satisfied)
                {
                    if (dbg)
                        Log.Message($"【MLF】TryMatch—  dropped不够，搜索工作台附近...");
                    Map map = workbench.Map;
                    int nearbyFound = 0;
                    foreach (IntVec3 cell in GenAdj.CellsOccupiedBy(workbench))
                    {
                        if (satisfied) break;
                        foreach (IntVec3 adj in GenAdj.CellsAdjacent8Way(cell, Rot4.North, IntVec2.Zero))
                        {
                            if (!adj.InBounds(map)) continue;
                            var cellThings = map.thingGrid.ThingsListAtFast(adj);
                            for (int ci = 0; ci < cellThings.Count; ci++)
                            {
                                Thing ct = cellThings[ci];
                                if (ct.def.category != ThingCategory.Item) continue;
                                if (!ct.Spawned) continue;
                                if (!ing.filter.Allows(ct)) continue;
                                // FixedIngredient 豁免 ingredientFilter（原版设计）
                                if (!ing.IsFixedIngredient && !bill.ingredientFilter.Allows(ct.def)) continue;
                                if (!bill.IsFixedOrAllowedIngredient(ct)) continue;
                                if (droppedThings.Contains(ct)) continue;

                                float cv = bill.recipe.IngredientValueGetter.ValuePerUnitOf(ct.def);
                                if (cv <= 0) continue;

                                int toUse = (int)Math.Ceiling(needed / cv);
                                toUse = Math.Min(toUse, ct.stackCount);
                                if (toUse <= 0) continue;

                                nearbyFound++;
                                chosen.Add(new ThingCount(ct, toUse));
                                needed -= toUse * cv;

                                if (dbg)
                                    Log.Message($"【MLF】TryMatch—  nearby: {ct.def.defName}x{toUse}@{adj}, 剩余needed={needed}");

                                if (needed <= 0.001f) { satisfied = true; break; }
                            }
                            if (satisfied) break;
                        }
                    }
                    if (dbg && !satisfied)
                        Log.Message($"【MLF】TryMatch—  工作台附近找到{nearbyFound}个，仍不够: needed={needed}");
                }

                if (!satisfied)
                {
                    if (dbg)
                        Log.Message($"【MLF】TryMatch—ing[{ingIdx}] 失败: {ing.filter.Summary} 缺 {needed}");
                    return null;
                }
                ingIdx++;
            }

            if (dbg)
                Log.Message($"【MLF】TryMatch—全部匹配成功! chosen={chosen.Count}个");
            return chosen.Count > 0 ? chosen : null;
        }
    }
}
