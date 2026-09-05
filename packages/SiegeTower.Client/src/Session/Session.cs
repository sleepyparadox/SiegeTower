using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;

namespace SiegeTower.Client;

public sealed class Session // 1 session per tab
{	
	internal NavigationManager NavigationManager { get; }
	public event Action? RedrawRequested;
	public Screen ActiveScreen { get; set; } = null!;
	public Session(NavigationManager injectedNavigationManager, HttpClient injectedHttpClient)
	{
		NavigationManager = injectedNavigationManager;
		var currentUrl = NavigationManager.ToBaseRelativePath(NavigationManager.Uri);
		HandleEvent(new NavigationEvent($"/{currentUrl}"));
	}

	public void HandleEvent(SessionEvent sessionEvent)
	{
		ArgumentNullException.ThrowIfNull(sessionEvent);
		NavigationSystem.HandleEvent(this, sessionEvent);
		TreeSystem.HandleEvent(this, sessionEvent);
		DockingSystem.HandleEvent(this, sessionEvent);
		ToolbarSystem.HandleEvent(this, sessionEvent);
		
		// Assume something changed
		Redraw();
	}

	public void Redraw()
	{
		RedrawRequested?.Invoke();
	}
}