# 更新日志

[English](CHANGELOG.en.md) · [玩家版](CHANGELOG.players.zh.md)

本文件记录 **攀爬梯（Climb Ladder）** 模组的版本变更。格式参考 [Keep a Changelog](https://keepachangelog.com/zh-CN/1.1.0/)。

## [0.1.2] — 2026-11-18

### 修复

- 修正寻路边配置：去掉斜向与双向「抄近路」边，避免海狸从梯子上下钻进地下或悬空移动，并降低路网刷新后卡死、找不到路径的概率。
- 门口补边改为按邻接关系动态计算，并在路网更新时移除已失效的边，避免建筑拆除后仍保留错误连接。
- **三层房屋 + 两层梯子**：第二层梯子正上方一格（第三层门口所在层）在梯子仍为工地时也会接入正式路网，缓解顶楼「未连接道路」提示。
- 斜上方一层门口的房屋：仅从该列最高梯子先上到落脚点，再横向一步进门，中段梯子不再误连。

### 变更

- 蓝图寻路：水平边与向下边为单向；仅**向上**一格为双向，使正上方节点进入地区路网。
- 蓝图增加 `BlockObjectNavMeshAdderSpec`、`GenerateFloorsOnStackable` 与 `UnblockedCoordinates`，与原版竖向通道类建筑的寻路加载方式对齐。
- `RopeLadderLink`：工地状态下为同层门口补双向道路边；在路网变化时尝试刷新向上连接（含调试用日志 `[ClimbLadder] 向上接通`）。

### 移除

- `Sprites/Rope/` 中仅保留 `Ladder.png` 及其 meta，删除未使用的 `SideRope`、`SpanRope`、`DeleteRope` 资源。

## [0.1.1] — 2026-11-18

### 变更

- 版本号提升至 0.1.1，便于区分本地 `Documents/Timberborn/Mods/ClimbLadder` 与创意工坊订阅目录中的旧包。

## [0.1.0] — 初版

### 新增

- 路径工具组中的 1×1 攀爬梯（木族 / 铁齿），可竖向叠放并与同格道路共存。
- `RopeLadderLink`：道路装饰连接强制与附近门口补边（初版逻辑）。
- 蓝图内手动配置的寻路边与本地化、模型、创意工坊基础结构。