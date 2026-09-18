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
		var createWindow = screen.NewEntity(entity => new DockWindow(entity, "Create Workspace", string.Empty));
		workspaceGroup.AttachChild(createWindow);

		var workspaceLayout = workspaceWindow.AddComponent<ControlLayout>();
		workspaceWindow.AddComponent<DockWindowControlLayout>();
		var workspaceList = screen.NewEntity(entity => new ControlLayoutNode(entity, ControlLayoutOrientation.Stack));
		workspaceLayout.AttachChild(workspaceList);
		AddCreateWorkspaceLayout(screen, createWindow, workspaceList);
		workspaceGroup.ActiveWindow = workspaceWindow;

		screen.NewEntity<AsyncTask>(entity => new AsyncTask(entity, () => LoadWorkspaces(session, screen, workspaceList)));
		return screen;
	}

	static async Task LoadWorkspaces(Session session, Screen screen, ControlLayoutNode workspaceList)
	{
		ClearWorkspaceList(workspaceList);
		var workspaceRows = await APISystem.WorkspaceGet(session);
		if (workspaceRows is null)
		{
			return;
		}

		foreach (var workspaceRow in workspaceRows)
		{
			var workspaceRowLayout = screen.NewEntity(entity => new ControlLayoutNode(entity, ControlLayoutOrientation.Row));
			workspaceList.AttachChild(workspaceRowLayout);
			var workspaceControl = screen.NewEntity<ControlLayoutControl>();
			workspaceControl.AddComponent(entity => new ButtonControl(entity, workspaceRow.Name,
				session => session.HandleEvent(new NavigationEvent($"/workspace/{workspaceRow.Name}"))));
			workspaceRowLayout.AttachChild(workspaceControl);
			var deleteControl = screen.NewEntity<ControlLayoutControl>();
			deleteControl.AddComponent(entity => new ButtonControl(entity, "Delete",
				session => screen.NewEntity<AsyncTask>(entity => new AsyncTask(entity, () => DeleteWorkspace(session, workspaceRow.Name, screen, workspaceList)))));
			workspaceRowLayout.AttachChild(deleteControl);
		}

		session.Redraw();
	}

	static void ClearWorkspaceList(ControlLayoutNode workspaceList)
	{
		var rows = ((IParentOf<ControlLayoutNode>)workspaceList).Children.Values.ToArray();
		foreach (var row in rows)
		{
			foreach (var control in ((IParentOf<ControlLayoutControl>)row).Children.Values.ToArray())
			{
				control.Entity.TryDeleteEntity();
			}

			row.Entity.TryDeleteEntity();
		}

		((IParentOf<ControlLayoutNode>)workspaceList).Children.Values.Clear();
	}

	static void AddCreateWorkspaceLayout(Screen screen, DockWindow window, ControlLayoutNode workspaceList)
	{
		var layout = window.AddComponent<ControlLayout>();
		window.AddComponent<DockWindowControlLayout>();
		var root = screen.NewEntity<ControlLayoutNode>(entity => new ControlLayoutNode(entity, ControlLayoutOrientation.Row));
		layout.AttachChild(root);
		var inputControl = screen.NewEntity<ControlLayoutControl>();
		var nameInput = inputControl.AddComponent(entity => new TextInputControl(entity, string.Empty));
		root.AttachChild(inputControl);
		var createControl = screen.NewEntity<ControlLayoutControl>();
		createControl.AddComponent(entity => new ButtonControl(entity, "Create",
			session => screen.NewEntity<AsyncTask>(entity => new AsyncTask(entity, () => CreateWorkspace(session, screen, nameInput, workspaceList)))));
		root.AttachChild(createControl);
	}

	static async Task DeleteWorkspace(Session session, string name, Screen screen, ControlLayoutNode workspaceList)
	{
		if (await APISystem.WorkspaceDelete(session, name))
		{
			await LoadWorkspaces(session, screen, workspaceList);
		}
	}

	static async Task CreateWorkspace(Session session, Screen screen, TextInputControl nameInput, ControlLayoutNode workspaceList)
	{
		if (string.IsNullOrWhiteSpace(nameInput.Value))
		{
			return;
		}

		var workspace = await APISystem.WorkspaceCreate(session, nameInput.Value.Trim());
		if (workspace is not null)
		{
			nameInput.Value = string.Empty;
			await LoadWorkspaces(session, screen, workspaceList);
		}
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