using SiegeTower.Client.UX;
using SiegeTower.Data.Graph.File;

namespace SiegeTower.Client.Screens.WorkspaceFiles;

public sealed class FileEditDockContent : IDockContent
{
	public FileEditDockContent()
	{
	}

	public FileEditDockContent(FileRow file)
	{
		Path = file.Path;
		Contents = file.Contents ?? string.Empty;
	}

	public string Name => string.IsNullOrEmpty(Path) ? "File" : System.IO.Path.GetFileName(Path);

	public Dock? Parent { get; set; }

	public string Path { get; } = string.Empty;

	public string Contents { get; set; } = string.Empty;
}