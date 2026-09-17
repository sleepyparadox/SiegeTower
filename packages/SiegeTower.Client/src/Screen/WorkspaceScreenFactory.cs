namespace SiegeTower.Client;

public static class WorkspaceScreenFactory
{
	public static Screen CreateWorkspaceScreen(Session session, string workspacePath, bool isFilesScreen)
	{
		ArgumentNullException.ThrowIfNull(session);
		ArgumentException.ThrowIfNullOrEmpty(workspacePath);

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