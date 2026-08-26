using SiegeTower.Client.UX;
using SiegeTower.Data.Graph.File;

namespace SiegeTower.Client.Screens.WorkspaceFiles;

public sealed class FileTreeDockContent : IDockContent
{
	private readonly WorkspaceFilesScreenData data;

	public FileTreeDockContent(WorkspaceFilesScreenData data)
	{
		this.data = data ?? throw new ArgumentNullException(nameof(data));
	}

	public string Name => "Files";

	public Dock? Parent { get; set; }

	public IReadOnlyList<FileRow> Files { get; set; } = [];

	public Task OpenFileAsync(FileRow file) => data.System.OpenFileAsync(data, file);
}