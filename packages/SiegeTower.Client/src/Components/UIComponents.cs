[Flags]
public enum ElementState
{
	None = 0,
	Hoverable = 1,
	Selected = 2,
	Inactive = 4,
	Disabled = 8,
	Open = 16,
	Expanded = 32,
	Draggable = 64,
	Dragging = 128,
	DropTarget = 256,
	Resizing = 512
}

public enum ElementColor
{
	None,
	Primary,
	Secondary,
	Success,
	Danger
}

public enum GridAlignment
{
	Default,
	Start,
	Center,
	End
}

public enum ExampleScreenMode
{
	Home,
	Files,
	Sql
}

public class ExampleScreenComponent : Component
{
	public ExampleScreenMode Mode { get; }

	public ExampleScreenComponent(Entity entity, ExampleScreenMode mode = ExampleScreenMode.Home) : base(entity)
	{
		Mode = mode;
	}
}

public class ScreenTitleBar : Component, IRequires<Element>
{
	public ScreenTitleBar(Entity entity) : base(entity) { }
}

public class TowerIcon : Component, IRequires<Element>
{
	public string Icon { get; set; }

	public TowerIcon(Entity entity) : this(entity, "fa-solid fa-tower-observation") { }

	public TowerIcon(Entity entity, string icon = "fa-solid fa-tower-observation") : base(entity)
	{
		Icon = icon;
	}
}

public class Breadcrumbs : Component, IRequires<Element>
{
	public Breadcrumbs(Entity entity) : base(entity) { }
}

public class ToolbarLabel : Component, IRequires<Element>
{
	public string Text { get; set; }

	public ToolbarLabel(Entity entity, string text) : base(entity)
	{
		Text = text;
	}
}

public class ToolbarButton : Component, IRequires<Element>
{
	public string Text { get; set; }

	public ToolbarButton(Entity entity, string text) : base(entity)
	{
		Text = text;
	}
}

public class ToolbarDropdown : Component, IRequires<Element>
{
	public ToolbarDropdown(Entity entity) : base(entity) { }
}

public class ToolbarSeparator : Component, IRequires<Element>
{
	public ToolbarSeparator(Entity entity) : base(entity) { }
}

public class Button : Component, IRequires<Element>
{
	public string Text { get; set; }

	public Button(Entity entity, string text) : base(entity)
	{
		Text = text;
	}
}

public class Dropdown : Component, IRequires<Element>
{
	public bool IsOpen { get; set; }

	public Dropdown(Entity entity) : base(entity) { }

	public void Toggle() => IsOpen = !IsOpen;
}

public class DockLayout : Component, IRequires<Element>
{
	public DockLayout(Entity entity) : base(entity) { }
}

public class DockRow : Component, IRequires<Element>
{
	public DockRow(Entity entity) : base(entity) { }
}

public class DockStack : Component, IRequires<Element>
{
	public DockStack(Entity entity) : base(entity) { }
}

public class Dock : Component, IRequires<Element>
{
	public string Region { get; set; }
	public string Title { get; set; }

	public Dock(Entity entity, string region, string title = "Dock") : base(entity)
	{
		Region = region;
		Title = title;
	}
}

public class DockResizeHandle : Component, IRequires<Element>
{
	public DockResizeHandle(Entity entity) : base(entity) { }
}

public class Subwindow : Component, IRequires<Element>
{
	public string Title { get; set; }

	public Subwindow(Entity entity, string title) : base(entity)
	{
		Title = title;
	}
}

public class SubwindowTitleBar : Component, IRequires<Element>
{
	public SubwindowTitleBar(Entity entity) : base(entity) { }
}

public class SubwindowContent : Component, IRequires<Element>
{
	public SubwindowContent(Entity entity) : base(entity) { }
}

public class SubwindowStatus : Component, IRequires<Element>
{
	public SubwindowStatus(Entity entity) : base(entity) { }
}

public class Tabs : Component, IRequires<Element>
{
	public Tabs(Entity entity) : base(entity) { }
}

public class Tab : Component, IRequires<Element>, IRequires<Hyperlink>
{
	public string Text { get; set; }

	public Tab(Entity entity, string text) : base(entity)
	{
		Text = text;
	}
}

public class TabContent : Component, IRequires<Element>
{
	public TabContent(Entity entity) : base(entity) { }
}

public class Tree : Component, IRequires<Element>
{
	public Tree(Entity entity) : base(entity) { }
}

public class TreeNode : Component, IRequires<Element>
{
	public string Text { get; set; }

	public TreeNode(Entity entity, string text) : base(entity)
	{
		Text = text;
	}

	public void Toggle(Element element)
	{
		element.State = element.State.HasFlag(ElementState.Expanded)
			? element.State & ~ElementState.Expanded
			: element.State | ElementState.Expanded;
	}
}

public class TreeNodeRow : Component, IRequires<Element>
{
	public TreeNodeRow(Entity entity) : base(entity) { }
}

public class StatusBar : Component, IRequires<Element>
{
	public StatusBar(Entity entity) : base(entity) { }
}

public class StatusItem : Component, IRequires<Element>
{
	public string Text { get; set; }

	public StatusItem(Entity entity, string text) : base(entity)
	{
		Text = text;
	}
}

public class Text : Component, IRequires<Element>
{
	public string Value { get; set; }

	public Text(Entity entity, string value) : base(entity)
	{
		Value = value;
	}
}

public class Label : Component, IRequires<Element>
{
	public string Value { get; set; }

	public Label(Entity entity, string value) : base(entity)
	{
		Value = value;
	}
}

public class MenuItemIcon : Component, IRequires<Element>
{
	public string Icon { get; set; }

	public MenuItemIcon(Entity entity, string icon) : base(entity)
	{
		Icon = icon;
	}
}

public class MenuItemLabel : Component, IRequires<Element>
{
	public string Text { get; set; }

	public MenuItemLabel(Entity entity, string text) : base(entity)
	{
		Text = text;
	}
}

public class MenuItemArrow : Component, IRequires<Element>
{
	public MenuItemArrow(Entity entity) : base(entity) { }
}

public class TabIcon : Component, IRequires<Element>
{
	public string Icon { get; set; }

	public TabIcon(Entity entity, string icon) : base(entity)
	{
		Icon = icon;
	}
}

public class TabLabel : Component, IRequires<Element>
{
	public string Text { get; set; }

	public TabLabel(Entity entity, string text) : base(entity)
	{
		Text = text;
	}
}

public class TabClose : Component, IRequires<Element>
{
	public TabClose(Entity entity) : base(entity) { }
}

public class TreeNodeToggle : Component, IRequires<Element>
{
	public TreeNodeToggle(Entity entity) : base(entity) { }
}

public class TreeNodeIcon : Component, IRequires<Element>
{
	public string Icon { get; set; }

	public TreeNodeIcon(Entity entity, string icon) : base(entity)
	{
		Icon = icon;
	}
}

public class TreeNodeLabel : Component, IRequires<Element>
{
	public string Text { get; set; }

	public TreeNodeLabel(Entity entity, string text) : base(entity)
	{
		Text = text;
	}
}

public class TreeNodeChildren : Component, IRequires<Element>
{
	public TreeNodeChildren(Entity entity) : base(entity) { }
}

public class Icon : Component, IRequires<Element>
{
	public string Class { get; set; }

	public Icon(Entity entity, string @class) : base(entity)
	{
		Class = @class;
	}
}

public class Separator : Component, IRequires<Element>
{
	public Separator(Entity entity) : base(entity) { }
}