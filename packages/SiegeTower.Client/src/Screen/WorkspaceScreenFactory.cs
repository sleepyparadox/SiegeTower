namespace SiegeTower.Client;

public static class WorkspaceScreenFactory
{
	public static Screen CreateWorkspaceHomeScreen(Session session, string workspacePath)
	{
		var screen = CreateScreen(session, "Workspace", workspacePath);
		var screenLayout = screen.SelectComponents<ScreenLayout>().Single();
		var workspaceNavToolbar = WorkspaceNavToolbar(screen, session, workspacePath);
		var dockingLayout = screen.NewEntity<DockingLayout>();
		screenLayout.AttachChildren<ScreenLayout, ScreenLayoutChild>([workspaceNavToolbar, dockingLayout]);

		var root = screen.NewEntity<DockContainer>(entity => new DockContainer(entity, DockOrientation.Vertical));
		dockingLayout.AttachChild<DockingLayout, DockLayoutNode>(root);
		AddDockWindow(screen, root, "Chat History", "Chat history will appear here.");

		var inputContainer = screen.NewEntity<DockContainer>(entity => new DockContainer(entity, DockOrientation.Vertical));
		inputContainer.IsFixedHeight = true;
		inputContainer.HeightInGridUnits = 6;
		root.AttachChild<DockContainer, DockLayoutNode>(inputContainer);
		var inputWindow = AddDockWindow(screen, inputContainer, "Chat Input", string.Empty);
		AddChatInputLayout(screen, inputWindow);
		return screen;
	}

	public static Screen CreateWorkspaceFilesScreen(Session session, string workspacePath)
	{
		var screen = CreateScreen(session, "Workspace Files", workspacePath, "Files");
		var screenLayout = screen.SelectComponents<ScreenLayout>().Single();
		var workspaceNavToolbar = WorkspaceNavToolbar(screen, session, workspacePath);
		var toolbarLayout = AddWorkspaceToolbar(screen);
		var dockingLayout = screen.NewEntity<DockingLayout>();
		screenLayout.AttachChildren<ScreenLayout, ScreenLayoutChild>([workspaceNavToolbar, toolbarLayout, dockingLayout]);

		var root = screen.NewEntity<DockContainer>(entity => new DockContainer(entity, DockOrientation.Horizontal));
		dockingLayout.AttachChild<DockingLayout, DockLayoutNode>(root);
		var filesContainer = screen.NewEntity<DockContainer>(entity => new DockContainer(entity, DockOrientation.Vertical));
		filesContainer.IsFixedWidth = true;
		filesContainer.WidthInGridUnits = 10;
		root.AttachChild<DockContainer, DockLayoutNode>(filesContainer);
		var filesWindow = AddDockWindow(screen, filesContainer, "Files", string.Empty);
		filesWindow.AddComponent<ControlLayout>();
		filesWindow.AddComponent<DockWindowControlLayout>();
		AddFileTreeControlLayout(screen, filesWindow);
		AddDockWindow(screen, root, "README.md", "Select a file to open it here.");
		return screen;
	}

	public static Screen CreateWorkspaceGitScreen(Session session, string workspacePath)
	{
		var screen = CreateScreen(session, "Workspace Git", workspacePath, "Git");
		var screenLayout = screen.SelectComponents<ScreenLayout>().Single();
		var workspaceNavToolbar = WorkspaceNavToolbar(screen, session, workspacePath);
		var toolbarLayout = AddGitToolbar(screen);
		var dockingLayout = screen.NewEntity<DockingLayout>();
		screenLayout.AttachChildren<ScreenLayout, ScreenLayoutChild>([workspaceNavToolbar, toolbarLayout, dockingLayout]);

		var root = screen.NewEntity<DockContainer>(entity => new DockContainer(entity, DockOrientation.Horizontal));
		dockingLayout.AttachChild<DockingLayout, DockLayoutNode>(root);
		AddDockWindow(screen, root, "Commits", "main\n  Initial commit\n  Add workspace files\n  Update agent prompt");
		AddDockWindow(screen, root, "Git Details", "Select a commit or changed file to inspect it.");
		return screen;
	}

	static Screen CreateScreen(Session session, string title, string workspacePath, string? childBreadcrumb = null)
	{
		ArgumentNullException.ThrowIfNull(session);
		ArgumentException.ThrowIfNullOrEmpty(workspacePath);

		var screen = new Screen(session, title);
		var titleLayout = AddTitleLayout(screen, title);
		screen.AddNewBreadCrumbEntity(titleLayout, "Home", "/", 0);
		screen.AddNewBreadCrumbEntity(titleLayout, "Workspaces", "/workspace", 1);
		screen.AddNewBreadCrumbEntity(titleLayout, "Workspace", workspacePath, 2);
		if (childBreadcrumb is not null)
		{
			screen.AddNewBreadCrumbEntity(titleLayout, childBreadcrumb, $"{workspacePath}/{childBreadcrumb.ToLowerInvariant().Replace(' ', '-')}", 3);
		}

		return screen;
	}

	static ToolbarLayout WorkspaceNavToolbar(Screen screen, Session session, string workspacePath)
		=> screen.NewEntity<ToolbarLayout>().AttachChildren<ToolbarLayout, Toolbar>(layout => [
			layout.AddToolbar(0).AttachChildren<Toolbar, ToolbarControl>(toolbar => [
				toolbar.AddToolbarControl<ButtonControl>(entity => new ButtonControl(entity, "Workspace", session => session.HandleEvent(new NavigationEvent(workspacePath)))),
				toolbar.AddToolbarControl<ButtonControl>(entity => new ButtonControl(entity, "Files", session => session.HandleEvent(new NavigationEvent($"{workspacePath}/files")))),
				toolbar.AddToolbarControl<ButtonControl>(entity => new ButtonControl(entity, "Git", session => session.HandleEvent(new NavigationEvent($"{workspacePath}/git"))))
			])
		]);

	static ToolbarLayout AddWorkspaceToolbar(Screen screen)
		=> screen.NewEntity<ToolbarLayout>().AttachChildren<ToolbarLayout, Toolbar>(layout => [
			layout.AddToolbar(0).AttachChildren<Toolbar, ToolbarControl>(toolbar => [
				toolbar.AddToolbarControl<LabelControl>(entity => new LabelControl(entity, "Workspace")),
				toolbar.AddToolbarControl<ComboBoxControl>(entity => new ComboBoxControl(entity, "Development"))
			])
		]);

	static ToolbarLayout AddGitToolbar(Screen screen)
		=> screen.NewEntity<ToolbarLayout>().AttachChildren<ToolbarLayout, Toolbar>(layout => [
			layout.AddToolbar(0).AttachChildren<Toolbar, ToolbarControl>(toolbar => [
				toolbar.AddToolbarControl<LabelControl>(entity => new LabelControl(entity, "Workspace")),
				toolbar.AddToolbarControl<ComboBoxControl>(entity => new ComboBoxControl(entity, "Development")),
				toolbar.AddToolbarControl<LabelControl>(entity => new LabelControl(entity, "Local name")),
				toolbar.AddToolbarControl<TextInputControl>(entity => new TextInputControl(entity, "SiegeTower")),
				toolbar.AddToolbarControl<ButtonControl>(entity => new ButtonControl(entity, "Pull"))
			])
		]);

	static void AddChatInputLayout(Screen screen, DockWindow window)
	{
		var layout = window.AddComponent<ControlLayout>();
		window.AddComponent<DockWindowControlLayout>();
		var root = screen.NewEntity<ControlLayoutNode>(entity => new ControlLayoutNode(entity, ControlLayoutOrientation.Row));
		layout.AttachChild(root);
		var input = screen.NewEntity<ControlLayoutControl>();
		input.AddComponent(entity => new TextInputControl(entity, "Ask the agent..."));
		root.AttachChild(input);
		var send = screen.NewEntity<ControlLayoutControl>();
		send.AddComponent(entity => new ButtonControl(entity, "Send"));
		root.AttachChild(send);
	}

	static void AddFileTreeControlLayout(Screen screen, DockWindow window)
	{
		var layout = window.Entity.EntityStorage.SelectComponents<ControlLayout>().Single(layout => layout.Entity.ID == window.Entity.ID);
		var root = screen.NewEntity(entity => new ControlLayoutNode(entity, ControlLayoutOrientation.Stack));
		layout.AttachChild(root);
		var tree = screen.NewEntity<ControlLayoutControl, TreeControl>();
		tree.AttachChildren<TreeControl, TreeNode>(parent => [
			screen.NewEntity<TreeNode>(entity => new TreeNode(entity, "src", TreeNodeIcon.Folder)).AttachChildren<TreeNode, TreeNode>(parent => [
				screen.NewEntity<TreeNode>(entity => new TreeNode(entity, "Program.cs")),
				screen.NewEntity<TreeNode>(entity => new TreeNode(entity, "Services", TreeNodeIcon.Folder))
			]),
			screen.NewEntity<TreeNode>(entity => new TreeNode(entity, "README.md"))
		]);
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

	static DockWindow AddDockWindow(Screen screen, DockContainer container, string title, string content)
	{
		var group = screen.NewEntity<DockWindowGroup>();
		container.AttachChild<DockContainer, DockLayoutNode>(group);
		var window = screen.NewEntity(entity => new DockWindow(entity, title, content));
		group.AttachChild(window);
		group.ActiveWindow = window;
		return window;
	}
}
