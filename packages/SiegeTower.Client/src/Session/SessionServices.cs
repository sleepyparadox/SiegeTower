using System.Net.Http;
using Microsoft.AspNetCore.Components;
using SiegeTower.Client.Services.Workspace;
using SiegeTower.Client.Services.Ollama;

namespace SiegeTower.Client;

public sealed class SessionServices
{
	public SessionServices(NavigationManager navigationManager, HttpClient httpClient, IOllamaService ollamaService, WorkspaceFileService workspaceFileService, WorkspaceGitService workspaceGitService, WorkspaceProjectService workspaceProjectService)
	{
		ArgumentNullException.ThrowIfNull(navigationManager);
		ArgumentNullException.ThrowIfNull(httpClient);
		NavigationManager = navigationManager;
		HttpClient = httpClient;
		OllamaService = ollamaService;
		WorkspaceFileService = workspaceFileService;
		WorkspaceGitService = workspaceGitService;
		WorkspaceProjectService = workspaceProjectService;
	}

	public NavigationManager NavigationManager { get; }

	public HttpClient HttpClient { get; }

	public IOllamaService OllamaService { get; }

	public WorkspaceFileService WorkspaceFileService { get; }

	public WorkspaceGitService WorkspaceGitService { get; }

	public WorkspaceProjectService WorkspaceProjectService { get; }
}
