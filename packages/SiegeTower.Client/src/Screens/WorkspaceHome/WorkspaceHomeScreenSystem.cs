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
		var task = LoadLogsAsync(data, DateTime.MinValue, cancellationToken);
		data.Session.LoadingQueue.Append(task);
		data.RefreshTimer ??= CreateLogRefreshTimer(data);
		return task;
	}

	public async Task SystemLoad(WorkspaceHomeScreenData data, CancellationToken cancellationToken = default)
	{
		await LoadAsync(data, cancellationToken);
		data.IsLoadedOnce = true;
		data.Session.RequestRedraw();
	}

	public PeriodicTimer CreateLogRefreshTimer(WorkspaceHomeScreenData data)
	{
		ArgumentNullException.ThrowIfNull(data);
		var timer = new PeriodicTimer(TimeSpan.FromSeconds(1));
		_ = RefreshLogsAsync(data, timer, async () =>
		{
			var minCreatedAtUtc = data.Cache
				.GetPrimaryIndex<OperationLogRowKey, WorkspaceOperationLog>()
				.Scan()
				.Select(log => log.CreatedAt)
				.DefaultIfEmpty(DateTime.MinValue)
				.Max();
			var task = LoadLogsAsync(data, minCreatedAtUtc);
			data.Session.LoadingQueue.Append(task);
			await task;
		});
		return timer;
	}

	public void Unload(WorkspaceHomeScreenData data)
	{
		ArgumentNullException.ThrowIfNull(data);
		data.RefreshTimer?.Dispose();
		data.RefreshTimer = null;
	}
	public Task SendMethod(WorkspaceHomeScreenData data, Operation operation) => SendMethodCoreAsync(data, operation);

	public IReadOnlyList<WorkspaceOperation> GetCachedOperations(WorkspaceHomeScreenData data) => data.Cache
		.GetPrimaryIndex<Guid, WorkspaceOperation>()
		.Scan()
		.OrderBy(operation => operation.CreatedAt)
		.ToArray();

	public IReadOnlyList<WorkspaceOperationLog> GetCachedOperationLogs(WorkspaceHomeScreenData data, WorkspaceOperation operation)
	{
		ArgumentNullException.ThrowIfNull(operation);
		return data.Cache
			.GetPrimaryIndex<OperationLogRowKey, WorkspaceOperationLog>()
			.Seek(
				new OperationLogRowKey(operation.ID, Guid.Empty),
				new OperationLogRowKey(operation.ID, new Guid("ffffffff-ffff-ffff-ffff-ffffffffffff")))
			.OrderBy(log => log.CreatedAt)
			.ToArray();
	}

	private async Task SendMethodCoreAsync(WorkspaceHomeScreenData data, Operation operation)
	{
		ArgumentNullException.ThrowIfNull(operation);
		await WorkspaceOperationService.SendAsync(new WorkspaceOperation { ID = Guid.NewGuid(), Operation = operation }, data.Session.Context, data.Session.Services.HttpClient);
		await LoadAsync(data);
	}

	private async Task RefreshLogsAsync(WorkspaceHomeScreenData data, PeriodicTimer timer, Func<Task> refresh)
	{
		while (await timer.WaitForNextTickAsync())
		{
			if (ReferenceEquals(data.RefreshTimer, timer))
			{
				await refresh();
			}
		}
	}

	private async Task LoadLogsAsync(WorkspaceHomeScreenData data, DateTime minCreatedAtUtc, CancellationToken cancellationToken = default)
	{
		var settingsTask = WorkspaceSettingsService.GetAsync(data.Session.Context, data.Session.Services.HttpClient, cancellationToken);
		await Task.WhenAll(
			WorkspaceOperationService.GetOperationsAsync(data.Cache, data.Session.Context, data.Session.Services.HttpClient, cancellationToken),
			WorkspaceOperationService.GetOperationLogsAsync(data.Cache, data.Session.Context, data.Session.Services.HttpClient, minCreatedAtUtc, cancellationToken),
			settingsTask);
		data.WorkspaceSettingsDockContent.SetSettings(await settingsTask);
		data.Session.RequestRedraw();
	}
}
