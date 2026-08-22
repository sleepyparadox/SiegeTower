using System.Diagnostics;
using SiegeTower.Data;

namespace SiegeTower.WorkspaceHarness.Services;

public sealed class GitService
{
	private readonly FileService fileService;

	public GitService(FileService fileService)
	{
		this.fileService = fileService;
	}

	public async Task PullAsync(WorkspaceProjectRow project, string accessToken, CancellationToken cancellationToken = default)
	{
		ArgumentNullException.ThrowIfNull(project);
		ArgumentException.ThrowIfNullOrWhiteSpace(accessToken);

		var projectPath = Path.Combine(fileService.RootPath, project.Namespace);
		var isRepository = Directory.Exists(Path.Combine(projectPath, ".git"));
		var arguments = isRepository
			? new[] { "-C", projectPath, "-c", $"http.extraHeader=Authorization: Bearer {accessToken}", "pull" }
			: new[] { "-c", $"http.extraHeader=Authorization: Bearer {accessToken}", "clone", project.GitRepo, projectPath };
		await RunAsync(arguments, cancellationToken);
	}

	private static async Task RunAsync(IReadOnlyList<string> arguments, CancellationToken cancellationToken)
	{
		var startInfo = new ProcessStartInfo
		{
			FileName = "git",
			RedirectStandardError = true,
			RedirectStandardOutput = true,
			UseShellExecute = false,
			CreateNoWindow = true
		};
		foreach (var argument in arguments)
		{
			startInfo.ArgumentList.Add(argument);
		}

		using var process = Process.Start(startInfo) ?? throw new InvalidOperationException("Could not start git.");

		var error = await process.StandardError.ReadToEndAsync(cancellationToken);
		await process.WaitForExitAsync(cancellationToken);
		if (process.ExitCode != 0)
		{
			throw new InvalidOperationException(error.Trim());
		}
	}

}