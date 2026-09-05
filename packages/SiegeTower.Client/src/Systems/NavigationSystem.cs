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

		if (navigationEvent.IsCanceled || navigationEvent.Hyperlink is not null && !navigationEvent.Hyperlink.IsInternal)
		{
			return;
		}

		NavigateTo(session, navigationEvent.Uri);
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
		else if (pathParts.Length >= 1 && (pathParts[0].Equals("example", StringComparison.OrdinalIgnoreCase) || pathParts[0].Equals("example-old", StringComparison.OrdinalIgnoreCase)))
		{
			var legacyScreenRenderer = pathParts[0].Equals("example-old", StringComparison.OrdinalIgnoreCase);
			session.ActiveScreen = pathParts.Length == 2 && pathParts[1].Equals("files", StringComparison.OrdinalIgnoreCase)
				? NewExampleFilesScreen(session, legacyScreenRenderer)
				: pathParts.Length == 2 && pathParts[1].Equals("sql", StringComparison.OrdinalIgnoreCase)
					? NewExampleSqlScreen(session, legacyScreenRenderer)
					: NewExampleScreen(session, legacyScreenRenderer);
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

	static Screen NewExampleScreen(Session session, bool legacyScreenRenderer = true)
	{
		var routePrefix = legacyScreenRenderer ? "/example-old" : "/example";
		var screen = new Screen(session, "Example", legacyScreenRenderer);
		screen.AddNewBreadCrumbEntity("Home", "/", true, 0);
		screen.AddNewBreadCrumbEntity("Example", routePrefix, true, 1);
		screen.AddNewBreadCrumbEntity("Home", routePrefix, true, 2);

		AddElement<ScreenTitleBar>(screen, "title-bar").WithChildren(titleBar =>
		{
			var tabs = AddElement<Tabs>(screen, "tabs");
			var breadcrumbs = AddElement<Breadcrumbs>(screen, "breadcrumbs");
			breadcrumbs.WithChildren(screen.SelectComponents<BreadCrumb>().OrderBy(breadcrumb => breadcrumb.Index).ToArray());
			titleBar.WithChildren(AddElement<TowerIcon>(screen, "tower-icon"), breadcrumbs, tabs);
			AttachTab(screen, tabs, "Home", routePrefix, true);
			AttachTab(screen, tabs, "Files", $"{routePrefix}/files", false);
			AttachTab(screen, tabs, "SQL", $"{routePrefix}/sql", false);
		});

		var toolbars = AddElement<ToolbarRows>(screen, "toolbars");
		var primaryToolbar = AddToolbar(screen, "toolbar-1");
		toolbars.WithChildren(primaryToolbar, AddToolbar(screen, "toolbar-2"), AddToolbar(screen, "toolbar-3"));

		var dockLayout = AddElement<DockLayout>(screen, "dock-layout");
		dockLayout.WithChildren(layout =>
		{
			layout.WithChildren(
				AddElement<DockRow>(screen, "dock-row").WithChildren(
					AddElement<DockStack>(screen, "dock-left-stack").WithChildren(
						AddDock(screen, "left", "Files").WithChildren(AddFileNodes(screen)),
						AddDock(screen, "bar", "Outline").WithChildren(
							AddElement<Subwindow>(screen, "file-outline", "Outline").WithChildren(
								AddElement<Label>(screen, "outline-heading", "README.md"),
								AddElement<Label>(screen, "outline-section", "Files")))),
					AddDock(screen, "middle", "File").WithChildren(
						AddElement<SubwindowContent>(screen, "file-content-window").WithChildren(
							AddElement<Text>(screen, "middle-content", "README.md\n\n# SiegeTower\n\nExample file content aligned to the grid."))),
					AddDock(screen, "right", "Properties").WithChildren(
						AddElement<Subwindow>(screen, "file-properties", "Properties").WithChildren(
							AddElement<Label>(screen, "property-name", "Name: README.md"),
							AddElement<Label>(screen, "property-type", "Type: Markdown"),
							AddElement<Label>(screen, "property-size", "Size: 1.2 KB")))));
		});

		var statusBar = AddElement<StatusBar>(screen, "status-bar");
		statusBar.WithChildren(AddElement<StatusItem>(screen, "status", "Ready | Example screen"));
		return screen;
	}

	static Screen NewExampleFilesScreen(Session session, bool legacyScreenRenderer = true)
	{
		var routePrefix = legacyScreenRenderer ? "/example-old" : "/example";
		var screen = new Screen(session, "Example Files", legacyScreenRenderer);
		screen.AddNewBreadCrumbEntity("Home", "/", true, 0);
		screen.AddNewBreadCrumbEntity("Example", routePrefix, true, 1);
		screen.AddNewBreadCrumbEntity("Files", $"{routePrefix}/files", true, 2);

		AddElement<ScreenTitleBar>(screen, "title-bar").WithChildren(titleBar =>
		{
			var tabs = AddElement<Tabs>(screen, "tabs");
			var breadcrumbs = AddElement<Breadcrumbs>(screen, "breadcrumbs");
			breadcrumbs.WithChildren(screen.SelectComponents<BreadCrumb>().OrderBy(breadcrumb => breadcrumb.Index).ToArray());
			titleBar.WithChildren(AddElement<TowerIcon>(screen, "tower-icon"), breadcrumbs, tabs);
			AttachTab(screen, tabs, "Home", routePrefix, false);
			AttachTab(screen, tabs, "Files", $"{routePrefix}/files", true);
			AttachTab(screen, tabs, "SQL", $"{routePrefix}/sql", false);
		});

		var toolbars = AddElement<ToolbarRows>(screen, "toolbars");
		toolbars.WithChildren(AddToolbar(screen, "toolbar-1"), AddToolbar(screen, "toolbar-2"), AddToolbar(screen, "toolbar-3"));

		var dockLayout = AddElement<DockLayout>(screen, "dock-layout");
		dockLayout.WithChildren(layout =>
		{
			layout.WithChildren(
				AddElement<DockRow>(screen, "dock-row").WithChildren(
					AddElement<DockStack>(screen, "dock-left-stack").WithChildren(
						AddDock(screen, "left", "Files").WithChildren(AddFileNodes(screen)),
						AddDock(screen, "bar", "Outline").WithChildren(
							AddElement<Subwindow>(screen, "file-outline", "Outline").WithChildren(
								AddElement<Label>(screen, "outline-heading", "README.md"),
								AddElement<Label>(screen, "outline-section", "Files")))),
					AddDock(screen, "middle", "File").WithChildren(
						AddElement<SubwindowContent>(screen, "file-content-window").WithChildren(
							AddElement<Text>(screen, "middle-content", "README.md\n\n# Files\n\nBrowse files in the workspace tree."))),
					AddDock(screen, "right", "Properties").WithChildren(
						AddElement<Subwindow>(screen, "file-properties", "Properties").WithChildren(
							AddElement<Label>(screen, "property-name", "Name: README.md"),
							AddElement<Label>(screen, "property-type", "Type: Markdown"),
							AddElement<Label>(screen, "property-size", "Size: 1.2 KB")))));
		});

		var statusBar = AddElement<StatusBar>(screen, "status-bar");
		statusBar.WithChildren(AddElement<StatusItem>(screen, "status", "Ready | Example files screen"));
		return screen;
	}

	static Screen NewExampleSqlScreen(Session session, bool legacyScreenRenderer = true)
	{
		var routePrefix = legacyScreenRenderer ? "/example-old" : "/example";
		var screen = new Screen(session, "Example SQL", legacyScreenRenderer);
		screen.AddNewBreadCrumbEntity("Home", "/", true, 0);
		screen.AddNewBreadCrumbEntity("Example", routePrefix, true, 1);
		screen.AddNewBreadCrumbEntity("SQL", $"{routePrefix}/sql", true, 2);

		AddElement<ScreenTitleBar>(screen, "title-bar").WithChildren(titleBar =>
		{
			var tabs = AddElement<Tabs>(screen, "tabs");
			var breadcrumbs = AddElement<Breadcrumbs>(screen, "breadcrumbs");
			breadcrumbs.WithChildren(screen.SelectComponents<BreadCrumb>().OrderBy(breadcrumb => breadcrumb.Index).ToArray());
			titleBar.WithChildren(AddElement<TowerIcon>(screen, "tower-icon"), breadcrumbs, tabs);
			AttachTab(screen, tabs, "Home", routePrefix, false);
			AttachTab(screen, tabs, "Files", $"{routePrefix}/files", false);
			AttachTab(screen, tabs, "SQL", $"{routePrefix}/sql", true);
		});

		var toolbars = AddElement<ToolbarRows>(screen, "toolbars");
		var primaryToolbar = AddToolbar(screen, "toolbar-1");
		var connection = AddElement<ToolbarDropdown>(screen, "sql-connection");
		var primaryRow = screen.SelectComponents<ToolbarRow>().Single(row => row.GetComponent<Element>().Id == "toolbar-1-row");
		toolbars.WithChildren(primaryToolbar, AddToolbar(screen, "toolbar-2"), AddToolbar(screen, "toolbar-3"));
		primaryRow.WithChildren(connection);

		var dockLayout = AddElement<DockLayout>(screen, "dock-layout");
		dockLayout.WithChildren(layout =>
		{
			layout.WithChildren(
				AddElement<DockRow>(screen, "dock-row").WithChildren(
					AddElement<DockStack>(screen, "dock-left-stack").WithChildren(
						AddDock(screen, "left", "Files").WithChildren(AddFileNodes(screen)),
						AddDock(screen, "bar", "Outline").WithChildren(
							AddElement<Subwindow>(screen, "file-outline", "Outline").WithChildren(
								AddElement<Label>(screen, "outline-heading", "Query.sql"),
								AddElement<Label>(screen, "outline-section", "Query")))),
					AddDock(screen, "middle", "File").WithChildren(
						AddElement<SubwindowContent>(screen, "file-content-window").WithChildren(
							AddElement<Text>(screen, "middle-content", "select *\nfrom users\nwhere active = true;"))),
					AddDock(screen, "right", "Properties").WithChildren(
						AddElement<Subwindow>(screen, "file-properties", "Properties").WithChildren(
							AddElement<Label>(screen, "property-name", "Name: Query.sql"),
							AddElement<Label>(screen, "property-type", "Type: SQL"),
							AddElement<Label>(screen, "property-size", "Size: 0.1 KB")))));
		});

		var statusBar = AddElement<StatusBar>(screen, "status-bar");
		statusBar.WithChildren(AddElement<StatusItem>(screen, "status", "Ready | Example SQL screen"));
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
		tabs.WithChildren(tab);
	}

	static Toolbar AddToolbar(Screen screen, string id)
	{
		var entity = screen.NewEntity();
		entity.AddComponent(e => new Element(e, id));
		var toolbar = entity.AddComponent<Toolbar>();
		var row = AddElement<ToolbarRow>(screen, $"{id}-row");
		toolbar.WithChildren(row);
		row.WithChildren(AddElement<ToolbarButton>(screen, $"{id}-button", "Action"));
		return toolbar;
	}

	static Dock AddDock(Screen screen, string region, string title)
	{
		var entity = screen.NewEntity();
		entity.AddComponent(e => new Element(e, $"dock-{region}"));
		var dock = entity.AddComponent(e => new Dock(e, region, title));
		var subwindow = AddElement<Subwindow>(screen, $"subwindow-{region}", title);
		dock.WithChildren(subwindow);
		return dock;
	}

	static Tree AddFileNodes(Screen screen)
	{
		var tree = AddElement<Tree>(screen, "file-tree");
		var src = AddTreeNode(screen, "node-src", "src", TreeNodeIcon.Folder);
		var app = AddTreeNode(screen, "node-app", "App", TreeNodeIcon.Folder);
		var program = AddTreeNode(screen, "node-program", "Program.cs");
		var sql = AddTreeNode(screen, "node-sql", "SQL", TreeNodeIcon.Folder);
		var query = AddTreeNode(screen, "node-query", "Query.sql");
		var dist = AddTreeNode(screen, "node-dist", "Dist", TreeNodeIcon.Folder);
		var output = AddTreeNode(screen, "node-output", "output.txt");
		var readme = AddTreeNode(screen, "node-readme", "README.md");

		tree.WithChildren(src);
		src.WithChildren(app, dist, readme);
		app.WithChildren(program, sql);
		sql.WithChildren(query);
		dist.WithChildren(output);

		src.IsExpanded = true;
		app.IsExpanded = true;
		sql.IsExpanded = true;
		dist.IsExpanded = true;
		readme.IsSelected = true;
		return tree;
	}

	static TreeNode AddTreeNode(Screen screen, string id, string text, TreeNodeIcon icon = TreeNodeIcon.File)
	{
		var entity = screen.NewEntity();
		entity.AddComponent(e => new Element(e, id));
		return entity.AddComponent(e => new TreeNode(e, text, icon));
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

		toolbarRows.WithChildren(toolbarRow);
		toolbarRow.WithChildren(toolbar);
		return screen;
	}
}