using SiegeTower.Data.ECSPattern;
using SiegeTower.Client.Services.API;
using SiegeTower.Client.UX;
using SiegeTower.Data;
using SiegeTower.GraphQuery;

namespace SiegeTower.Client.Screens.WorkspaceList;

public sealed class WorkspaceListScreenSystem : ISystem
{
	public WorkspaceListScreenSystem() { }

	public Task Load(WorkspaceListScreenData data)
	{
		ArgumentNullException.ThrowIfNull(data);
		return LoadAsync(data);
	}
	public Task LoadAsync(WorkspaceListScreenData data, CancellationToken cancellationToken = default)
	{
		ArgumentNullException.ThrowIfNull(data);
		var task = LoadCoreAsync(data, cancellationToken);
		data.LoadingQueue.Append(task);
		return task;
	}

	public Task SystemLoad(WorkspaceListScreenData data, CancellationToken cancellationToken = default) => LoadAsync(data, cancellationToken);

	private async Task LoadCoreAsync(WorkspaceListScreenData data, CancellationToken cancellationToken)
	{
		var workspaces = await APIService.Get<WorkspaceRow>(data.Cache, data.Session.Context, cancellationToken);
		data.WorkspaceListDockContent.Workspaces = workspaces;
		data.Workspaces = workspaces;
		data.Session.RequestRedraw();
	}

	public void OpenWorkspace(WorkspaceListScreenData data, string id)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(id);
		data.Session.Context.WorkspaceID = id;
		data.Session.NavigateTo($"workspace/{Uri.EscapeDataString(id)}");
	}

	public Task CreateAsync(WorkspaceListScreenData data, CancellationToken cancellationToken = default) => TrackAsync(data, CreateCoreAsync(data, cancellationToken));

	private async Task CreateCoreAsync(WorkspaceListScreenData data, CancellationToken cancellationToken)
	{
		var content = data.WorkspaceListCreateContent;
		if (string.IsNullOrWhiteSpace(content.WorkspaceName) || content.IsCreating) return;
		content.IsCreating = true;
		try
		{
			await APIService.CreateWorkspace(data.Cache, data.Session.Context, content.WorkspaceName.Trim(), cancellationToken);
			content.WorkspaceName = string.Empty;
			await LoadAsync(data, cancellationToken);
		}
		finally { content.IsCreating = false; }
	}

	public Task DeleteWorkspaceAsync(WorkspaceListScreenData data, string id, CancellationToken cancellationToken = default) => TrackAsync(data, DeleteWorkspaceCoreAsync(data, id, cancellationToken));

	private async Task DeleteWorkspaceCoreAsync(WorkspaceListScreenData data, string id, CancellationToken cancellationToken)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(id);
		await APIService.DeleteWorkspace(data.Cache, data.Session.Context, id, cancellationToken);
		await LoadAsync(data, cancellationToken);
	}

	public Task DeleteAllWorkspacesAsync(WorkspaceListScreenData data, CancellationToken cancellationToken = default) => TrackAsync(data, DeleteAllWorkspacesCoreAsync(data, cancellationToken));

	private async Task DeleteAllWorkspacesCoreAsync(WorkspaceListScreenData data, CancellationToken cancellationToken)
	{
		await APIService.DeleteAllWorkspaces(data.Cache, data.Session.Context, cancellationToken);
		await LoadAsync(data, cancellationToken);
	}

	public Task GenerateAccessTokenAsync(WorkspaceListScreenData data, CancellationToken cancellationToken = default) => TrackAsync(data, GenerateAccessTokenCoreAsync(data, cancellationToken));

	private async Task GenerateAccessTokenCoreAsync(WorkspaceListScreenData data, CancellationToken cancellationToken)
	{
		var content = data.WorkspaceGitAuthContent;
		if (string.IsNullOrWhiteSpace(content.AppId) || string.IsNullOrWhiteSpace(content.InstallationId) || string.IsNullOrWhiteSpace(content.PrivateKey) || content.IsGenerating) return;
		content.IsGenerating = true;
		content.Error = null;
		data.Session.RequestRedraw();
		try
		{
			content.AccessToken = await APIService.GenerateGithubAccessToken(data.Cache, data.Session.Context, new GithubAccessTokenRequest { AppId = content.AppId.Trim(), InstallationId = content.InstallationId.Trim(), PrivateKey = content.PrivateKey.Trim() }, cancellationToken);
			content.PrivateKey = string.Empty;
		}
		catch (Exception exception) { content.Error = exception; }
		finally { content.IsGenerating = false; data.Session.RequestRedraw(); }
	}

	private async Task TrackAsync(WorkspaceListScreenData data, Task task)
	{
		data.LoadingQueue.Append(task);
		await task;
	}
}
