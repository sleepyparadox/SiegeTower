using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components;
using SiegeTower.Client.Screens.Common;
using SiegeTower.Client.Screens.Home;
using SiegeTower.Client.Screens.PodList;
using SiegeTower.Client.Services.Uri;
using SiegeTower.Client.UX;

namespace SiegeTower.Client;

// A session instance exists per browser tab
public sealed class Session
{
	public Session(NavigationManager navigationManager /*Dependency Injection*/)
	{
		ArgumentNullException.ThrowIfNull(navigationManager);
		NavigateTo(navigationManager.Uri);
	}

	public Screen ActiveScreen { get; private set; } = new HomeScreen();

	public SessionContext SessionContext { get; } = new()
	{
		BaseUri = string.Empty,
		ApiBaseUri = "localhost:5006/api"
	};

	public BreadCrumb[] BreadCrumbs { get; set; } = [];

	public DragOperation DragOperation { get; } = new();

	public event EventHandler? RedrawRequested;

	public void Redraw()
	{
		RedrawRequested?.Invoke(this, EventArgs.Empty);
	}

	public void SetActiveScreen(Screen screen)
	{
		ArgumentNullException.ThrowIfNull(screen);
		ActiveScreen = screen;
		Redraw();
	}

	#region Navigate

	public void NavigateTo(string uri)
	{
		var homeBreadCrumb = new BreadCrumb("SiegeTower", GetNavigationUrlHomeScreen());

		var parsedUri = UriService.Parse(uri, SessionContext.BaseUri);
		if (parsedUri.PathParts.Length > 0 && string.Equals(parsedUri.PathParts[0], "pods"))
		{
			var podsBreadCrumb = new BreadCrumb("Pods", GetNavigationUrlPodListScreen());
			BreadCrumbs = [homeBreadCrumb, podsBreadCrumb];
			
			var podsScreen = new PodListScreen(this);
			ActiveScreen = podsScreen;
		}
		else
		{
			BreadCrumbs = [homeBreadCrumb];

			var homeScreen = new HomeScreen();
			ActiveScreen = homeScreen;
		}

		Redraw();
	}

	public string GetNavigationUrlHomeScreen() => BuildNavigationUrl();
	public string GetNavigationUrlPodListScreen() => BuildNavigationUrl("pods");

	public string BuildNavigationUrl(params string[] parts)
	{
		ArgumentNullException.ThrowIfNull(parts);
		if (parts.Length == 0)
		{
			return SessionContext.BaseUri;
		}

		return string.Join("/", new[] { SessionContext.BaseUri }.Concat(parts));
	}

	#endregion

	#region Drag

	public void DragStart(object target)
	{
		ArgumentNullException.ThrowIfNull(target);
		DragOperation.Target = target;
		Console.WriteLine($"Drag started: {target.GetType().Name}");
		Redraw();
	}

	public void DragStop(object over)
	{
		ArgumentNullException.ThrowIfNull(over);
		if (DragOperation.Target is IDockContent dockContent)
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
			Console.WriteLine($"Drag stopped: {DragOperation.Target?.GetType().Name ?? "none"} over {over.GetType().Name}");
			DragOperation.Target = null;
		}
		
		Redraw();
	}

	public void DragStop(MouseEventArgs args)
	{
		if (DragOperation.Target is null)
		{
			return;
		}

		Console.WriteLine($"Drag stopped: {DragOperation.Target.GetType().Name} over no supported drop target " +
			$"at client ({args.ClientX}, {args.ClientY}), screen ({args.ScreenX}, {args.ScreenY}), " +
			$"button {args.Button}, buttons {args.Buttons}, " +
			$"modifiers [alt={args.AltKey}, ctrl={args.CtrlKey}, meta={args.MetaKey}, shift={args.ShiftKey}]");
		DragOperation.Target = null;
		Redraw();
	}

	#endregion
}
