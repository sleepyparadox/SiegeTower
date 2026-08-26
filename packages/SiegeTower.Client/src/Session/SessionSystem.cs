using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.Routing;
using SiegeTower.Client.Pattern;
using SiegeTower.Client.Screens.Common;
using SiegeTower.Client.Screens.Home;
using SiegeTower.Client.Screens.WorkspaceList;
using SiegeTower.Client.Screens.Ollama;
using SiegeTower.Client.Screens.WorkspaceFiles;
using SiegeTower.Client.Screens.WorkspaceHome;
using SiegeTower.Client.Services.Uri;
using SiegeTower.Client.UX;

namespace SiegeTower.Client;

public sealed class SessionSystem : IDataSystem
{
	private readonly SessionData data;

	public SessionSystem(SessionData data)
	{
		ArgumentNullException.ThrowIfNull(data);
		this.data = data;
		data.BurgerMenu =
		[
			new MenuItem("Home", () => NavigateTo(GetNavigationUrlHomeScreen())),
			new MenuItem("Workspaces", () => NavigateTo(GetNavigationUrlWorkspaceListScreen())),
			new MenuItem("Ollama", () => NavigateTo(GetNavigationUrlOllamaScreen()))
		];
	}

	public void Redraw() => data.RequestRedrawEvent();

	public void SetActiveScreen(IScreenData screen)
	{
		ArgumentNullException.ThrowIfNull(screen);
		data.ActiveScreen = screen;
		Redraw();
	}

	public void NavigateTo(string uri)
	{
		ArgumentNullException.ThrowIfNull(uri);
		data.Services.NavigationManager.NavigateTo(uri);
	}

	public void ApplyNavigation(string uri)
	{
		var homeBreadCrumb = new BreadCrumb("SiegeTower", GetNavigationUrlHomeScreen());
		var parsedUri = UriService.Parse(uri, data.Context.BaseUri);
		if (parsedUri.PathParts.Length > 0 && string.Equals(parsedUri.PathParts[0], "workspace"))
		{
			var workspacesBreadCrumb = new BreadCrumb("Workspaces", GetNavigationUrlWorkspaceListScreen());
			if (parsedUri.PathParts.Length > 1)
			{
				data.Context.WorkspaceID = parsedUri.PathParts[1];
				data.BreadCrumbs = [homeBreadCrumb, workspacesBreadCrumb, new BreadCrumb(data.Context.WorkspaceID, GetNavigationUrlToWorkspaceScreen(data.Context.WorkspaceID))];
				data.ActiveScreen = parsedUri.PathParts.Length > 2 && parsedUri.PathParts[2].Equals("files", StringComparison.OrdinalIgnoreCase)
					? new WorkspaceFilesScreenData(data)
					: new WorkspaceHomeScreenData(data);
			}
			else
			{
				data.BreadCrumbs = [homeBreadCrumb, workspacesBreadCrumb];
				data.ActiveScreen = new WorkspaceListScreenData(data);
			}
		}
		else if (parsedUri.PathParts.Length > 0 && string.Equals(parsedUri.PathParts[0], "ollama"))
		{
			data.BreadCrumbs = [homeBreadCrumb, new BreadCrumb("Ollama", GetNavigationUrlOllamaScreen())];
			data.ActiveScreen = new OllamaScreenData(data);
		}
		else
		{
			data.BreadCrumbs = [homeBreadCrumb];
			data.ActiveScreen = new HomeScreenData(data);
		}

		_ = SystemLoadActiveScreen();
		Redraw();
	}

	private Task SystemLoadActiveScreen() => data.ActiveScreen switch
	{
		HomeScreenData screen => screen.System.SystemLoad(screen),
		OllamaScreenData screen => screen.System.SystemLoad(screen),
		WorkspaceListScreenData screen => screen.System.SystemLoad(screen),
		WorkspaceHomeScreenData screen => screen.System.SystemLoad(screen),
		WorkspaceFilesScreenData screen => screen.System.SystemLoad(screen),
		_ => Task.CompletedTask
	};

	public void HandleLocationChanged(object? sender, LocationChangedEventArgs args) => ApplyNavigation(args.Location);

	public string GetNavigationUrlHomeScreen() => BuildNavigationUrl();
	public string GetNavigationUrlWorkspaceListScreen() => BuildNavigationUrl("workspace");
	public string GetNavigationUrlToWorkspaceScreen(string id) => BuildNavigationUrl("workspace", global::System.Uri.EscapeDataString(id));
	public string GetNavigationUrlToWorkspaceSCreen(string id) => GetNavigationUrlToWorkspaceScreen(id);
	public string GetNavigationUrlToWorkspaceFilesScreen(string id) => BuildNavigationUrl("workspace", global::System.Uri.EscapeDataString(id), "files");
	public string GetNavigationUrlOllamaScreen() => BuildNavigationUrl("ollama");

	public string BuildNavigationUrl(params string[] parts)
	{
		ArgumentNullException.ThrowIfNull(parts);
		return parts.Length == 0 ? data.Context.BaseUri : string.Join("/", new[] { data.Context.BaseUri }.Concat(parts));
	}

	public void DragStart(object target)
	{
		ArgumentNullException.ThrowIfNull(target);
		data.DragOperation.Target = target;
		Console.WriteLine($"Drag started: {target.GetType().Name}");
		Redraw();
	}

	public void DragStop(object over)
	{
		ArgumentNullException.ThrowIfNull(over);
		if (data.DragOperation.Target is IDockContent dockContent)
		{
			if (over is Dock overDock)
				DockService.Attach(overDock, dockContent);
			else if (over is DockGrid.Divider dockDivider)
				DockService.Attach(dockDivider.PrimaryDock, dockContent);
			else if (over is IDockContent overDockContent)
				DockService.Attach(overDockContent.Parent!, dockContent);
		}
		else
		{
			Console.WriteLine($"Drag stopped: {data.DragOperation.Target?.GetType().Name ?? "none"} over {over.GetType().Name}");
			data.DragOperation.Target = null;
		}
		Redraw();
	}

	public void DragStop(MouseEventArgs args)
	{
		if (data.DragOperation.Target is null)
			return;

		Console.WriteLine($"Drag stopped: {data.DragOperation.Target.GetType().Name} over no supported drop target " +
			$"at client ({args.ClientX}, {args.ClientY}), screen ({args.ScreenX}, {args.ScreenY}), " +
			$"button {args.Button}, buttons {args.Buttons}, " +
			$"modifiers [alt={args.AltKey}, ctrl={args.CtrlKey}, meta={args.MetaKey}, shift={args.ShiftKey}]");
		data.DragOperation.Target = null;
		Redraw();
	}
}
