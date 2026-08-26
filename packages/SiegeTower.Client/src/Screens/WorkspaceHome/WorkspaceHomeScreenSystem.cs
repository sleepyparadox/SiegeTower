using SiegeTower.Data.ECSPattern;
using SiegeTower.Client.Services.Workspace;
using SiegeTower.Data;
using SiegeTower.GraphQuery;

namespace SiegeTower.Client.Screens.WorkspaceHome;

public sealed class WorkspaceHomeScreenSystem : ISystem
{
	public WorkspaceHomeScreenSystem() { }

	public Task Load(WorkspaceHomeScreenData data)
	{
		ArgumentNullException.ThrowIfNull(data);
		return LoadAsync(data);
	}
	public Task LoadAsync(WorkspaceHomeScreenData data, CancellationToken cancellationToken = default)
	{
		ArgumentNullException.ThrowIfNull(data);
		var task = LoadCoreAsync(data, cancellationToken);
		data.Session.LoadingQueue.Append(task);
		return task;
	}

	public async Task SystemLoad(WorkspaceHomeScreenData data, CancellationToken cancellationToken = default)
	{
		await LoadAsync(data, cancellationToken);
		data.IsLoadedOnce = true;
		data.Session.RequestRedraw();
	}
	public Task SendMethod(WorkspaceHomeScreenData data, Operation operation) => SendMethodCoreAsync(data, operation);

	private async Task SendMethodCoreAsync(WorkspaceHomeScreenData data, Operation operation)
	{
		ArgumentNullException.ThrowIfNull(operation);
		await WorkspaceOperationService.SendAsync(new WorkspaceOperation { ID = Guid.NewGuid(), Operation = operation }, data.Session.Context, data.Session.Services.HttpClient);
		await LoadAsync(data);
	}

	private async Task LoadCoreAsync(WorkspaceHomeScreenData data, CancellationToken cancellationToken)
	{
		var settingsTask = WorkspaceSettingsService.GetAsync(data.Session.Context, data.Session.Services.HttpClient, cancellationToken);
		await Task.WhenAll(
			WorkspaceOperationService.GetOperationsAsync(data.Cache, data.Session.Context, data.Session.Services.HttpClient, cancellationToken),
			WorkspaceOperationService.GetOperationLogsAsync(data.Cache, data.Session.Context, data.Session.Services.HttpClient, cancellationToken),
			settingsTask);
		data.WorkspaceSettingsDockContent.SetSettings(await settingsTask);
		data.Session.RequestRedraw();
	}
}
