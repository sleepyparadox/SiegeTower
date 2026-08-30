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