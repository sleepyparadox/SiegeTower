using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using SiegeTower.Client.Screens.Common;
using SiegeTower.Client.Screens.Home;
using SiegeTower.Client.Screens.WorkspaceList;
using SiegeTower.Client.Screens.Ollama;
using SiegeTower.Client.Screens.WorkspaceFiles;
using SiegeTower.Client.Screens.WorkspaceHome;
using SiegeTower.Client.Services.Uri;
using SiegeTower.Client.UX;
using SiegeTower.Client.Services.Ollama;
using SiegeTower.Client.Services.Workspace;
using SiegeTower.Client.Debug;

namespace SiegeTower.Client;

// A Main + SesssonData exists per browser tab
public sealed class SessionBlazorHandler : IDisposable
{
	public SessionData Data { get; } = new();

	public SessionSystem System { get; }

	public SessionBlazorHandler(NavigationManager injectedNavigationManager, HttpClient injectedHttpClient)
	{
		var uri = injectedNavigationManager.Uri;
		var apiBaseUri = new System.Uri(new System.Uri(uri), "/api").ToString();
		SessionContext = new()
		{
			BaseUri = string.Empty,
			ApiBaseUri = apiBaseUri
		};
		SessionServices = new(injectedNavigationManager, injectedHttpClient);
		Data.Context = SessionContext;
		Data.Services = SessionServices;
		System = new(Data);
		Data.RequestRedraw = System.Redraw;
		Data.NavigateTo = System.NavigateTo;
		SessionServices.NavigationManager.LocationChanged += System.HandleLocationChanged;
		System.ApplyNavigation(uri);
	}

	public SessionServices SessionServices { get; }

	public SessionContext SessionContext { get; }
	public void Dispose()
	{
		SessionServices.NavigationManager.LocationChanged -= System.HandleLocationChanged;
	}
}