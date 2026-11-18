# Changelog

[中文](CHANGELOG.zh.md) · [For players](CHANGELOG.players.en.md)

All notable changes to **Climb Ladder** are documented here. The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/).

## [0.1.2] — 2026-11-18

### Fixed

- Nav mesh edges: removed diagonal and spurious two-way links that let beavers path through the ground or air and could strand them after road-network updates.
- Entrance patching now derives links from actual adjacency and drops stale edges when the nav mesh is rebuilt (e.g. after demolishing buildings).
- **Three-story buildings with two ladder segments**: the cell directly above the top ladder segment (where the third-floor entrance sits) joins the **regular** road network even while the ladder is still a construction site, reducing “not connected to roads” on the top floor.
- Doors one floor up and one tile sideways: connection only from the top of the column (up to landing, then one step sideways), not from mid-column ladders.

### Changed

- Blueprint pathing: horizontal and downward edges are one-way; only the **up** edge is two-way so the cell above counts as district road.
- Blueprint adds `BlockObjectNavMeshAdderSpec`, `GenerateFloorsOnStackable`, and `UnblockedCoordinates`, aligned with how vanilla vertical path buildings register nav mesh.
- `RopeLadderLink`: two-way road edges to same-floor entrances while unfinished; refreshes upward links on road updates (debug log: `[ClimbLadder] 向上接通` / upward link message).

### Removed

- Unused rope sprites under `Sprites/Rope/`; only `Ladder.png` and its meta remain.

## [0.1.1] — 2026-11-18

### Changed

- Version bumped to 0.1.1 to tell apart the local install (`Documents/Timberborn/Mods/ClimbLadder`) from an older Steam Workshop copy.

## [0.1.0] — Initial

### Added

- 1×1 climb ladder in the Paths tool group (Folktails / IronTeeth), stackable vertically, can share a cell with vanilla roads.
- `RopeLadderLink` for path-decoration connectivity and entrance patching (initial behavior).
- Blueprint nav edges, localizations, models, and workshop-ready layout.