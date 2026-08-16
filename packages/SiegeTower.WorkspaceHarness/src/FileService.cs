using SiegeTower.Data.Graph.File;

namespace SiegeTower.WorkspaceHarness;

public sealed class FileService
{
	private readonly string rootPath;

	public FileService(IConfiguration configuration)
		: this(configuration["Workspace:Root"] ?? Directory.GetCurrentDirectory())
	{
	}

	public FileService(string rootPath)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(rootPath);
		this.rootPath = Path.GetFullPath(rootPath);
	}

	public IReadOnlyList<FileRow> GetFiles(bool includeContents)
	{
		return Directory
			.EnumerateFiles(rootPath, "*", SearchOption.AllDirectories)
			.Order(StringComparer.Ordinal)
			.Select(path => new FileRow(
				Path.GetRelativePath(rootPath, path),
				includeContents ? File.ReadAllText(path) : null))
			.ToArray();
	}
}
