---
description: "Apply the SiegeTower UX spacing and flush-grid layout style when creating or changing UI."
agent: "agent"
---
# SiegeTower UX Style

Build UI as a flush, consistent grid. Avoid decorative wrappers and ad hoc spacing around containers.

## Layout Rules

- Use `ux-box` for container elements such as panels, tables, flex regions, grids, and content areas.
- Keep `ux-box` containers flush to their parent: no unnecessary margin, padding, or wrapper elements.
- Use `ux-height-fill` and `ux-width-fill` when a region should fill the available space.
- Use the existing sidedock width utility for standard dock columns instead of introducing one-off widths.

## Rhythm Rules

- Use `ux-line` for horizontal rows and bars: menu bars, title bars, dock tabs, table rows, and similar stacked UI elements.
- Keep line-based elements on the shared UX line height unless the design explicitly requires a fill-height region.
- Use `ux-padded` for text and compact inline content inside a line or container, including labels, icons, and table-cell content.
- Do not add padding to both a container and its inner text unless both levels have a clear layout purpose.
- Prefer one standard UX utility over bespoke per-component spacing.

## Visual Direction

Treat the UX as a workbench made of aligned slots with a shared line height and consistent inner padding. Favor a clean, dense, functional, brutalist grid over rounded, bubbly, card-heavy, or social-media styling.

Before adding a new spacing rule or wrapper, check whether `ux-box`, `ux-line`, `ux-padded`, `ux-height-fill`, or `ux-width-fill` already expresses the intent.
