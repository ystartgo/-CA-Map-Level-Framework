using UnityEngine;
using Verse;

namespace MapLevelFramework
{
    public class MLF_Settings : ModSettings
    {
        public bool debugPathfindingAndJob;
        public bool transcendentPowerMode;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref debugPathfindingAndJob, "debugPathfindingAndJob", false);
            Scribe_Values.Look(ref transcendentPowerMode, "transcendentPowerMode", false);
        }

        public void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard listing = new Listing_Standard();
            listing.Begin(inRect);
            listing.CheckboxLabeled(
                "寻路与job检测日志",
                ref debugPathfindingAndJob,
                "开启后在日志中输出跨层寻路和工作分配的详细信息。格式：【MLF】寻路与job检测-{pawn}—...");
            listing.CheckboxLabeled(
                "超凡储电模式",
                ref transcendentPowerMode,
                "开启后楼层传送器的储电容量和充电效率将达到超凡级别。关闭时储电上限等于电网总功率。\n不要问为什么，问就是超凡科技。");
            listing.End();
        }
    }
}
