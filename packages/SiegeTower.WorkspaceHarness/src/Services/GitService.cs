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

		var localPath = fileService.GetGitPath(operation.LocalPath);
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
		return SwitchBranchAsync(new GitSwitchBranchOperation
		{
			LocalPath = operation.LocalPath,
			Branch = operation.Branch,
			Create = true
		}, accessToken, cancellationToken);
	}

	public Task<GitCommandResult> SwitchBranchAsync(GitSwitchBranchOperation operation, string? accessToken, CancellationToken cancellationToken = default)
	{
		ArgumentNullException.ThrowIfNull(operation);
		ArgumentException.ThrowIfNullOrWhiteSpace(operation.Branch);
		var gitArguments = operation.Create
			? new[] { "switch", "-c", operation.Branch }
			: new[] { "switch", operation.Branch };
		return RunGitAsync(gitArguments, accessToken, fileService.GetGitPath(operation.LocalPath), cancellationToken);
	}

	public Task<GitCommandResult> PushAsync(GitPushOperation operation, string? accessToken, CancellationToken cancellationToken = default)
	{
		ArgumentNullException.ThrowIfNull(operation);
		ArgumentException.ThrowIfNullOrWhiteSpace(operation.Branch);
		return RunGitAsync(["push", "origin", operation.Branch], accessToken, fileService.GetGitPath(operation.LocalPath), cancellationToken);
	}

	public Task<GitCommandResult> CommitAsync(GitCommitOperation operation, string? accessToken, CancellationToken cancellationToken = default)
	{
		ArgumentNullException.ThrowIfNull(operation);
		ArgumentException.ThrowIfNullOrWhiteSpace(operation.Message);
		var repositoryPath = fileService.GetGitPath(operation.LocalPath);
		return CommitRepositoryAsync(repositoryPath, operation.Message, accessToken, cancellationToken);
	}

	public async Task<GitRepositoryStatus> GetRepositoryStatusAsync(string localPath, CancellationToken cancellationToken = default)
	{
		var repositoryPath = fileService.GetGitPath(localPath);
		var branchResult = await RunGitAsync(["branch", "--show-current"], null, repositoryPath, cancellationToken);
		var branchesResult = await RunGitAsync(["branch", "--format=%(refname:short)"], null, repositoryPath, cancellationToken);
		var statusResult = await RunGitAsync(["status", "--short"], null, repositoryPath, cancellationToken);
		var changes = statusResult.Output
			.Split('\n', StringSplitOptions.RemoveEmptyEntries)
			.Where(line => line.Length >= 4)
			.Select(line => new GitChangeRow { Status = line[..2].Trim(), Path = line[3..] })
			.ToList();
		return new GitRepositoryStatus(
			branchResult.Output,
			branchesResult.Output.Split('\n', StringSplitOptions.RemoveEmptyEntries).ToList(),
			changes);
	}

	private async Task<GitCommandResult> CommitRepositoryAsync(string repositoryPath, string message, string? accessToken, CancellationToken cancellationToken)
	{
		await RunGitAsync(["add", "-A"], accessToken, repositoryPath, cancellationToken);
		return await RunGitAsync(["commit", "-m", message], accessToken, repositoryPath, cancellationToken);
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
			CreateNoWindow = true,
			Environment = { ["GIT_TERMINAL_PROMPT"] = "0" }
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

public sealed record GitRepositoryStatus(string CurrentBranch, List<string> Branches, List<GitChangeRow> Changes);