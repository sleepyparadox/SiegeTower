using System.Net.Http;
using Microsoft.AspNetCore.Components;
using SiegeTower.Client.Services.Ollama;

namespace SiegeTower.Client;

public sealed class SessionServices
{
	public SessionServices(Session session, NavigationManager navigationManager, HttpClient httpClient)
	{
		ArgumentNullException.ThrowIfNull(session);
		ArgumentNullException.ThrowIfNull(navigationManager);
		ArgumentNullException.ThrowIfNull(httpClient);
		NavigationManager = navigationManager;
		HttpClient = httpClient;
		OllamaService = new(session);
	}

	public NavigationManager NavigationManager { get; }

	public HttpClient HttpClient { get; }

	public OllamaService OllamaService { get; }
}
