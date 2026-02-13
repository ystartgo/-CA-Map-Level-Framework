# Map Level Framework v1 技术规范（框架向 / MIT）

> 文档目标：用于**直接开工**。本规范仅定义框架职责，不绑定具体玩法。
> 适用代码基线：`【CA】地图多层框架 Map Level Framework`

---

## 1. 北极星与非目标

### 1.1 北极星（North Star）
MLF 是 RimWorld 的“垂直空间基础设施”：
- 支持任意层级（地上/地下/天空/水下等）
- 玩家交互保持“单地图感官”
- 通过统一补丁与坐标系统，让其他 Mod 能在此之上扩展玩法

### 1.2 非目标（v1 不做）
- 不实现具体玩法规则（氧气、水压、飞行生态等）
- 不实现全量跨层 AI/作业系统
- 不做多层同时显示（v1 固定“仅当前层显示”）

---

## 2. 第一性原则

1. **交互唯一真相**：任何输入都只落到当前聚焦层（或地面层）。
2. **坐标单一来源**：所有系统共享同一套坐标转换 API，不允许散落自定义换算。
3. **层级是空间，不是玩法**：框架只提供空间与生命周期，玩法由扩展 Mod 注入。
4. **最小侵入补丁**：补丁只覆盖必要链路，避免与生态 Mod 产生不必要冲突。
5. **先闭环后扩展**：先保证创建/切换/建造/保存读档闭环，再扩跨层行为。

---

## 3. 当前代码基线（已存在能力）

### 3.1 层级管理
- `LevelManager.RegisterLevel(...)` 已支持按 `elevation + area + mapSize` 建层：
  `Source/MapLevelFramework/Core/LevelManager.cs:82`
- 聚焦层切换 `FocusLevel(...)`：
  `Source/MapLevelFramework/Core/LevelManager.cs:149`
- 交互地图统一入口 `CurrentInteractionMap`：
  `Source/MapLevelFramework/Core/LevelManager.cs:227`
- 序列化入口 `ExposeData()`：
  `Source/MapLevelFramework/Core/LevelManager.cs:314`

### 3.2 坐标转换
- 主图↔子图坐标转换：
  `Source/MapLevelFramework/Core/LevelCoordUtility.cs:21`,
  `Source/MapLevelFramework/Core/LevelCoordUtility.cs:34`,
  `Source/MapLevelFramework/Core/LevelCoordUtility.cs:49`,
  `Source/MapLevelFramework/Core/LevelCoordUtility.cs:63`
- 渲染偏移 `GetDrawOffset(...)`：
  `Source/MapLevelFramework/Core/LevelCoordUtility.cs:77`

### 3.3 单地图交互补丁（已接通）
- 建造地图重定向：`Patch_Designator_Map`
  `Source/MapLevelFramework/Patches/Patch_Designator_Map.cs:17`
- 鼠标格子重定向：`Patch_UI_MouseCell`
  `Source/MapLevelFramework/Patches/Patch_UI_MouseCell.cs:19`
- 点击目标重定向：`Patch_GenUI_TargetsAt`
  `Source/MapLevelFramework/Patches/Patch_GenUI_TargetsAt.cs:14`
- 选择器重定向：`Patch_Selector`
  `Source/MapLevelFramework/Patches/Patch_Selector.cs:15`
- 防止切到子图：`Patch_Game_CurrentMap`
  `Source/MapLevelFramework/Patches/Patch_Game_CurrentMap.cs:12`

### 3.4 叠加渲染
- 主图绘制后叠加当前层：`Patch_MapDrawer`
  `Source/MapLevelFramework/Patches/Patch_MapDrawer.cs:9`
- 子图静态/动态绘制：`LevelRenderer`
  `Source/MapLevelFramework/Render/LevelRenderer.cs:34`,
  `Source/MapLevelFramework/Render/LevelRenderer.cs:81`

### 3.5 层级数据
- `LevelData.area` 已定义覆盖区域；`usableCells` 已预留不规则可用格子：
  `Source/MapLevelFramework/Core/LevelData.cs:19`,
  `Source/MapLevelFramework/Core/LevelData.cs:63`

---

## 4. v1 交付边界（开工版）

### 必达
1. 楼梯触发建层（Up/Down）
2. 二层（或任意层）只覆盖“应存在区域”，不是整张主图
3. 仅当前层显示，保持单地图交互感
4. 建造/选择/目标/鼠标 坐标一致
5. 存档读档后层级结构与内容一致

### 延后
- 跨层作业、跨层寻路、跨层弹道
- 复杂环境传播（温度/气体/天气差异）

---

## 5. 关键场景规格（你给的 13×13 房间）

### 场景定义
- 玩家（李华）在主图造出 13×13 封闭房间
- 放置楼梯后可到二楼
- 二楼地图只覆盖“有支撑的区域”
- 一楼视图不应被二楼错误遮挡（露天处保持露天）

### 判定流程（框架层）
输入：`baseMap + stairCell + targetElevation`

1. **连通候选集**（基于楼梯锚点）
   - 以 `stairCell` 为起点做 Flood Fill（4 邻接）
   - 仅扩展满足 `IsUpperFloorSupported(baseCell)` 的格子

2. **支撑判定 `IsUpperFloorSupported`（v1 默认规则）**
   - `Roofed(baseCell)` 为 true（“一楼屋顶是二楼地板”的最小语义）
   - 且满足其一：
     - 该格有可站立地面（地板/非不可行走地形）
     - 或该格有实体支撑建筑（如墙等 Full fillage）
   - 并允许扩展 Mod 注入额外支撑规则

3. **生成层级几何**
   - 将候选集压缩为最小包围矩形 `areaRect`
   - `mapSize = areaRect.Size`（保证“二楼 map 小于一楼 map”）
   - 将候选集映射到子图坐标保存为 `usableCells`

4. **创建 LevelMap**
   - 调用现有 `RegisterLevel(elevation, areaRect, ..., mapSize)`
   - 非 `usableCells` 区域标记为不可建造/不可站立（逻辑层强约束）

5. **聚焦显示**
   - 自动 `FocusLevel(targetElevation)`
   - 保持当前的“仅当前层显示”策略

---

## 6. 单地图感官规范（核心）

1. **当前地图始终是主图**（游戏层面）
   - 通过 `Patch_Game_CurrentMap` 把对子图切换请求转回主图
2. **交互目标是当前聚焦层**（输入层面）
   - `Designator.Map / UI.MouseCell / TargetsAt / Selector` 全链路重定向
3. **显示只画当前层**（渲染层面）
   - `Patch_MapDrawer` 仅在聚焦层时叠加
   - 不聚焦时不叠加，露天区域自然不会被遮挡

---

## 7. 补丁矩阵（v1）

### 7.1 已有（保留并加测试）
- `Designator.Map` getter
- `UI.MouseCell`
- `GenUI.TargetsAt`
- `Selector.SelectableObjectsUnderMouse`
- `Game.CurrentMap` setter
- `MapDrawer.DrawMapMesh`
- `GenSpawn.Spawn`（Projectile/Mote 特例）

### 7.2 v1 必补（新加）
1. `GenSpawn.SpawningWipes`：跨层/同层遮挡清除判定正确化
2. `GenSpawn.CanSpawnAt`：禁止在无支撑或不可用层格生成
3. `GenConstruct.CanPlaceBlueprintAt`：蓝图放置按 `usableCells` 与层边界校验
4. （可选）`ThingGrid` 查询链路防串层：避免逻辑误命中非聚焦层

---

## 8. 数据模型规范

### 8.1 LevelData 最低字段
- `elevation`
- `area`（主图坐标）
- `mapParent` / `LevelMap`
- `usableCells`（子图坐标不规则掩码）

### 8.2 序列化要求
- `usableCells` 必须纳入保存/读取
- 读档后需重建：`hostMap`、`mapParent`、`focusedElevation` 一致性

> 备注：当前 `usableCells` 已定义但尚未序列化，需补齐。
> 代码位置：`Source/MapLevelFramework/Core/LevelData.cs:63`, `Source/MapLevelFramework/Core/LevelData.cs:82`

---

## 9. 对外 API（给其他 Modder）

v1 需要稳定的最小接口：
1. `TryCreateLevelFromConnector(...)`：从连接器（楼梯等）建层
2. `TryGetLevelAtCell(baseMap, baseCell, elevation, out LevelData)`
3. `ToLevelCoord / ToBaseCoord`（仅使用统一坐标 API）
4. `IsUsableCell(level, levelCell)`
5. 事件：`OnLevelRegistered / OnLevelRemoved / OnFocusedLevelChanged`

扩展 Mod（如水下/天空）只需：
- 提供支撑规则扩展
- 提供层语义标签（`levelTag`）
- 提供自身玩法系统，不改框架核心

---

## 10. 验收标准（Definition of Done）

### A. 创建与覆盖范围
- 在 13×13 封闭房间放置楼梯，生成 elevation=1 层
- 新层 map 尺寸等于有效区域包围矩形，不等于主图尺寸
- 不支持区域不生成层

### B. 交互一致性
- 聚焦 2F 时：鼠标高亮、选择、建造、目标选择都落在 2F
- 回到地面时：以上能力回归地面

### C. 渲染一致性
- 地面视图下无 2F 遮挡露天区域
- 2F 视图仅显示 2F（v1）

### D. 存档稳定性
- 保存→读档后层级数量、elevation、区域、聚焦层一致
- 2F 建筑/物品位置不漂移

### E. 破坏回收
- 拆除连接器（楼梯）可触发层级回收或失效标记（按策略）

---

## 11. 开工顺序（无时间估算）

1. **数据闭环**
   - 补 `usableCells` 序列化
   - 增加读档一致性校验

2. **楼梯建层链路**
   - Connector Def + Comp
   - 支撑判定 + Flood Fill + 生成 `areaRect + usableCells`
   - 调用 `RegisterLevel`

3. **放置与生成边界**
   - `CanSpawnAt / CanPlaceBlueprintAt / SpawningWipes` 补丁
   - 全部走 `usableCells` 校验

4. **渲染与交互回归**
   - 校验“仅当前层显示”与露天不遮挡
   - 快捷键切层（可选）

5. **MIT 公开准备**
   - API 注释
   - 扩展示例（最小）
   - 兼容性声明与冲突边界说明

---

## 12. 许可与生态声明

- 本项目采用 **MIT**，允许二次开发、商用、再分发。
- 框架目标是成为“多层空间标准层”，欢迎社区在其上实现水下/天空/地底玩法。

---

## 附：当前已验证的核心入口（供开发定位）

- `Source/MapLevelFramework/Core/LevelManager.cs:82`
- `Source/MapLevelFramework/Core/LevelManager.cs:149`
- `Source/MapLevelFramework/Core/LevelCoordUtility.cs:21`
- `Source/MapLevelFramework/Patches/Patch_UI_MouseCell.cs:41`
- `Source/MapLevelFramework/Patches/Patch_Designator_Map.cs:17`
- `Source/MapLevelFramework/Patches/Patch_MapDrawer.cs:9`
- `Source/MapLevelFramework/Render/LevelRenderer.cs:34`
- `Source/MapLevelFramework/Core/LevelData.cs:63`
