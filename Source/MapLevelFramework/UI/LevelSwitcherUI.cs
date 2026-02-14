using System.Linq;
using UnityEngine;
using Verse;

namespace MapLevelFramework.Gui
{
    /// <summary>
    /// 层级切换 UI - 在游戏界面上显示层级切换按钮。
    /// 通过 MapComponent.MapComponentOnGUI 调用。
    /// </summary>
    public static class LevelSwitcherUI
    {
        private const float ButtonWidth = 40f;
        private const float ButtonHeight = 30f;
        private const float Padding = 4f;

        public static void DrawLevelSwitcher(LevelManager mgr)
        {
            if (mgr == null) return;

            var elevations = mgr.AllElevations.ToList();
            if (elevations.Count == 0) return;

            // 添加地面层
            elevations.Insert(0, 0);

            // 按 elevation 从高到低排列（高楼在上）
            elevations.Sort((a, b) => b.CompareTo(a));

            // 计算 UI 位置（右侧中间）
            float totalHeight = elevations.Count * (ButtonHeight + Padding);
            float startX = Verse.UI.screenWidth - ButtonWidth - 20f;
            float startY = (Verse.UI.screenHeight - totalHeight) / 2f;

            // 绘制背景
            Rect bgRect = new Rect(startX - 5f, startY - 5f,
                ButtonWidth + 10f, totalHeight + 10f);
            Widgets.DrawBoxSolid(bgRect, new Color(0.1f, 0.1f, 0.1f, 0.7f));
            Widgets.DrawBox(bgRect);

            // 绘制按钮
            float y = startY;
            foreach (int elev in elevations)
            {
                Rect btnRect = new Rect(startX, y, ButtonWidth, ButtonHeight);

                bool isFocused = mgr.FocusedElevation == elev;
                string label = elev == 0 ? "1F" : (elev > 0 ? $"{elev + 1}F" : $"B{-elev}");

                // 高亮当前层
                if (isFocused)
                {
                    Widgets.DrawBoxSolid(btnRect, new Color(0.3f, 0.5f, 0.8f, 0.8f));
                }

                if (Widgets.ButtonText(btnRect, label, true, true, true))
                {
                    if (mgr.FocusedElevation != elev)
                    {
                        mgr.FocusLevel(elev);
                    }
                }

                y += ButtonHeight + Padding;
            }
        }
    }
}
