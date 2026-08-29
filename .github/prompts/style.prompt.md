---
description: "Apply the SiegeTower flush-grid composition style when creating or changing UI."
agent: "agent"
---
# SiegeTower Grid Style

Build UI from a flush, consistent grid. The core grid unit is `2rem`. Structural elements touch; only content-bearing elements such as text and icons receive internal padding.

The interface uses a monospace font to reinforce the grid. Text and Font Awesome icons are content inside the grid, not replacements for structural layout rules.

## Composition Rules

- Compose the application as a `grid-shell`: fixed title and toolbar rows at whole grid-unit heights, followed by one `grid-dock-region` that fills the remaining height, and a final `grid-status-bar`.
- Use `grid-box` for structural container elements such as panels, tables, flex regions, grids, and content areas.
- Keep structural elements flush to their parent and siblings. Do not use margin or gap to create ordinary composition space.
- Use `grid-line` for horizontal rows and bars, and use `grid-stack` when a vertical stack primitive is added.
- Use `grid-divider` for a divider inside a toolbar. It occupies one `2rem` cell and renders a centered `|` without padding or gaps.
- Compose the dock region from a left dock, a fill middle dock, and a right dock. Docks fill the available height; left and right widths may use existing dock width utilities.
- Use `grid-dock`, `grid-dock-fill`, `grid-tabs`, `grid-tab`, and `grid-dock-content` to keep dock tabs and active content in separate regions.
- Use `grid-height-fill` and `grid-width-fill` only when a region should consume remaining space.
- Use existing width utilities for standard dock columns instead of introducing one-off widths.

## Unit Rules

- Use `2rem` as the core structural dimension. Prefer `2rem`, `4rem`, `6rem`, and other integer multiples for rows, columns, and fixed regions.
- Keep typography monospace so characters occupy a predictable horizontal rhythm within grid cells.
- Use the font rhythm to support alignment and scanning, but do not size structural regions by character count or assume text will always fit.
- Keep standard rows at one or more whole grid units. Let unrepresented horizontal space remain at the right edge of a row.
- Keep standard stacks at one or more whole grid units. Let unrepresented vertical space remain at the bottom of a stack.
- Allow content to overflow or grow by whole grid units when it cannot fit; do not compress the grid to remove remainder pixels.
- Use `grid-padded` for internal space around text, icons, and other content that needs breathing room. Padding must not create space between structural siblings.
- Use dividers only between components within one toolbar. Do not use them to divide whole toolbars; each toolbar begins with its grip icon.

## Dock Rules

- Each dock has a `2rem` tab strip above its active content. Tabs do not shrink; `grid-tabs` owns horizontal scrolling when they do not fit.
- Changing tabs replaces only the active dock content. It must not change the dock region's outer size or the height of the tab strip.
- The dock region owns the remaining application height. Dock content must not use a hardcoded viewport height.
- Content decides how it responds to limited space. Use `grid-overflow-x` or `grid-overflow-y` for scrollable content and `grid-wrap-x` when text should wrap.
- Leave content underflow unfilled when its intrinsic size is smaller than the dock. Do not add spacer gaps to make it appear full.
- Keep overflow and wrapping policies on the content view, not on the shell or dock, so editable text, chat logs, and other content can behave differently.

## Status Bar Rules

- Use `grid-status-bar` as the final child of `grid-shell` for application-wide loading state, connection state, and recent actions.
- Treat the status bar as the intentional half-unit exception: it is `1rem` high, uses `.5rem` typography, and uses `.25rem` padding.
- Keep the status bar on one non-wrapping row. It may scroll horizontally when its messages do not fit.
- The dock region fills the space above the status bar; the status bar must not overlap dock content or scroll with it.

## Space Ownership

- Before adding space, identify its owner:
	- structural space is flush;
	- remaining space belongs at the edge or to an explicit fill element;
	- content space belongs inside the content-bearing element.
- A toolbar divider is an explicit structural cell, not a border attached to a neighboring component.
- Do not add bespoke margins, gaps, padding, or dimensions in components when a `grid-*` primitive expresses the intent.
- Add a new grid primitive only when the layout relationship is repeated and cannot be expressed by the existing vocabulary.
- Keep visual styling separate from grid composition.

Treat the interface as a dense workbench of aligned slots. Favor a clean, functional grid over rounded, bubbly, card-heavy, or decorative composition.
