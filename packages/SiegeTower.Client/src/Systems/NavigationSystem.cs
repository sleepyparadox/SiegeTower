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
		else if (pathParts.Length >= 1 && pathParts[0].Equals("example", StringComparison.OrdinalIgnoreCase))
		{
			session.ActiveScreen = pathParts.Length == 2 && pathParts[1].Equals("files", StringComparison.OrdinalIgnoreCase)
				? NewTypedExampleScreen(session, "Example Files", "README.md\n\n# Files\n\nBrowse files in the workspace tree.", "Ready | Example files screen")
				: pathParts.Length == 2 && pathParts[1].Equals("sql", StringComparison.OrdinalIgnoreCase)
					? NewTypedExampleScreen(session, "Example SQL", "select *\nfrom users\nwhere active = true;", "Ready | Example SQL screen")
					: NewTypedExampleScreen(session, "Example", "README.md\n\n# SiegeTower\n\nExample file content aligned to the grid.", "Ready | Example screen");
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
		var titleLayout = AddTitleLayout(screen, "Home");
		screen.AddNewBreadCrumbEntity(titleLayout, "Home", "/", true, 0);
		return screen;
	}

	static Screen NewWorkspaceListScreen(Session session)
	{
		var screen = new Screen(session, "Workspaces");
		var titleLayout = AddTitleLayout(screen, "Workspaces");
		screen.AddNewBreadCrumbEntity(titleLayout, "Home", "/", true, 0);
		screen.AddNewBreadCrumbEntity(titleLayout, "Workspaces", "/workspace", true, 1);
		return screen;
	}

	static Screen NewOllamaScreen(Session session)
	{
		var screen = new Screen(session, "Ollama");
		var titleLayout = AddTitleLayout(screen, "Ollama");
		screen.AddNewBreadCrumbEntity(titleLayout, "Home", "/", true, 0);
		screen.AddNewBreadCrumbEntity(titleLayout, "Ollama", "/ollama", true, 1);
		return screen;
	}

	static Screen NewTypedExampleScreen(Session session, string title, string documentContent, string status)
	{
		var screen = new Screen(session, title);
		var screenLayout = screen.NewEntity().AddComponent<ScreenLayout>();
		var titleLayout = AddTitleLayout(screen, title);
		var toolbarLayout = screen.NewEntity().AddComponent<ToolbarLayout>();
		var dockingLayout = screen.NewEntity().AddComponent<DockingLayout>();

		screen.AddNewBreadCrumbEntity(titleLayout, "Home", "/", true, 0);
		screen.AddNewBreadCrumbEntity(titleLayout, title, "/example", true, 1);
		ParentingSystem.AttachParentChild<ScreenLayout, ScreenLayoutChild>(screenLayout, toolbarLayout);
		ParentingSystem.AttachParentChild<ScreenLayout, ScreenLayoutChild>(screenLayout, dockingLayout);
		AddFileToolbar(screen, toolbarLayout);
		AddSearchToolbar(screen, toolbarLayout);
		AddWorkspaceToolbar(screen, toolbarLayout);

		var root = screen.NewEntity().AddComponent(entity => new DockContainer(entity, DockOrientation.Horizontal));
		ParentingSystem.AttachParentChild<DockingLayout, DockLayoutNode>(dockingLayout, root);

		var leftStack = screen.NewEntity().AddComponent(entity => new DockContainer(entity, DockOrientation.Vertical));
		var documentGroup = screen.NewEntity().AddComponent<DockWindowGroup>();
		var rightStack = screen.NewEntity().AddComponent(entity => new DockContainer(entity, DockOrientation.Vertical));
		rightStack.IsFixedWidth = true;
		rightStack.WidthInGridUnits = 10;
		ParentingSystem.AttachParentChild<DockContainer, DockLayoutNode>(root, leftStack);
		ParentingSystem.AttachParentChild<DockContainer, DockLayoutNode>(root, documentGroup);
		ParentingSystem.AttachParentChild<DockContainer, DockLayoutNode>(root, rightStack);

		var filesGroup = screen.NewEntity().AddComponent<DockWindowGroup>();
		var outlineGroup = screen.NewEntity().AddComponent<DockWindowGroup>();
		ParentingSystem.AttachParentChild<DockContainer, DockLayoutNode>(leftStack, filesGroup);
		ParentingSystem.AttachParentChild<DockContainer, DockLayoutNode>(leftStack, outlineGroup);

		var filesWindow = AddDockWindow(screen, filesGroup, "Files", "src\n  App\n    Program.cs\n    SQL\n      Query.sql\n  Dist\nREADME.md");
		AddFileTreeControlLayout(screen, filesWindow);
		AddDockWindow(screen, outlineGroup, "Outline", "README.md\nFiles");
		AddDockWindow(screen, documentGroup, title, documentContent);
		var propertiesWindow = AddDockWindow(screen, rightStack, "Properties", "Name: README.md\nType: Markdown\nStatus: " + status);
		AddPropertiesControlLayout(screen, propertiesWindow, status);
		return screen;
	}

	static TitleLayout AddTitleLayout(Screen screen, string title)
	{
		var screenLayout = screen.SelectComponents<ScreenLayout>().SingleOrDefault();
		if (screenLayout is null)
		{
			screenLayout = screen.NewEntity().AddComponent<ScreenLayout>();
		}

		var titleLayout = screen.NewEntity().AddComponent(entity => new TitleLayout(entity, title));
		ParentingSystem.AttachParentChild<ScreenLayout, ScreenLayoutChild>(screenLayout, titleLayout);
		return titleLayout;
	}

	static DockWindow AddDockWindow(Screen screen, DockWindowGroup group, string title, string content)
	{
		var window = screen.NewEntity().AddComponent(e => new DockWindow(e, title, content));
		group.AttachChild(window);
		group.ActiveWindow = window;
		return window;
	}

	static DockWindow AddDockWindow(Screen screen, DockContainer container, string title, string content)
	{
		var group = screen.NewEntity().AddComponent<DockWindowGroup>();
		ParentingSystem.AttachParentChild<DockContainer, DockLayoutNode>(container, group);
		return AddDockWindow(screen, group, title, content);
	}

	static void AddPropertiesControlLayout(Screen screen, DockWindow window, string status)
	{
		var layout = window.AddComponent<ControlLayout>();
		window.AddComponent<DockWindowControlLayout>();
		var root = screen.NewEntity().AddComponent(entity => new ControlLayoutNode(entity, ControlLayoutOrientation.Stack));
		ParentingSystem.AttachParentChild<ControlLayout, ControlLayoutNode>(layout, root);

		var gridEntity = screen.NewEntity();
		var gridPlacement = gridEntity.AddComponent<ControlLayoutControl>();
		var grid = gridEntity.AddComponent(entity => new GridControl(entity, 2));
		ParentingSystem.AttachParentChild<ControlLayoutNode, ControlLayoutControl>(root, gridPlacement);

		AddGridCellControl(screen, grid, 0, 0, entity => new LabelControl(entity, "Name"));
		AddGridCellControl(screen, grid, 0, 1, entity => new TextInputControl(entity, "README.md"));
		AddGridCellControl(screen, grid, 1, 0, entity => new LabelControl(entity, "Status"));
		AddGridCellControl(screen, grid, 1, 1, entity => new LabelControl(entity, status));
	}

	static TControl AddGridCellControl<TControl>(Screen screen, GridControl grid, int row, int column, Func<Entity, TControl> createControl)
		where TControl : Component
	{
		var entity = screen.NewEntity();
		var cell = entity.AddComponent(entity => new GridCellControl(entity, row, column));
		var control = entity.AddComponent(createControl);
		ParentingSystem.AttachParentChild<GridControl, GridCellControl>(grid, cell);
		return control;
	}

	static void AddFileTreeControlLayout(Screen screen, DockWindow window)
	{
		var layout = window.AddComponent<ControlLayout>();
		window.AddComponent<DockWindowControlLayout>();
		var root = screen.NewEntity().AddComponent(entity => new ControlLayoutNode(entity, ControlLayoutOrientation.Stack));
		ParentingSystem.AttachParentChild<ControlLayout, ControlLayoutNode>(layout, root);

		var treeEntity = screen.NewEntity();
		var treePlacement = treeEntity.AddComponent<ControlLayoutControl>();
		var tree = treeEntity.AddComponent<TreeControl>();
		ParentingSystem.AttachParentChild<ControlLayoutNode, ControlLayoutControl>(root, treePlacement);

		var src = AddTreeNode(screen, "src", TreeNodeIcon.Folder);
		var app = AddTreeNode(screen, "App", TreeNodeIcon.Folder);
		var program = AddTreeNode(screen, "Program.cs");
		var sql = AddTreeNode(screen, "SQL", TreeNodeIcon.Folder);
		var query = AddTreeNode(screen, "Query.sql");
		var dist = AddTreeNode(screen, "Dist", TreeNodeIcon.Folder);
		var output = AddTreeNode(screen, "output.txt");
		var readme = AddTreeNode(screen, "README.md");

		ParentingSystem.AttachParentChild<TreeControl, TreeNode>(tree, src);
		ParentingSystem.AttachParentChild<TreeNode, TreeNode>(src, app);
		ParentingSystem.AttachParentChild<TreeNode, TreeNode>(src, dist);
		ParentingSystem.AttachParentChild<TreeNode, TreeNode>(src, readme);
		ParentingSystem.AttachParentChild<TreeNode, TreeNode>(app, program);
		ParentingSystem.AttachParentChild<TreeNode, TreeNode>(app, sql);
		ParentingSystem.AttachParentChild<TreeNode, TreeNode>(sql, query);
		ParentingSystem.AttachParentChild<TreeNode, TreeNode>(dist, output);

		src.IsExpanded = true;
		app.IsExpanded = true;
		sql.IsExpanded = true;
		dist.IsExpanded = true;
		readme.IsSelected = true;
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

	static Toolbar AddFileToolbar(Screen screen, ToolbarLayout layout)
	{
		var toolbar = AddToolbar(screen, layout, 0);
		AddToolbarControl<ButtonControl>(screen, toolbar, entity => new ButtonControl(entity, "New"));
		AddToolbarControl<ButtonControl>(screen, toolbar, entity => new ButtonControl(entity, "Open"));
		AddToolbarControl<SeparatorControl>(screen, toolbar, entity => new SeparatorControl(entity));
		AddToolbarControl<ButtonControl>(screen, toolbar, entity => new ButtonControl(entity, "Save"));
		return toolbar;
	}

	static Toolbar AddSearchToolbar(Screen screen, ToolbarLayout layout)
	{
		var toolbar = AddToolbar(screen, layout, 0);
		AddToolbarControl<LabelControl>(screen, toolbar, entity => new LabelControl(entity, "Find"));
		AddToolbarControl<TextInputControl>(screen, toolbar, entity => new TextInputControl(entity, "Query.sql"));
		AddToolbarControl<SeparatorControl>(screen, toolbar, entity => new SeparatorControl(entity));
		AddToolbarControl<ComboBoxControl>(screen, toolbar, entity => new ComboBoxControl(entity, "Current file"));
		return toolbar;
	}

	static Toolbar AddWorkspaceToolbar(Screen screen, ToolbarLayout layout)
	{
		var toolbar = AddToolbar(screen, layout, 1);
		AddToolbarControl<LabelControl>(screen, toolbar, entity => new LabelControl(entity, "Workspace"));
		AddToolbarControl<ComboBoxControl>(screen, toolbar, entity => new ComboBoxControl(entity, "Development"));
		AddToolbarControl<SeparatorControl>(screen, toolbar, entity => new SeparatorControl(entity));
		AddToolbarControl<ButtonControl>(screen, toolbar, entity => new ButtonControl(entity, "Run"));
		return toolbar;
	}

	static Toolbar AddToolbar(Screen screen, ToolbarLayout layout, int rowIndex)
	{
		var toolbar = screen.NewEntity().AddComponent(entity => new Toolbar(entity, rowIndex));
		ParentingSystem.AttachParentChild<ToolbarLayout, Toolbar>(layout, toolbar);
		return toolbar;
	}

	static TControl AddToolbarControl<TControl>(Screen screen, Toolbar toolbar, Func<Entity, TControl> createControl)
		where TControl : Component
	{
		var entity = screen.NewEntity();
		var toolbarControl = entity.AddComponent<ToolbarControl>();
		var control = entity.AddComponent(createControl);
		ParentingSystem.AttachParentChild<Toolbar, ToolbarControl>(toolbar, toolbarControl);
		return control;
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

	static TreeNode AddTreeNode(Screen screen, string text, TreeNodeIcon icon = TreeNodeIcon.File)
	{
		var entity = screen.NewEntity();
		return entity.AddComponent(e => new TreeNode(e, text, icon));
	}

	static Screen NewWorkspaceScreen(Session session, string workspacePath, bool isFilesScreen)
	{
		var screen = new Screen(session, isFilesScreen ? "Workspace Files" : "Workspace");
		var titleLayout = AddTitleLayout(screen, screen.Title);
		screen.AddNewBreadCrumbEntity(titleLayout, "Home", "/", true, 0);
		screen.AddNewBreadCrumbEntity(titleLayout, "Workspaces", "/workspace", true, 1);
		screen.AddNewBreadCrumbEntity(titleLayout, "Workspace", workspacePath, true, 2);
		if (isFilesScreen)
		{
			screen.AddNewBreadCrumbEntity(titleLayout, "Files", $"{workspacePath}/files", true, 3);
		}
		return screen;
	}

	static Screen NewFallbackScreen(Session session)
		=> NewHomeScreen(session);
}