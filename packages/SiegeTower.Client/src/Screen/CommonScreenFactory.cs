namespace SiegeTower.Client;

public static class CommonScreenFactory
{
	public static Screen CreateWorkspaceListScreen(Session session)
	{
		ArgumentNullException.ThrowIfNull(session);

		var screen = new Screen(session, "Workspaces");
		var titleLayout = AddTitleLayout(screen, "Workspaces");
		screen.AddNewBreadCrumbEntity(titleLayout, "Home", "/", 0);
		screen.AddNewBreadCrumbEntity(titleLayout, "Workspaces", "/workspace", 1);

		var screenLayout = screen.SelectComponents<ScreenLayout>().Single();
		var dockingLayout = screen.NewEntity<DockingLayout>();
		screenLayout.AttachChild<ScreenLayout, ScreenLayoutChild>(dockingLayout);

		var dockStack = screen.NewEntity<DockContainer>(entity => new DockContainer(entity, DockOrientation.Vertical));
		var workspaceGroup = screen.NewEntity<DockWindowGroup>();
		dockingLayout.AttachChild<DockingLayout, DockLayoutNode>(dockStack);
		dockStack.AttachChild<DockContainer, DockLayoutNode>(workspaceGroup);

		var workspaceWindow = screen.NewEntity(entity => new DockWindow(entity, "Workspaces", string.Empty));
		workspaceGroup.AttachChild(workspaceWindow);
		workspaceGroup.ActiveWindow = workspaceWindow;

		var workspaceLayout = workspaceWindow.AddComponent<ControlLayout>();
		workspaceWindow.AddComponent<DockWindowControlLayout>();
		var workspaceList = screen.NewEntity(entity => new ControlLayoutNode(entity, ControlLayoutOrientation.Stack));
		workspaceLayout.AttachChild(workspaceList);

		screen.NewEntity<AsyncTask>(entity => new AsyncTask(entity, () => LoadWorkspaces(session, screen, workspaceList)));
		return screen;
	}

	static async Task LoadWorkspaces(Session session, Screen screen, ControlLayoutNode workspaceList)
	{
		var workspaceRows = await APISystem.WorkspaceGet(session);
		if (workspaceRows is null)
		{
			return;
		}

		foreach (var workspaceRow in workspaceRows)
		{
			var workspaceControl = screen.NewEntity<ControlLayoutControl>();
			workspaceControl.AddComponent(entity => new ButtonControl(entity, workspaceRow.Name, $"/workspace/{workspaceRow.Name}"));
			workspaceList.AttachChild(workspaceControl);
		}

		session.Redraw();
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
}