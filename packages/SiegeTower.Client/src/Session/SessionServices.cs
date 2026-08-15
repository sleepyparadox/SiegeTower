using System.Net.Http;
using Microsoft.AspNetCore.Components;
using SiegeTower.Client.Services.Ollama;

namespace SiegeTower.Client;

public sealed class SessionServices
{
	public SessionServices(NavigationManager navigationManager, HttpClient httpClient, IOllamaService ollamaService)
	{
		ArgumentNullException.ThrowIfNull(navigationManager);
		ArgumentNullException.ThrowIfNull(httpClient);
		NavigationManager = navigationManager;
		HttpClient = httpClient;
		OllamaService = ollamaService;
	}

	public NavigationManager NavigationManager { get; }

	public HttpClient HttpClient { get; }

	public IOllamaService OllamaService { get; }
}
