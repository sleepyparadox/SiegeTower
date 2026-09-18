using System.Diagnostics;
using SiegeTower.Data;

namespace SiegeTower.WorkspaceHarness.Services;

public sealed class GitService
{
	private readonly FileService fileService;

	public GitService(FileService fileService)
	{
		this.fileService = fileService ?? throw new ArgumentNullException(nameof(fileService));
	}

	public Task<GitCommandResult> CloneAsync(GitCloneOperation operation, string? accessToken, CancellationToken cancellationToken = default)
	{
		ArgumentNullException.ThrowIfNull(operation);
		ArgumentException.ThrowIfNullOrWhiteSpace(operation.Repo);
		ArgumentException.ThrowIfNullOrWhiteSpace(operation.LocalPath);

		var localPath = fileService.GetSafePath(operation.LocalPath);
		var arguments = new List<string>();
		AddAuthentication(arguments, accessToken);
		arguments.Add("clone");
		if (!string.IsNullOrWhiteSpace(operation.Branch))
		{
			arguments.AddRange(["--branch", operation.Branch]);
		}

		arguments.Add(operation.Repo);
		arguments.Add(localPath);
		return RunAsync(arguments, fileService.RootPath, cancellationToken);
	}

	public Task<GitCommandResult> CreateBranchAsync(GitCreateBranchOperation operation, string? accessToken, CancellationToken cancellationToken = default)
	{
		ArgumentNullException.ThrowIfNull(operation);
		ArgumentException.ThrowIfNullOrWhiteSpace(operation.Branch);
		return RunGitAsync(["switch", "-c", operation.Branch], accessToken, fileService.RootPath, cancellationToken);
	}

	public Task<GitCommandResult> PushAsync(GitPushOperation operation, string? accessToken, CancellationToken cancellationToken = default)
	{
		ArgumentNullException.ThrowIfNull(operation);
		ArgumentException.ThrowIfNullOrWhiteSpace(operation.Branch);
		return RunGitAsync(["push", "origin", operation.Branch], accessToken, fileService.RootPath, cancellationToken);
	}

	public Task<GitCommandResult> CommitAsync(GitCommitOperation operation, string? accessToken, CancellationToken cancellationToken = default)
	{
		ArgumentNullException.ThrowIfNull(operation);
		ArgumentException.ThrowIfNullOrWhiteSpace(operation.Message);
		return RunGitAsync(["commit", "-am", operation.Message], accessToken, fileService.RootPath, cancellationToken);
	}

	private Task<GitCommandResult> RunGitAsync(IReadOnlyList<string> gitArguments, string? accessToken, string workingDirectory, CancellationToken cancellationToken)
	{
		var arguments = new List<string>();
		AddAuthentication(arguments, accessToken);
		arguments.AddRange(gitArguments);
		return RunAsync(arguments, workingDirectory, cancellationToken);
	}

	private static void AddAuthentication(List<string> arguments, string? accessToken)
	{
		if (!string.IsNullOrWhiteSpace(accessToken))
		{
			var credentials = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes($"x-access-token:{accessToken.Trim()}"));
			arguments.AddRange(["-c", $"http.extraHeader=Authorization: Basic {credentials}"]);
		}
	}

	private static async Task<GitCommandResult> RunAsync(IReadOnlyList<string> arguments, string workingDirectory, CancellationToken cancellationToken)
	{
		var startInfo = new ProcessStartInfo
		{
			FileName = "git",
			WorkingDirectory = workingDirectory,
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
		var outputTask = process.StandardOutput.ReadToEndAsync(cancellationToken);
		var errorTask = process.StandardError.ReadToEndAsync(cancellationToken);
		await process.WaitForExitAsync(cancellationToken);
		var output = await outputTask;
		var error = await errorTask;
		if (process.ExitCode != 0)
		{
			throw new InvalidOperationException(string.IsNullOrWhiteSpace(error) ? output.Trim() : error.Trim());
		}

		return new GitCommandResult(output.Trim(), error.Trim());
	}
}

public sealed record GitCommandResult(string Output, string Error);