# Climb Ladder — Design notes

[中文](zh.md)

Version 0.1.0. Mod id: `Jove.ClimbLadder`. Minimum game version: 1.1.0.0.

## Current behavior

The player places `RopeLadder.{Folktails,IronTeeth}` (display name: Climbing Ladder) from the Paths tool group. Primary design: **vertical stacks** linking ground roads to upper-floor entrances; flat placement is a short-span extra. Each cell is 1×1×1, stackable (`Stackable: BlockObject`), `MatterBelow: GroundOrStackable`, occupations `Middle, Corners` — not `Path`, so it can share a cell with a vanilla road.

`PathSpec` and `IsPath` nav edges (horizontal, vertical ±Z, and diagonal supplements) provide road traversal. `RopeLadderLink` (C#):

- `IPathConnectionEnforcer`: path decoration treats neighbors as connectable (roads and other ladder cells).
- Entrance wiring: when the road mesh updates, adds edges between doorstep and entrance for nearby buildings, fixing "disconnected" housing when the ladder stack starts below the floor.

Demolition uses vanilla `BuildingDeconstructionTool`. `GroundOrStackable` plus stackable blocks lets box selection include the whole column.

## Model

- `tools/make-ladder-mesh.py` writes `.timbermesh` (zlib + protobuf): two rails, three rungs.
- Materials: `BaseWood_Brown.Folktails`, `BaseWood_DarkBrown.IronTeeth`.
- Toolbar icon: `Sprites/Rope/Ladder.png` (line art matching vanilla path buttons).

## Implementation

| File | Role |
|---|---|
| `Buildings/Paths/Rope/RopeLadder.*.blueprint.json` | Blueprint, nav edges, loc keys |
| `src/RopeLadderLink.cs` | Path connection + entrance road patches |
| `src/ClimbLadderConfigurator.cs` | Bindito: `RopeLadderSpec` → `RopeLadderLink` |
| `src/RopeLadderSpec.cs` | Blueprint marker component |
| `tools/deploy.sh` | Sync to `~/Documents/Timberborn/Mods/ClimbLadder` |

`RopeLadderVisual.cs` is leftover from an earlier rope mesh approach; the current blueprint does not attach it.

## Blueprint location

Building blueprints live under `Buildings/` at the mod root. `Blueprints/` is only for `TemplateCollections`. Wrong placement causes `Blueprint not found` at startup.

## Localization

`Localizations/*.csv` must have exactly three columns (`ID,Text,Comment`). Quote `Text` if it contains commas. Run `tools/check-localization.py` before deploy.

## Build

```bash
cd src && dotnet build -c Release
cd .. && bash tools/deploy.sh
```

Output DLL: `Scripts/Jove.ClimbLadder.dll`.