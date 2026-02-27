# 【CA】地图多层框架 Map Level Framework

## 中文介绍

### 概述

RimWorld 首个真实多层建筑框架。在单张地图上实现多层建筑（上层）和无限地下空间（地下层），所有层级共享同一坐标系，殖民者可通过楼梯在层级间自由移动。

**这不是视觉效果，每个层级都是真实的地图，拥有完整的原版系统。**

### 核心特性

#### 🏗️ 真实多层建筑
- 放置上楼梯 → 自动扫描屋顶连通区域 → 创建对应层级
- 屋顶-地板双向同步：下层加屋顶 = 上层铺地板，上层拆地板 = 下层拆屋顶
- 支持最多 18 层（受渲染深度缓冲约束）
- 每个层级是独立的 PocketMap，拥有完整的 AI、需求、工作、寻路系统

#### ⛏️ 无限地下空间
- 放置下楼梯 → 创建地下层（全图厚岩石顶）
- 初始可用区域为楼梯一格，周围 8 格生成原版岩石（花岗岩/大理石/石灰岩/砂岩/板岩 + 4% 矿脉）
- 挖掘岩石 → 动态扩展边界 → 想挖多深挖多深
- 建造地下仓库、工坊、监狱、矿场

#### 🤖 智能跨层 AI
**殖民者自动跨层工作，无需手动指挥：**
- 自动发现其他楼层的工作（建造、清洁、搬运、制作、研究等）
- 自动跨层满足需求（饥饿 → 下楼吃饭，疲劳 → 下楼睡觉，娱乐 → 下楼玩耍）
- 袭击者会通过楼梯追击到不同楼层

#### 📦 智能跨层材料配送
**工作台缺原料？殖民者自动从其他楼层搬运：**
- 支持建造（蓝图/框架）、加油（燃料）、制作/烹饪（Bill 原料）
- 单材料模式：手持材料走楼梯送达
- 多材料模式：依次捡进背包 → 走楼梯 → 全部丢在工作台旁 → 直接开工
- 智能材料匹配：
  - 窄 filter 优先（防止宽 filter 消耗窄 filter 需要的材料）
  - 尊重用户的材料过滤设置
  - 优先选择非固定材料作为产物材质（精粹数值更好？产物就用精粹）

#### ⚡ 跨层电力系统
- 电池中继组件：连接不同楼层的电网
- 1F 发电机 → 电池中继 → 2F 建筑供电

#### 💥 物理系统
- **坠落**：拆除下层屋顶 → 上层物品和殖民者掉落（殖民者受 10-35 钝伤，物品损 25-75% HP）
- **跳楼**：右键菜单从露天边缘跳下（15-40 钝伤）
- **跳楼精神崩溃**：极端崩溃时殖民者会冲向楼层边缘跳下

#### 🎮 完整交互重定向
- 点击/拖选/右键菜单 → 自动重定向到聚焦层级
- 建造/区划/指定器 → 在子地图上操作
- 殖民者栏/工作面板/日程面板 → 包含所有层级的殖民者
- 警报系统 → 聚合所有层级的资源数据
- 物品可用性/配方计数/贸易 → 跨层聚合

### 使用方法

1. **建造上层**：
   - 在有屋顶的房间内放置"楼层传送器（上）"
   - 系统自动扫描屋顶连通区域并创建 2F
   - 使用层级切换器（左侧 UI）切换到 2F
   - 在 2F 建造墙壁、门、家具
   - 殖民者会自动通过楼梯上下楼搬运材料

2. **建造地下层**：
   - 在 1F 放置"楼层传送器（下）"
   - 系统自动创建 B1（地下一层）
   - 切换到 B1，指定殖民者挖掘岩石
   - 岩石被挖掘后，边界自动扩展，生成新岩石
   - 建造地下设施

3. **跨层工作**：
   - 殖民者会自动发现其他楼层的工作
   - 无需手动指挥，他们会自动走楼梯前往
   - 工作完成后自动返回或继续寻找下一个工作

### 性能说明

- 在 374-mod 环境下深度测试，性能优化到位
- 使用叠加渲染技术，避免重复绘制
- 智能 Section 管理，只渲染活跃区域
- 定期清理缓存，防止内存泄漏

### 兼容性

- ✅ RimWorld 1.5/1.6
- ✅ Odyssey DLC（逆重飞船起飞时自动保存层级数据，降落后完整恢复）
- ✅ 大部分主流 mod（在 374-mod 环境下测试）
- ⚠️ 可能与修改地图生成/渲染的 mod 冲突

### 已知问题

- 地下层使用默认光照（未来会优化）
- 电力覆盖层在 2F+ 可能显示异常（已修复）

### 技术细节

- 80+ C# 源文件
- 50+ Harmony 补丁
- 覆盖层级管理、渲染、AI、物理、交互重定向、跨层材料配送、跨层电力全链路
- 使用 PocketMap 技术实现真实多层
- 使用 Graphics.DrawMesh + Y 偏移实现叠加渲染
- 使用意图系统（CrossFloorIntent）管理跨层行为

### 更新日志

**v1.0.0**
- 初始版本发布
- 上层建筑系统
- 地下空间系统
- 跨层 AI
- 跨层材料配送
- 跨层电力系统
- 物理系统
- 完整交互重定向

### 致谢

感谢所有测试者的反馈和建议。

### 作者

Cagier.阳 (CA)

---

## English Description

### Overview

The first true multi-level building framework for RimWorld. Build multi-story structures (upper floors) and infinite underground spaces (basement levels) on a single map. All levels share the same coordinate system, and colonists can freely move between floors using stairs.

**This is not a visual effect - each level is a real map with complete vanilla systems.**

### Core Features

#### 🏗️ Real Multi-Level Buildings
- Place upstairs → Auto-scan roofed areas → Create corresponding level
- Roof-floor bidirectional sync: Add roof on 1F = Auto-floor on 2F, Remove floor on 2F = Remove roof on 1F
- Support up to 18 levels (limited by rendering depth buffer)
- Each level is an independent PocketMap with complete AI, needs, work, and pathfinding systems

#### ⛏️ Infinite Underground Space
- Place downstairs → Create basement level (full-map thick rock roof)
- Initial usable area is one cell at stairs, surrounded by 8 cells of vanilla rocks (granite/marble/limestone/sandstone/slate + 4% ore veins)
- Mine rocks → Dynamic boundary expansion → Dig as deep as you want
- Build underground storage, workshops, prisons, mines

#### 🤖 Smart Cross-Floor AI
**Colonists automatically work across floors without manual commands:**
- Auto-discover work on other floors (construction, cleaning, hauling, crafting, research, etc.)
- Auto-satisfy needs across floors (hungry → go downstairs to eat, tired → go downstairs to sleep, recreation → go downstairs to play)
- Raiders will chase colonists through stairs to different floors

#### 📦 Smart Cross-Floor Material Delivery
**Workbench needs ingredients? Colonists automatically haul from other floors:**
- Support construction (blueprints/frames), refueling (fuel), crafting/cooking (bill ingredients)
- Single-material mode: Carry material, walk stairs, deliver
- Multi-material mode: Pick up multiple items into inventory → Walk stairs → Drop all near workbench → Start working immediately
- Smart material matching:
  - Narrow filter priority (prevent broad filters from consuming materials needed by narrow filters)
  - Respect user's ingredient filter settings
  - Prefer non-fixed ingredients as product material (better stats? Use better material for product)

#### ⚡ Cross-Floor Power System
- Battery relay component: Connect power grids on different floors
- 1F generator → Battery relay → 2F building powered

#### 💥 Physics System
- **Falling**: Remove lower floor roof → Upper floor items and colonists fall (colonists take 10-35 blunt damage, items lose 25-75% HP)
- **Jumping**: Right-click menu to jump from open edge (15-40 blunt damage)
- **Jump-off mental break**: Extreme mental break causes colonists to rush to floor edge and jump

#### 🎮 Complete Interaction Redirection
- Click/drag-select/right-click menu → Auto-redirect to focused level
- Build/zone/designators → Operate on sub-maps
- Colonist bar/work panel/schedule panel → Include colonists from all levels
- Alert system → Aggregate resource data from all levels
- Item availability/recipe counting/trading → Cross-floor aggregation

### How to Use

1. **Build Upper Floors**:
   - Place "Floor Transporter (Up)" in a roofed room
   - System auto-scans roofed connected areas and creates 2F
   - Use level switcher (left UI) to switch to 2F
   - Build walls, doors, furniture on 2F
   - Colonists will automatically use stairs to haul materials

2. **Build Basement Levels**:
   - Place "Floor Transporter (Down)" on 1F
   - System auto-creates B1 (basement level 1)
   - Switch to B1, designate colonists to mine rocks
   - After rocks are mined, boundary auto-expands, generating new rocks
   - Build underground facilities

3. **Cross-Floor Work**:
   - Colonists will auto-discover work on other floors
   - No manual commands needed, they will automatically use stairs
   - After work is done, they auto-return or continue finding next work

### Performance

- Deeply tested in 374-mod environment, performance optimized
- Use overlay rendering technology to avoid duplicate drawing
- Smart Section management, only render active areas
- Periodic cache cleanup to prevent memory leaks

### Compatibility

- ✅ RimWorld 1.5/1.6
- ✅ Odyssey DLC (Auto-save level data when gravship takes off, fully restore after landing)
- ✅ Most mainstream mods (tested in 374-mod environment)
- ⚠️ May conflict with mods that modify map generation/rendering

### Known Issues

- Basement levels use default lighting (will be optimized in the future)
- Power overlay may display abnormally on 2F+ (fixed)

### Technical Details

- 80+ C# source files
- 50+ Harmony patches
- Cover level management, rendering, AI, physics, interaction redirection, cross-floor material delivery, cross-floor power
- Use PocketMap technology to implement real multi-level
- Use Graphics.DrawMesh + Y offset to implement overlay rendering
- Use intent system (CrossFloorIntent) to manage cross-floor behavior

### Changelog

**v1.0.0**
- Initial release
- Upper floor building system
- Underground space system
- Cross-floor AI
- Cross-floor material delivery
- Cross-floor power system
- Physics system
- Complete interaction redirection

### Credits

Thanks to all testers for feedback and suggestions.

### Author

Cagier.阳 (CA)
