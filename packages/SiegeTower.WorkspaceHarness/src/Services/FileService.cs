using SiegeTower.Data.Graph.File;

namespace SiegeTower.WorkspaceHarness.Services;

public sealed class FileService
{
	private readonly string rootPath;

	public string RootPath => rootPath;

	public FileService(IConfiguration configuration)
		: this(configuration["Workspace:Root"] ?? Directory.GetCurrentDirectory())
	{
	}

	public FileService(string rootPath)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(rootPath);
		var fullRootPath = Path.GetFullPath(rootPath);
		this.rootPath = Directory.ResolveLinkTarget(fullRootPath, true)?.FullName ?? fullRootPath;
	}

	public IReadOnlyList<FileRow> GetFiles(bool includeContents)
	{
		return Directory
			.EnumerateFiles(rootPath, "*", SearchOption.AllDirectories)
			.Order(StringComparer.Ordinal)
			.Select(path => new FileRow(
				Path.GetRelativePath(rootPath, GetSafePath(path)),
				includeContents ? File.ReadAllText(GetSafePath(path)) : null))
			.ToArray();
	}

	public IReadOnlyList<FileRow> SearchFiles(string searchTerm, bool includeContents = true)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(searchTerm);
		return Directory
			.EnumerateFiles(rootPath, "*", SearchOption.AllDirectories)
			.Order(StringComparer.Ordinal)
			.Where(path => Path.GetFileName(path).Contains(searchTerm, StringComparison.OrdinalIgnoreCase)
				|| File.ReadLines(path).Any(line => line.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)))
			.Select(path => new FileRow(
				Path.GetRelativePath(rootPath, GetSafePath(path)),
				includeContents ? File.ReadAllText(GetSafePath(path)) : null))
			.ToArray();
	}

	public FileRow WriteFile(string path, string contents)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(path);
		ArgumentNullException.ThrowIfNull(contents);
		var safePath = GetSafePath(path);
		Directory.CreateDirectory(Path.GetDirectoryName(safePath)!);
		File.WriteAllText(safePath, contents);
		return new FileRow(Path.GetRelativePath(rootPath, safePath), contents);
	}

	public string GetSafePath(string path)
	{
		var fullPath = Path.GetFullPath(path, rootPath);
		var relativePath = Path.GetRelativePath(rootPath, fullPath);
		if (relativePath == ".." || relativePath.StartsWith(".." + Path.DirectorySeparatorChar, StringComparison.Ordinal))
		{
			throw new UnauthorizedAccessException("The requested path is outside the workspace root.");
		}

		var resolvedPath = rootPath;
		foreach (var part in relativePath.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries))
		{
			resolvedPath = Path.Combine(resolvedPath, part);
			if (Directory.Exists(resolvedPath))
			{
				resolvedPath = Directory.ResolveLinkTarget(resolvedPath, true)?.FullName ?? resolvedPath;
			}
			else if (File.Exists(resolvedPath))
			{
				resolvedPath = File.ResolveLinkTarget(resolvedPath, true)?.FullName ?? resolvedPath;
			}
			else
			{
				break;
			}
		}

		var resolvedRelativePath = Path.GetRelativePath(rootPath, resolvedPath);
		if (resolvedRelativePath == ".." || resolvedRelativePath.StartsWith(".." + Path.DirectorySeparatorChar, StringComparison.Ordinal))
		{
			throw new UnauthorizedAccessException("The requested path is outside the workspace root.");
		}

		return resolvedPath;
	}
}