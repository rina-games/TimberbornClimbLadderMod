# 攀爬梯 — 设计说明

[English](en.md)

版本 0.1.2。Mod Id：`Jove.ClimbLadder`。最低游戏版本：1.1.0.0。玩家向说明见 [使用说明](使用说明.md)、[更新说明](../CHANGELOG.players.zh.md)。

## 当前行为

玩家从路径工具组放置 `RopeLadder.{Folktails,IronTeeth}`（显示名：攀爬梯）。设计目标为**竖向叠放**，服务多层建筑门口连通；同层平铺仅作短距辅助。每格 1×1×1，可叠放（`Stackable: BlockObject`），`MatterBelow: GroundOrStackable`，占用 `Middle, Corners`，不占 `Path`，因此可与同格道路共存。

蓝图内 `PathSpec` + `IsPath` 道路边提供寻路。水平边和向下的边是**单向**的，要对面也有回头边才接通，所以不会钻进地下。向上的边是**双向**的：梯子正上方那一格（高层门口常在这里）会进入路网；没有建筑时它只是死胡同，不会成为抄近路。`RopeLadderLink`（C#）负责：

- `IPathConnectionEnforcer`：让道路装饰模型把相邻格视为可连接（含其他攀爬梯与道路）。
- 门口：同一层紧挨着，或正在梯子正上方时，补一条从门口回到梯子的边。顶格斜上方一格的门口，只从这一列最高的梯子先上到落脚点，再横跨一步进门。不从中段抄近路，也不向地下拉边。路网刷新后会拆掉不再成立的边。

删除走原版 `BuildingDeconstructionTool`；`GroundOrStackable` + 可叠放使框选时能整列进入拆除列表。

## 模型

- `tools/make-ladder-mesh.py` 生成 `.timbermesh`（ zlib + protobuf），两根立柱 + 三根横档。
- 材质：木族 `BaseWood_Brown.Folktails`，铁齿 `BaseWood_DarkBrown.IronTeeth`。
- 图标：`Sprites/Rope/Ladder.png`，由 `tools/make-ladder-icon.py` 生成。线色 `#BBA16C`、线宽约 4.8 px / 128、圆头笔画、黑色柔和投影（alpha 峰值 ~140）都是从游戏 `resources.assets` 里的 `PathIcon` / `StairsIcon` 实测量取的；造型为 1×1 木框——两根竖轨 + 顶部横梁 + 三根外凸横档，对应实机的方框绳梯。

## 实现

| 文件 | 作用 |
|---|---|
| `Buildings/Paths/Rope/RopeLadder.*.blueprint.json` | 建筑蓝图、寻路边、本地化键 |
| `src/RopeLadderLink.cs` | 道路连接与门口路网补丁 |
| `src/ClimbLadderConfigurator.cs` | Bindito 注册 `RopeLadderSpec` → `RopeLadderLink` |
| `src/RopeLadderSpec.cs` | 蓝图组件标记 |
| `tools/deploy.sh` | 同步到 `~/Documents/Timberborn/Mods/ClimbLadder` |

`RopeLadderVisual.cs` 为早期程序化绳索视觉，当前蓝图未挂 `RopeLadderSpec` 装饰到该组件，可视为遗留代码。

## 蓝图位置

建筑蓝图必须在 mod 根目录的 `Buildings/` 下。`Blueprints/` 仅放 `TemplateCollections`。否则会 `Blueprint not found`。

## 本地化

`Localizations/*.csv` 必须严格三列（`ID,Text,Comment`）。文案里若有逗号，整段 `Text` 用双引号包住。可用 `tools/check-localization.py` 检查。

## 构建

```bash
cd src && dotnet build -c Release
cd .. && bash tools/deploy.sh
```

输出 DLL：`Scripts/Jove.ClimbLadder.dll`。