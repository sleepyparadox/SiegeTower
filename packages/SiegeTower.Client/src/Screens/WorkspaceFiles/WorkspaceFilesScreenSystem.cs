using SiegeTower.Data.ECSPattern;
using SiegeTower.Client.Services.Workspace;
using SiegeTower.Client.UX;
using SiegeTower.Data.Graph.File;
using SiegeTower.GraphQuery;

namespace SiegeTower.Client.Screens.WorkspaceFiles;

public sealed class WorkspaceFilesScreenSystem : ISystem
{
	public WorkspaceFilesScreenSystem() { }

	public Task Load(WorkspaceFilesScreenData data)
	{
		ArgumentNullException.ThrowIfNull(data);
		return LoadAsync(data);
	}
	public Task LoadAsync(WorkspaceFilesScreenData data, CancellationToken cancellationToken = default)
	{
		ArgumentNullException.ThrowIfNull(data);
		var task = LoadCoreAsync(data, cancellationToken);
		data.Session.LoadingQueue.Append(task);
		return task;
	}

	public async Task SystemLoad(WorkspaceFilesScreenData data, CancellationToken cancellationToken = default)
	{
		await LoadAsync(data, cancellationToken);
		data.IsLoadedOnce = true;
		data.Session.RequestRedraw();
	}
	public Task OpenFileAsync(WorkspaceFilesScreenData data, FileRow file) => TrackAsync(data, OpenFileCoreAsync(data, file));

	private async Task LoadCoreAsync(WorkspaceFilesScreenData data, CancellationToken cancellationToken)
	{
		data.FileTreeDockContent.Files = await WorkspaceFileService.GetFiles(data.Cache, data.Session.Context, data.Session.Services.HttpClient, false, cancellationToken);
		data.Session.RequestRedraw();
	}

	private async Task OpenFileCoreAsync(WorkspaceFilesScreenData data, FileRow file)
	{
		ArgumentNullException.ThrowIfNull(file);
		if (!data.OpenFiles.TryGetValue(file.Path, out var content))
		{
			var files = await WorkspaceFileService.GetFiles(data.Cache, data.Session.Context, data.Session.Services.HttpClient, true);
			var fileWithContents = files.FirstOrDefault(item => string.Equals(item.Path, file.Path, StringComparison.Ordinal));
			content = new FileEditDockContent(fileWithContents ?? file);
			data.OpenFiles.Add(file.Path, content);
			DockService.Attach(data.DockGrid.Center, content);
		}

		data.DockGrid.Center.ActiveContent = content;
		data.Session.RequestRedraw();
	}

	private async Task TrackAsync(WorkspaceFilesScreenData data, Task task)
	{
		data.Session.LoadingQueue.Append(task);
		await task;
	}
}
