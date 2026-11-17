# 攀爬梯 — 设计说明

[English](en.md)

版本 0.1.0。Mod Id：`Jove.ClimbLadder`。最低游戏版本：1.1.0.0。

## 当前行为

玩家从路径工具组放置 `RopeLadder.{Folktails,IronTeeth}`（显示名：攀爬梯）。设计目标为**竖向叠放**，服务多层建筑门口连通；同层平铺仅作短距辅助。每格 1×1×1，可叠放（`Stackable: BlockObject`），`MatterBelow: GroundOrStackable`，占用 `Middle, Corners`，不占 `Path`，因此可与同格道路共存。

蓝图内 `PathSpec` + `IsPath` 道路边（含水平四向与竖向 ±Z，以及斜向补边）提供寻路。`RopeLadderLink`（C#）负责：

- `IPathConnectionEnforcer`：让道路装饰模型把相邻格视为可连接（含其他攀爬梯与道路）。
- 门口连通：对附近带入口的建筑，在路网更新时补「门口外 ↔ 门内」及梯子列的道路边，解决梯子放在楼下时楼房显示未连接的问题。

删除走原版 `BuildingDeconstructionTool`；`GroundOrStackable` + 可叠放使框选时能整列进入拆除列表。

## 模型

- `tools/make-ladder-mesh.py` 生成 `.timbermesh`（ zlib + protobuf），两根立柱 + 三根横档。
- 材质：木族 `BaseWood_Brown.Folktails`，铁齿 `BaseWood_DarkBrown.IronTeeth`。
- 图标：`Sprites/Rope/Ladder.png`，线稿风格对齐原版路径按钮。

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