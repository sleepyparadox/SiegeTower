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
			var homeScreen = new Screen(session, "Home");
			homeScreen.AddNewBreadCrumbEntity("Home", "/", true, 0);
			session.ActiveScreen = homeScreen;
		}
		else if (pathParts.Length == 1 && pathParts[0].Equals("workspace", StringComparison.OrdinalIgnoreCase))
		{
			var workspaceListScreen = new Screen(session, "Workspaces");
			workspaceListScreen.AddNewBreadCrumbEntity("Home", "/", true, 0);
			workspaceListScreen.AddNewBreadCrumbEntity("Workspaces", "/workspace", true, 1);
			session.ActiveScreen = workspaceListScreen;
		}
		else if (pathParts.Length == 1 && pathParts[0].Equals("ollama", StringComparison.OrdinalIgnoreCase))
		{
			var ollamaScreen = new Screen(session, "Ollama");
			ollamaScreen.AddNewBreadCrumbEntity("Home", "/", true, 0);
			ollamaScreen.AddNewBreadCrumbEntity("Ollama", "/ollama", true, 1);
			session.ActiveScreen = ollamaScreen;
		}
		else if (pathParts.Length >= 2 && pathParts[0].Equals("workspace", StringComparison.OrdinalIgnoreCase))
		{
			var workspacePath = $"/workspace/{pathParts[1]}";
			var isFilesScreen = pathParts.Length == 3 && pathParts[2].Equals("files", StringComparison.OrdinalIgnoreCase);
			var workspaceScreen = new Screen(session, isFilesScreen ? "Workspace Files" : "Workspace");
			workspaceScreen.AddNewBreadCrumbEntity("Home", "/", true, 0);
			workspaceScreen.AddNewBreadCrumbEntity("Workspaces", "/workspace", true, 1);
			workspaceScreen.AddNewBreadCrumbEntity("Workspace", workspacePath, true, 2);
			if (isFilesScreen)
			{
				workspaceScreen.AddNewBreadCrumbEntity("Files", $"{workspacePath}/files", true, 3);
			}
			session.ActiveScreen = workspaceScreen;
		}
		else
		{
			var homeScreen = new Screen(session, "Home");
			homeScreen.AddNewBreadCrumbEntity("Home", "/", true, 0);

			var rows = homeScreen.AddNewEntityAndComponents((s, e) => new Element(s, e, "Toolbar"), (s, e) => new ToolbarRows(s, e))
				.Item2;

			session.ActiveScreen = homeScreen;
		}

		session.NavigationManager.NavigateTo(url);
	}
}