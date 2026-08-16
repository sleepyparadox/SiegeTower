using SiegeTower.Client.UX;
using SiegeTower.Data.Graph.File;

namespace SiegeTower.Client.Screens.WorkspaceFiles;

public sealed class FileTreeDockContent : IDockContent
{
	private readonly WorkspaceFilesScreen screen;

	public FileTreeDockContent(WorkspaceFilesScreen screen)
	{
		this.screen = screen ?? throw new ArgumentNullException(nameof(screen));
	}

	public string Name => "Files";

	public Dock? Parent { get; set; }

	public IReadOnlyList<FileRow> Files { get; set; } = [];

	public Task OpenFileAsync(FileRow file) => screen.OpenFileAsync(file);
}