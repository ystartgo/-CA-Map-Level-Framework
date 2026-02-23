using RimWorld;

namespace MapLevelFramework
{
    /// <summary>
    /// 楼梯专用电池组件。无自放电，纯粹作为跨层电力传输的中继。
    /// </summary>
    public class CompPowerBatteryRelay : CompPowerBattery
    {
        public override void CompTick()
        {
            // 跳过原版 5W 自放电，楼梯电池只做传输中继
        }
    }
}
