namespace SiegeTower.Client;

public static class NavigationSystem
{
	public static void HandleEvent(Session session, SessionEvent sessionEvent)
	{
		ArgumentNullException.ThrowIfNull(session);
		ArgumentNullException.ThrowIfNull(sessionEvent);

		if (sessionEvent is NavigationEvent navigationEvent)
		{
			HandleEvent_NavigationEvent(session, navigationEvent);
		}
	}

	static void HandleEvent_NavigationEvent(Session session, NavigationEvent navigationEvent)
	{
		ArgumentNullException.ThrowIfNull(session);
		ArgumentNullException.ThrowIfNull(navigationEvent);

		if (navigationEvent.IsCanceled || !navigationEvent.Hyperlink.IsInternal)
		{
			return;
		}

		NavigateTo(session, navigationEvent.Hyperlink.Uri);
	}

	public static void NavigateTo(Session session, string url)
	{
		ArgumentNullException.ThrowIfNull(session);
		ArgumentException.ThrowIfNullOrEmpty(url);

		var path = session.NavigationManager.ToAbsoluteUri(url).AbsolutePath.TrimEnd('/');
		var pathParts = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
		if (pathParts.Length == 0)
		{
			session.ActiveScreen = NewHomeScreen(session);
		}
		else if (pathParts.Length == 1 && pathParts[0].Equals("workspace", StringComparison.OrdinalIgnoreCase))
		{
			session.ActiveScreen = NewWorkspaceListScreen(session);
		}
		else if (pathParts.Length == 1 && pathParts[0].Equals("ollama", StringComparison.OrdinalIgnoreCase))
		{
			session.ActiveScreen = NewOllamaScreen(session);
		}
		else if (pathParts.Length >= 1 && pathParts[0].Equals("example", StringComparison.OrdinalIgnoreCase))
		{
			var mode = pathParts.Length == 2 && pathParts[1].Equals("files", StringComparison.OrdinalIgnoreCase)
				? ExampleScreenMode.Files
				: pathParts.Length == 2 && pathParts[1].Equals("sql", StringComparison.OrdinalIgnoreCase)
					? ExampleScreenMode.Sql
					: ExampleScreenMode.Home;
			session.ActiveScreen = NewExampleScreen(session, mode);
		}
		else if (pathParts.Length >= 2 && pathParts[0].Equals("workspace", StringComparison.OrdinalIgnoreCase))
		{
			var workspacePath = $"/workspace/{pathParts[1]}";
			var isFilesScreen = pathParts.Length == 3 && pathParts[2].Equals("files", StringComparison.OrdinalIgnoreCase);
			session.ActiveScreen = NewWorkspaceScreen(session, workspacePath, isFilesScreen);
		}
		else
		{
			session.ActiveScreen = NewFallbackScreen(session);
		}

		session.NavigationManager.NavigateTo(url);
	}

	static Screen NewHomeScreen(Session session)
	{
		var screen = new Screen(session, "Home");
		screen.AddNewBreadCrumbEntity("Home", "/", true, 0);
		return screen;
	}

	static Screen NewWorkspaceListScreen(Session session)
	{
		var screen = new Screen(session, "Workspaces");
		screen.AddNewBreadCrumbEntity("Home", "/", true, 0);
		screen.AddNewBreadCrumbEntity("Workspaces", "/workspace", true, 1);
		return screen;
	}

	static Screen NewOllamaScreen(Session session)
	{
		var screen = new Screen(session, "Ollama");
		screen.AddNewBreadCrumbEntity("Home", "/", true, 0);
		screen.AddNewBreadCrumbEntity("Ollama", "/ollama", true, 1);
		return screen;
	}

	static Screen NewExampleScreen(Session session, ExampleScreenMode mode)
	{
		var title = mode switch
		{
			ExampleScreenMode.Files => "Example Files",
			ExampleScreenMode.Sql => "Example SQL",
			_ => "Example"
		};
		var screen = new Screen(session, title);
		screen.AddNewBreadCrumbEntity("Home", "/", true, 0);
		screen.AddNewBreadCrumbEntity("Example", "/example", true, 1);
		screen.AddNewBreadCrumbEntity(mode switch
		{
			ExampleScreenMode.Files => "Files",
			ExampleScreenMode.Sql => "SQL",
			_ => "Home"
		}, mode switch
		{
			ExampleScreenMode.Files => "/example/files",
			ExampleScreenMode.Sql => "/example/sql",
			_ => "/example"
		}, true, 2);
	screen.NewEntity().AddComponent(e => new ExampleScreenComponent(e, mode));

		var titleBar = AddElement<ScreenTitleBar>(screen, "title-bar");
		var tower = AddElement<TowerIcon>(screen, "tower-icon");
		var breadcrumbs = AddElement<Breadcrumbs>(screen, "breadcrumbs");
		ElementSystem.Attach(titleBar, tower);
		ElementSystem.Attach(titleBar, breadcrumbs);

		var tabs = AddElement<Tabs>(screen, "tabs");
		ElementSystem.Attach(titleBar, tabs);
		AttachTab(screen, tabs, "Home", "/example", mode == ExampleScreenMode.Home);
		AttachTab(screen, tabs, "Files", "/example/files", mode == ExampleScreenMode.Files);
		AttachTab(screen, tabs, "SQL", "/example/sql", mode == ExampleScreenMode.Sql);

		var toolbars = AddElement<ToolbarRows>(screen, "toolbars");
		var primaryToolbar = AddToolbar(screen, "toolbar-1");
		ElementSystem.Attach(toolbars, primaryToolbar);
		if (mode == ExampleScreenMode.Sql)
		{
			var connection = AddElement<ToolbarDropdown>(screen, "sql-connection");
			var primaryRow = screen.SelectComponents<ToolbarRow>().Single(row => row.GetComponent<Element>().Id == "toolbar-1-row");
			ElementSystem.Attach(primaryRow, connection);
		}
		ElementSystem.Attach(toolbars, AddToolbar(screen, "toolbar-2"));
		ElementSystem.Attach(toolbars, AddToolbar(screen, "toolbar-3"));

		var dockLayout = AddElement<DockLayout>(screen, "dock-layout");
		var dockRow = AddElement<DockRow>(screen, "dock-row");
		var leftStack = AddElement<DockStack>(screen, "dock-left-stack");
		ElementSystem.Attach(dockLayout, dockRow);
		ElementSystem.Attach(dockRow, leftStack);

		var leftDock = AddDock(screen, leftStack, "left", "Files");
		var barDock = AddDock(screen, leftStack, "bar", "Outline");
		var middleDock = AddDock(screen, dockRow, "middle", "File");
		var rightDock = AddDock(screen, dockRow, "right", "Properties");

		var tree = AddElement<Tree>(screen, "file-tree");
		ElementSystem.Attach(leftDock, tree);
		AddFileNodes(screen, tree);

		var middleContent = AddElement<SubwindowContent>(screen, "file-content-window");
		var middleText = mode == ExampleScreenMode.Sql
			? "select *\nfrom users\nwhere active = true;"
			: mode == ExampleScreenMode.Files
				? "README.md\n\n# Files\n\nBrowse files in the workspace tree."
				: "README.md\n\n# SiegeTower\n\nExample file content aligned to the grid.";
		var fileContent = AddElement<Text>(screen, "middle-content", middleText);
		ElementSystem.Attach(middleDock, middleContent);
		ElementSystem.Attach(middleContent, fileContent);

		var properties = AddElement<Subwindow>(screen, "file-properties", "Properties");
		ElementSystem.Attach(rightDock, properties);
		ElementSystem.Attach(properties, AddElement<Label>(screen, "property-name", "Name: README.md"));
		ElementSystem.Attach(properties, AddElement<Label>(screen, "property-type", "Type: Markdown"));
		ElementSystem.Attach(properties, AddElement<Label>(screen, "property-size", "Size: 1.2 KB"));

		var outline = AddElement<Subwindow>(screen, "file-outline", "Outline");
		ElementSystem.Attach(barDock, outline);
		ElementSystem.Attach(outline, AddElement<Label>(screen, "outline-heading", "README.md"));
		ElementSystem.Attach(outline, AddElement<Label>(screen, "outline-section", "Files"));

		var statusBar = AddElement<StatusBar>(screen, "status-bar");
		ElementSystem.Attach(statusBar, AddElement<StatusItem>(screen, "status", "Ready | Example screen"));
		return screen;
	}

	static T AddElement<T>(Screen screen, string id) where T : Component, IRequires<Element>
	{
		var entity = screen.NewEntity();
		entity.AddComponent(e => new Element(e, id));
		return entity.AddComponent<T>();
	}

	static T AddElement<T>(Screen screen, string id, string value) where T : Component, IRequires<Element>
	{
		var entity = screen.NewEntity();
		entity.AddComponent(e => new Element(e, id));
		return entity.AddComponent(e => (T)Activator.CreateInstance(typeof(T), e, value)!);
	}

	static void AttachTab(Screen screen, Tabs tabs, string text, string uri, bool selected)
	{
		var entity = screen.NewEntity();
		entity.AddComponent(e => new Element(e, $"tab-{text.ToLowerInvariant()}"));
		entity.AddComponent(e => new Hyperlink(e, uri, true));
		var tab = entity.AddComponent(e => new Tab(e, text));
		if (selected)
		{
			tab.GetComponent<Element>().State |= ElementState.Selected;
		}
		ElementSystem.Attach(tabs, tab);
	}

	static Toolbar AddToolbar(Screen screen, string id)
	{
		var entity = screen.NewEntity();
		entity.AddComponent(e => new Element(e, id));
		var toolbar = entity.AddComponent<Toolbar>();
		var row = AddElement<ToolbarRow>(screen, $"{id}-row");
		ElementSystem.Attach(toolbar, row);
		ElementSystem.Attach(row, AddElement<ToolbarButton>(screen, $"{id}-button", "Action"));
		return toolbar;
	}

	static Dock AddDock(Screen screen, IRequires<Element> parent, string region, string title)
	{
		var entity = screen.NewEntity();
		entity.AddComponent(e => new Element(e, $"dock-{region}"));
		var dock = entity.AddComponent(e => new Dock(e, region, title));
		var subwindow = AddElement<Subwindow>(screen, $"subwindow-{region}", title);
		ElementSystem.Attach(dock, subwindow);
		ElementSystem.Attach(parent, dock);
		return dock;
	}

	static void AddFileNodes(Screen screen, Tree tree)
	{
		var root = AddTreeNode(screen, "node-root", "src");
		var client = AddTreeNode(screen, "node-client", "SiegeTower.Client");
		var file = AddTreeNode(screen, "node-file", "README.md");
		ElementSystem.Attach(tree, root);
		ElementSystem.Attach(root, client);
		ElementSystem.Attach(client, file);
		root.GetComponent<Element>().State |= ElementState.Expanded;
		client.GetComponent<Element>().State |= ElementState.Expanded;
		file.GetComponent<Element>().State |= ElementState.Selected;
	}

	static TreeNode AddTreeNode(Screen screen, string id, string text)
	{
		var node = AddElement<TreeNode>(screen, id, text);
		var row = AddElement<TreeNodeRow>(screen, $"{id}-row");
		var label = AddElement<TreeNodeLabel>(screen, $"{id}-label", text);
		ElementSystem.Attach(node, row);
		ElementSystem.Attach(row, label);
		return node;
	}

	static Screen NewWorkspaceScreen(Session session, string workspacePath, bool isFilesScreen)
	{
		var screen = new Screen(session, isFilesScreen ? "Workspace Files" : "Workspace");
		screen.AddNewBreadCrumbEntity("Home", "/", true, 0);
		screen.AddNewBreadCrumbEntity("Workspaces", "/workspace", true, 1);
		screen.AddNewBreadCrumbEntity("Workspace", workspacePath, true, 2);
		if (isFilesScreen)
		{
			screen.AddNewBreadCrumbEntity("Files", $"{workspacePath}/files", true, 3);
		}
		return screen;
	}

	static Screen NewFallbackScreen(Session session)
	{
		var screen = NewHomeScreen(session);
		var toolbarRows = screen.NewEntity().AddComponent<Element>().AddComponent<ToolbarRows>();
		var toolbarRow = screen.NewEntity().AddComponent<Element>().AddComponent<ToolbarRow>();
		var toolbar = screen.NewEntity().AddComponent<Element>().AddComponent<Toolbar>();

		ElementSystem.Attach(toolbarRows, toolbarRow);
		ElementSystem.Attach(toolbarRow, toolbar);
		return screen;
	}
}