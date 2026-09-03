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

- Treat `Element` as the common UI component. It owns the parent/child hierarchy, grid placement intent, and semantic visual state for its entity.
- Keep UI component data and relationships in ECS components. Use `IRequires<Element>` for components that must be rendered as elements and use additional `IRequires<>` relationships to enforce required data.
- Use reusable `{Component}View.razor` files to render ECS components. Views should receive components as parameters, compose their markup from shared CSS classes, and avoid owning persistent UI state.
- Name reusable component Razor files with the `View` postfix, such as `BreadcrumbView.razor`, `MenuView.razor`, and `ToolbarView.razor`. Reserve other Razor names for application roots, pages, and layouts.
- Store persistent UI state on ECS components, not in views. Menu open state, selected tabs, expanded nodes, inactive state, and drag state belong to components or `Session`.
- Use `Session` for global interactions such as navigation, dragging, resizing, selection coordination, and context menus. Views render the resulting state.
- Reuse the same view for the same component in different contexts. Use role classes such as `menu-burger`, `menu-dropdown`, `menu-context`, `tabs-top`, and `tabs-subwindow` to describe placement without duplicating the component view.
- Keep hierarchy and ordering in `Element` and `ElementSystem`. Do not encode component relationships in CSS selectors or view-local collections.

## CSS Composition Rules

- Use one semantic base class only when a component needs a stable semantic hook or component-specific behavior. Do not repeat grid rules in base classes.
- Use `grid-*` classes for spatial composition: dimensions, alignment, padding, fill behavior, indentation, overflow, scrolling, and separators.
- Use `color-*` classes for semantic surfaces: `color-primary`, `color-secondary`, `color-success`, and `color-danger`.
- Use `is-*` classes for visual state: `is-hoverable`, `is-selected`, `is-inactive`, `is-disabled`, `is-open`, `is-expanded`, `is-draggable`, `is-dragging`, and `is-drop-target`.
- Keep color and state classes independent from component classes so the same color or state can be used by menus, buttons, tabs, toolbars, trees, docks, and status items.
- Prefer semantic style values on `Element` that render to classes. Do not put raw CSS declarations, arbitrary colors, margins, gaps, or dimensions in ECS components.
- Use hover styling only for elements that are explicitly interactive or marked as hoverable. Selected and inactive styling must remain available even when hover is not present.
- Use `:focus-visible` and the shared focus treatment for keyboard interaction. Do not use hover as the only indication of an available action.
- Keep component CSS small. If a rule describes reusable geometry, add or extend a grid primitive instead of adding it to the component stylesheet.

## Planned UI Elements

| ECS component | Reusable view | Grid composition | Role or state composition |
| --- | --- | --- | --- |
| `Screen` | `ScreenView.razor` | `grid-shell` | `layer-screen` |
| `ScreenTitleBar` | `ScreenTitleBarView.razor` | `grid-line`, `grid-center-vertically`, `grid-width-fill` | `color-primary` |
| `TowerIcon` | `TowerIconView.razor` | `grid-line`, `grid-center-vertically` | `icon-*` |
| `Breadcrumbs` | `BreadcrumbsView.razor` | `grid-row-layout`, `grid-center-vertically` | `breadcrumbs` |
| `Breadcrumb` | `BreadcrumbView.razor` | `grid-tab`, `grid-center-vertically` | `is-hoverable`, `is-selected`, `is-inactive` |
| `MenuComponent` | `MenuView.razor` | `grid-dock`, `grid-overflow-y` | `menu-burger`, `menu-dropdown`, `menu-context`, `layer-menu`, `color-*` |
| `MenuItemComponent` | `MenuItemView.razor` | `grid-line`, `grid-center-vertically`, `grid-width-fill` | `is-hoverable`, `is-selected`, `is-inactive`, `is-disabled` |
| `Toolbar` | `ToolbarView.razor` | `grid-dock`, `grid-width-fill` | `color-*`, `is-dragging`, `is-drop-target` |
| `ToolbarRow` | `ToolbarRowView.razor` | `grid-line`, `grid-center-vertically`, `grid-wrap-x` | `is-draggable` |
| `ToolbarGrip` | `ToolbarGripView.razor` | `grid-line`, `grid-center-vertically`, `grid-i-draggable` | `is-dragging` |
| `Button` | `ButtonView.razor` | `grid-line`, `grid-center-vertically`, `grid-padded` | `button-*`, `color-*`, `is-selected`, `is-disabled` |
| `Dropdown` | `DropdownView.razor` | `grid-line`, `grid-center-vertically` | `is-open`, `is-selected`, `is-disabled` |
| `DockLayout` | `DockLayoutView.razor` | `grid-dock-region`, `grid-width-fill`, `grid-height-fill` | `layer-screen` |
| `Dock` | `DockView.razor` | `grid-dock`, `grid-overflow-y` | `dock-left`, `dock-middle`, `dock-right`, `is-resizing` |
| `Subwindow` | `SubwindowView.razor` | `grid-dock`, `grid-height-fill` | `is-selected`, `is-inactive`, `is-collapsed` |
| `Tabs` | `TabsView.razor` | `grid-tabs`, `grid-overflow-x` | `tabs-top`, `tabs-subwindow` |
| `Tab` | `TabView.razor` | `grid-tab`, `grid-center-vertically`, `grid-padded` | `is-selected`, `is-inactive`, `is-disabled` |
| `TabContent` | `TabContentView.razor` | `grid-dock-content`, `grid-overflow-x`, `grid-overflow-y` | `is-selected` |
| `Tree` | `TreeView.razor` | `grid-dock-content`, `grid-overflow-y`, `grid-width-fill` | `grid-fill` |
| `TreeNode` | `TreeNodeView.razor` | `grid-width-fill` | `is-expanded`, `is-collapsed`, `is-selected`, `is-inactive` |
| `TreeNodeRow` | `TreeNodeRowView.razor` | `grid-line`, `grid-center-vertically`, `grid-width-fill` | `is-drop-target`, `--tree-depth` |
| `StatusBar` | `StatusBarView.razor` | `grid-status-bar`, `grid-overflow-x` | `layer-status`, `color-*` |
| `StatusItem` | `StatusItemView.razor` | `grid-center-vertically`, `grid-wrap-x` | `is-inactive` |
| `Text` | `TextView.razor` | `grid-wrap-x` | `text-*` |
| `Label` | `LabelView.razor` | `grid-center-vertically`, `grid-wrap-x` | `label-interactive`, `is-hoverable`, `is-inactive` |

The table is a component vocabulary, not a requirement that every element must have every listed class. Select only the classes that express the element's actual layout, role, and state.

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
