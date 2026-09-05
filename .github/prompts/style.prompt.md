---
description: "Apply the SiegeTower flush-grid composition style when creating or changing UI."
agent: "agent"
---
# SiegeTower Grid Style

Build UI from a flush, consistent grid. The core grid unit is `2rem`. Structural elements touch; only content-bearing elements such as text and icons receive internal padding.

The interface uses a monospace font to reinforce the grid. Text and Font Awesome icons are content inside the grid, not replacements for structural layout rules.

## Complexity Hint

- Reduce complexity before adding components, state, or styling. Keep the UI focused on the small set of elements the user directly interacts with.
- Prefer the simplest usable composition over feature-heavy or decorative UI. Reuse the same views and screen structure wherever the interaction is the same.
- Put standardized, reusable layout behavior in `grid.css`. Extend the shared `grid-*` vocabulary for repeated rows, alignment, sizing, overflow, scrolling, and spacing instead of adding one-off component rules.
- Keep screen composition, rendering, and layout responsibilities separate: ECS components own data and relationships, reusable views render them, and grid CSS controls spatial behavior.
- When debugging missing or incorrect UI, trace the smallest path from route to screen factory, ECS components, view queries, rendered markup, and CSS layout. Fix the first point where the expected content or behavior is lost.

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
- Compose UI components from grid classes first. A component-specific class may identify semantics or provide behavior that cannot be expressed by the grid, but must not create a competing spacing or sizing system.

## Component and View Rules

- Treat every ECS component type as a table keyed by `Entity`. A view or system query is a join across the component tables needed for its behavior; an entity can have one of each joined component type.
- Use `Control` as the common typed UI primitive. Compose controls with typed relationship components rather than creating alternate control hierarchies. For example, `ToolbarControl` joins a standard `Control` to a toolbar without replacing its control behavior.
- Keep UI component data and relationships in ECS components. Use `IRequires<Control>` for components that require a standard control, and use additional `IRequires<>` relationships to enforce other required data.
- Use typed `Layout` components and their relationships to express layout membership, placement, grouping, and ordering. Do not encode component relationships in CSS selectors or view-local collections.
- `ToolbarLayout` and `Toolbar` own toolbar placement and ordering. `ToolbarControl` joins controls to that model. Keep toolbar drag-and-drop ordering bespoke to the toolbar systems and components.
- `DockWindows` own dock-window layout, grouping, and drag-and-drop behavior. Keep dock-window interaction separate from toolbar ordering; do not force both domains through a generic drag-and-drop relationship.
- `Element`, `ElementSystem`, `MenuComponent`, and `MenuItemComponent` are legacy migration surfaces. Do not use them for new UI or extend their behavior; migrate touched UI toward typed `Controls` and `Layouts` when practical. Remove the `Element` system and its remaining usages once its typed replacements cover the affected UI.
- Use reusable `{Component}View.razor` files to render ECS components. Views should receive components as parameters, compose their markup from shared CSS classes, and avoid owning persistent UI state.
- Name reusable component Razor files with the `View` postfix, such as `BreadcrumbView.razor`, `ControlView.razor`, and `ToolbarView.razor`. Reserve other Razor names for application roots, pages, and layouts.
- Store persistent UI state on ECS components, not in views. Selected tabs, expanded nodes, inactive state, and toolbar or dock-window drag state belong to components or `Session`.
- Use `Session` for global interactions such as navigation, dragging, resizing, selection coordination, and context menus. Views render the resulting state.
- Reuse the same view for the same component in different contexts. Use role classes such as `tabs-top` and `tabs-subwindow` to describe visual placement without duplicating the component view.

## CSS Composition Rules

- Use one semantic base class only when a component needs a stable semantic hook or component-specific behavior. Do not repeat grid rules in base classes.
- Use `grid-*` classes for spatial composition: dimensions, alignment, padding, fill behavior, indentation, overflow, scrolling, and separators.
- Use `color-*` classes for semantic surfaces: `color-primary`, `color-secondary`, `color-success`, and `color-danger`.
- Use `is-*` classes for visual state: `is-hoverable`, `is-selected`, `is-inactive`, `is-disabled`, `is-open`, `is-expanded`, `is-draggable`, `is-dragging`, and `is-drop-target`.
- Keep color and state classes independent from component classes so the same color or state can be used by menus, buttons, tabs, toolbars, trees, docks, and status items.
- Prefer semantic style values on `Control` and related typed components that render to classes. Do not put raw CSS declarations, arbitrary colors, margins, gaps, or dimensions in ECS components.
- Use hover styling only for elements that are explicitly interactive or marked as hoverable. Selected and inactive styling must remain available even when hover is not present.
- Use `:focus-visible` and the shared focus treatment for keyboard interaction. Do not use hover as the only indication of an available action.
- Keep component CSS small. If a rule describes reusable geometry, add or extend a grid primitive instead of adding it to the component stylesheet.

## Unit Rules

- Use `2rem` as the core structural dimension. Prefer `2rem`, `4rem`, `6rem`, and other integer multiples for rows, columns, and fixed regions.
- Keep typography monospace so characters occupy a predictable horizontal rhythm within grid cells.
- Use the font rhythm to support alignment and scanning, but do not size structural regions by character count or assume text will always fit.
- Keep standard rows at one or more whole grid units. Let unrepresented horizontal space remain at the right edge of a row.
- Keep standard stacks at one or more whole grid units. Let unrepresented vertical space remain at the bottom of a stack.
- Allow content to overflow or grow by whole grid units when it cannot fit; do not compress the grid to remove remainder pixels.
- Use `grid-padded` for internal space around text, icons, and other content that needs breathing room. Padding must not create space between structural siblings.
- Use dividers only between components within one toolbar. Do not use them to divide whole toolbars; each toolbar begins with its grip icon.

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
