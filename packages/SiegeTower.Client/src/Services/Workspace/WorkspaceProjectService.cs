using System.Net.Http.Json;
using SiegeTower.Data;

namespace SiegeTower.Client.Services.Workspace;

public sealed class WorkspaceProjectService
{
	readonly Session session;

	public WorkspaceProjectService(Session session)
	{
		ArgumentNullException.ThrowIfNull(session);
		this.session = session;
	}

	public async Task<IReadOnlyList<WorkspaceProjectRow>> GetProjectsAsync(CancellationToken cancellationToken = default)
	{
		return await session.SessionServices.HttpClient.GetFromJsonAsync<List<WorkspaceProjectRow>>(GetRoute(), cancellationToken) ?? [];
	}

	public async Task<WorkspaceProjectRow> AddProjectAsync(WorkspaceProjectRow project, CancellationToken cancellationToken = default)
	{
		ArgumentNullException.ThrowIfNull(project);
		using var response = await session.SessionServices.HttpClient.PostAsJsonAsync(GetRoute(), project, cancellationToken);
		response.EnsureSuccessStatusCode();
		return await response.Content.ReadFromJsonAsync<WorkspaceProjectRow>(cancellationToken: cancellationToken)
			?? throw new InvalidOperationException("The workspace returned an empty project response.");
	}

	public async Task PullProjectAsync(string @namespace, CancellationToken cancellationToken = default)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(@namespace);
		using var response = await session.SessionServices.HttpClient.PostAsync(
			$"{GetRoute()}/{System.Uri.EscapeDataString(@namespace)}/git-pull",
			null,
			cancellationToken);
		response.EnsureSuccessStatusCode();
	}

	private string GetRoute()
	{
		var workspaceId = session.SessionContext.WorkspaceID;
		if (string.IsNullOrWhiteSpace(workspaceId))
		{
			throw new InvalidOperationException("A workspace ID is required to request projects.");
		}

		return $"/workspace/{System.Uri.EscapeDataString(workspaceId)}/api/project";
	}
}