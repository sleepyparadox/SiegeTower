using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using SiegeTower.Client.Screens.Common;
using SiegeTower.Client.Screens.Home;
using SiegeTower.Client.Screens.PodList;
using SiegeTower.Client.Screens.Ollama;
using SiegeTower.Client.Services.Uri;
using SiegeTower.Client.UX;
using SiegeTower.Client.Services.Ollama;
using SiegeTower.Client.Debug;

namespace SiegeTower.Client;

// A session instance exists per browser tab
public sealed class Session : IDisposable
{
	public Session(NavigationManager injectedNavigationManager, HttpClient injectedHttpClient)
	{
		var uri = injectedNavigationManager.Uri;
		SessionServices = new(
			injectedNavigationManager,
			injectedHttpClient,
			DebugUiService.IsDebugUrl(uri) ? new FakeOllamaService() : new OllamaService(this));
		BurgerMenu =
		[
			new MenuItem("Home", () => NavigateTo(GetNavigationUrlHomeScreen())),
			new MenuItem("Pods", () => NavigateTo(GetNavigationUrlPodListScreen())),
			new MenuItem("Ollama", () => NavigateTo(GetNavigationUrlOllamaScreen()))
		];
		SessionServices.NavigationManager.LocationChanged += HandleLocationChanged;
		ApplyNavigation(uri);
	}

	public Screen ActiveScreen { get; private set; } = new HomeScreen();

	public SessionServices SessionServices { get; }

	public SessionContext SessionContext { get; } = new()
	{
		BaseUri = string.Empty,
		ApiBaseUri = "localhost:5006/api"
	};

	public BreadCrumb[] BreadCrumbs { get; set; } = [];

	public MenuItem[] BurgerMenu { get; }

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
		ArgumentNullException.ThrowIfNull(uri);
		SessionServices.NavigationManager.NavigateTo(uri);
	}

	private void ApplyNavigation(string uri)
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
		else if (parsedUri.PathParts.Length > 0 && string.Equals(parsedUri.PathParts[0], "ollama"))
		{
			var ollamaBreadCrumb = new BreadCrumb("Ollama", GetNavigationUrlOllamaScreen());
			BreadCrumbs = [homeBreadCrumb, ollamaBreadCrumb];

			var ollamaScreen = new OllamaScreen(this);
			ActiveScreen = ollamaScreen;
		}
		else
		{
			BreadCrumbs = [homeBreadCrumb];

			var homeScreen = new HomeScreen();
			ActiveScreen = homeScreen;
		}

		Redraw();
	}

	private void HandleLocationChanged(object? sender, LocationChangedEventArgs args)
	{
		ApplyNavigation(args.Location);
	}

	public string GetNavigationUrlHomeScreen() => BuildNavigationUrl();
	public string GetNavigationUrlPodListScreen() => BuildNavigationUrl("pods");
	public string GetNavigationUrlOllamaScreen() => BuildNavigationUrl("ollama");

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

	public void Dispose()
	{
		SessionServices.NavigationManager.LocationChanged -= HandleLocationChanged;
	}

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
