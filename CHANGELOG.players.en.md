# Climb Ladder — Player update notes

[中文](CHANGELOG.players.zh.md) · Developer log: [CHANGELOG.en.md](CHANGELOG.en.md)

This page is only about **what you feel in-game**. For implementation details, see the developer changelog.

---

## 0.1.2 (2026-11-18)

### What was fixed

- **Weird movement**: Beavers are much less likely to clip through floors or air along ladders.
- **Getting stuck**: Fewer bad path links after the road network updates, so beavers should wander off less often.
- **Three floors, two ladder segments, top still “not connected”**: If the third-floor door is on the cell **directly above the top ladder**, that link is attempted even while the ladder is still under construction, so the top floor can clear the “not connected to roads” warning sooner.
- **Door one tile beside the ladder and one floor up**: Beavers go up to the landing on top of the column, then step sideways—no diagonal skip through the building.

### What you should do

1. **Fully quit the game and restart** after updating the mod (don’t rely on reload only).
2. In the mod list, check **Climb Ladder** shows **0.1.2**.
3. Connect the **bottom ladder segment to ground roads** so the whole column joins the district network.
4. When stacking up, place the **top segment so the highest door can be reached**—often one segment below the top floor, with the door on the cell directly above the top ladder.
5. If you run **another ladder mod** (e.g. “Ladder” on Steam), disable one of them to avoid path conflicts.

### Still possible

- While **houses and ladders are construction sites**, “not connected” may appear until builders finish and materials are delivered. If the top floor stays red on **0.1.2**, note your layout and send feedback.

---

## 0.1.1 (2026-11-18)

- Version bump only. **No gameplay changes.**

---

## 0.1.0 — Initial release

### What you get

- **Climb Ladder** in the **Paths** toolbar: 1×1 cells, stack vertically.
- Shares a cell with **roads**; use as an exterior lift to link ground roads to upper-floor doors.
- Research **100**, **1 log** per segment; remove with the vanilla **demolish buildings** tool.

### Requirements

- Game version **1.1.0.0** or newer.