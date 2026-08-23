using System.Net.Http;
using Microsoft.AspNetCore.Components;

namespace SiegeTower.Client;

public sealed class SessionServices
{
	public SessionServices(NavigationManager navigationManager, HttpClient httpClient)
	{
		ArgumentNullException.ThrowIfNull(navigationManager);
		ArgumentNullException.ThrowIfNull(httpClient);
		NavigationManager = navigationManager;
		HttpClient = httpClient;
	}

	public NavigationManager NavigationManager { get; }

	public HttpClient HttpClient { get; }

}
