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

		if (navigationEvent.IsCanceled)
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
		screen.AddNewBreadCrumbEntity(titleLayout, "Home", "/", 0);
		return screen;
	}

	static Screen NewWorkspaceListScreen(Session session)
	{
		var screen = new Screen(session, "Workspaces");
		var titleLayout = AddTitleLayout(screen, "Workspaces");
		screen.AddNewBreadCrumbEntity(titleLayout, "Home", "/", 0);
		screen.AddNewBreadCrumbEntity(titleLayout, "Workspaces", "/workspace", 1);
		return screen;
	}

	static Screen NewOllamaScreen(Session session)
	{
		var screen = new Screen(session, "Ollama");
		var titleLayout = AddTitleLayout(screen, "Ollama");
		screen.AddNewBreadCrumbEntity(titleLayout, "Home", "/", 0);
		screen.AddNewBreadCrumbEntity(titleLayout, "Ollama", "/ollama", 1);
		return screen;
	}

	static Screen NewTypedExampleScreen(Session session, string title, string documentContent, string status)
	{
		var screen = new Screen(session, title);
		var screenLayout = screen.NewEntity<ScreenLayout>();
		var titleLayout = AddTitleLayout(screen, title);

		screen.AddNewBreadCrumbEntity(titleLayout, "Home", "/", 0);
		screen.AddNewBreadCrumbEntity(titleLayout, title, "/example", 1);
		var toolbarLayout = screen.NewEntity<ToolbarLayout>().AttachChildren<ToolbarLayout, Toolbar>(layout => [
			layout.AddToolbar(0).AttachChildren<Toolbar, ToolbarControl>(toolbar => [
				toolbar.AddToolbarControl<ButtonControl>(entity => new ButtonControl(entity, "New")),
				toolbar.AddToolbarControl<ButtonControl>(entity => new ButtonControl(entity, "Open")),
				toolbar.AddToolbarControl<SeparatorControl>(entity => new SeparatorControl(entity)),
				toolbar.AddToolbarControl<ButtonControl>(entity => new ButtonControl(entity, "Save"))
			]),
			layout.AddToolbar(0).AttachChildren<Toolbar, ToolbarControl>(toolbar => [
				toolbar.AddToolbarControl<LabelControl>(entity => new LabelControl(entity, "Find")),
				toolbar.AddToolbarControl<TextInputControl>(entity => new TextInputControl(entity, "Query.sql")),
				toolbar.AddToolbarControl<SeparatorControl>(entity => new SeparatorControl(entity)),
				toolbar.AddToolbarControl<ComboBoxControl>(entity => new ComboBoxControl(entity, "Current file"))
			]),
			layout.AddToolbar(1).AttachChildren<Toolbar, ToolbarControl>(toolbar => [
				toolbar.AddToolbarControl<LabelControl>(entity => new LabelControl(entity, "Workspace")),
				toolbar.AddToolbarControl<ComboBoxControl>(entity => new ComboBoxControl(entity, "Development")),
				toolbar.AddToolbarControl<SeparatorControl>(entity => new SeparatorControl(entity)),
				toolbar.AddToolbarControl<ButtonControl>(entity => new ButtonControl(entity, "Run"))
			])
		]);

		var dockingLayout = screen.NewEntity<DockingLayout>();
		screenLayout.AttachChildren<ScreenLayout, ScreenLayoutChild>([toolbarLayout, dockingLayout]);

		var root = screen.NewEntity<DockContainer>(entity => new DockContainer(entity, DockOrientation.Horizontal));
		dockingLayout.AttachChild<DockingLayout, DockLayoutNode>(root);

		var leftStack = screen.NewEntity<DockContainer>(entity => new DockContainer(entity, DockOrientation.Vertical));
		var documentGroup = screen.NewEntity<DockWindowGroup>();
		var rightStack = screen.NewEntity<DockContainer>(entity => new DockContainer(entity, DockOrientation.Vertical));
		rightStack.IsFixedWidth = true;
		rightStack.WidthInGridUnits = 10;
		root.AttachChildren<DockContainer, DockLayoutNode>([leftStack, documentGroup, rightStack]);

		var filesGroup = screen.NewEntity<DockWindowGroup>();
		var outlineGroup = screen.NewEntity<DockWindowGroup>();
		leftStack.AttachChildren<DockContainer, DockLayoutNode>([filesGroup, outlineGroup]);

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
		var group = screen.NewEntity<DockWindowGroup>();
		container.AttachChild<DockContainer, DockLayoutNode>(group);
		return AddDockWindow(screen, group, title, content);
	}

	static void AddPropertiesControlLayout(Screen screen, DockWindow window, string status)
	{
		var layout = window.AddComponent<ControlLayout>();
		window.AddComponent<DockWindowControlLayout>();
		var root = screen.NewEntity<ControlLayoutNode>(entity => new ControlLayoutNode(entity, ControlLayoutOrientation.Stack));
		layout.AttachChild(root);

		var gridPlacement = screen.NewEntity<ControlLayoutControl>();
		var grid = gridPlacement.AddComponent(entity => new GridControl(entity, 2));
		root.AttachChild(gridPlacement);

		AddGridCellControl(screen, grid, 0, 0, entity => new LabelControl(entity, "Name"));
		AddGridCellControl(screen, grid, 0, 1, entity => new TextInputControl(entity, "README.md"));
		AddGridCellControl(screen, grid, 1, 0, entity => new LabelControl(entity, "Status"));
		AddGridCellControl(screen, grid, 1, 1, entity => new LabelControl(entity, status));
	}

	static TControl AddGridCellControl<TControl>(Screen screen, GridControl grid, int row, int column, Func<Entity, TControl> createControl)
		where TControl : Component
	{
		var entity = screen.NewEntity<GridCellControl>(e => new GridCellControl(e, row, column));
		var control = entity.AddComponent(createControl);
		grid.AttachChild(entity);
		return control;
	}

	static void AddFileTreeControlLayout(Screen screen, DockWindow window)
	{
		var layout = window.AddComponent<ControlLayout>();
		window.AddComponent<DockWindowControlLayout>();

		var layoutStack = screen.NewEntity(entity => new ControlLayoutNode(entity, ControlLayoutOrientation.Stack));
		layout.AttachChild(layoutStack);

		var tree = screen.NewEntity<ControlLayoutControl, TreeControl>();
		tree.AttachChildren<TreeControl, TreeNode>(parent => 
		[
			screen.NewEntity<TreeNode>(entity => new TreeNode(entity, "src", TreeNodeIcon.Folder)).AttachChildren<TreeNode, TreeNode>(parent => 
			[
				screen.NewEntity<TreeNode>(entity => new TreeNode(entity, "App", TreeNodeIcon.Folder)).AttachChildren<TreeNode, TreeNode>(parent => 
				[
					screen.NewEntity<TreeNode>(entity => new TreeNode(entity, "Program.cs")),
					screen.NewEntity<TreeNode>(entity => new TreeNode(entity, "SQL", TreeNodeIcon.Folder)).AttachChildren<TreeNode, TreeNode>(parent => 
					[
						screen.NewEntity<TreeNode>(entity => new TreeNode(entity, "Query.sql"))
					])
				]),
				screen.NewEntity<TreeNode>(entity => new TreeNode(entity, "Dist", TreeNodeIcon.Folder)).AttachChildren<TreeNode, TreeNode>(parent => 
				[
						screen.NewEntity<TreeNode>(entity => new TreeNode(entity, "output.txt"))
				]),
				screen.NewEntity<TreeNode>(entity => new TreeNode(entity, "README.md"))
			])
		]);
	}

	static Screen NewWorkspaceScreen(Session session, string workspacePath, bool isFilesScreen)
	{
		var screen = new Screen(session, isFilesScreen ? "Workspace Files" : "Workspace");
		var titleLayout = AddTitleLayout(screen, screen.Title);
		screen.AddNewBreadCrumbEntity(titleLayout, "Home", "/", 0);
		screen.AddNewBreadCrumbEntity(titleLayout, "Workspaces", "/workspace", 1);
		screen.AddNewBreadCrumbEntity(titleLayout, "Workspace", workspacePath, 2);
		if (isFilesScreen)
		{
			screen.AddNewBreadCrumbEntity(titleLayout, "Files", $"{workspacePath}/files", 3);
		}
		return screen;
	}

	static Screen NewFallbackScreen(Session session)
		=> NewHomeScreen(session);
}