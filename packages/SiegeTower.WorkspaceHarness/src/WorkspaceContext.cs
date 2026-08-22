using SiegeTower.Data.Ollama;
using SiegeTower.Data;

namespace SiegeTower.WorkspaceHarness;

public sealed class WorkspaceContext
{
	private readonly object sync = new();
	private readonly List<OllamaChatMessage> chatHistory = [];
	private readonly List<WorkspaceProjectRow> projects = [];

	public WorkspaceContext(IConfiguration configuration, IHttpClientFactory httpClientFactory)
	{
		Services = new WorkspaceServices(configuration, httpClientFactory);
	}

	public WorkspaceServices Services { get; }

	public TimeSpan PromptTimeout { get; } = TimeSpan.FromMinutes(5);

	public string? GithubAccessToken { get; private set; }

	public DateTime? GithubAccessTokenExpiresAtUtc { get; private set; }

	public IReadOnlyList<WorkspaceProjectRow> GetProjects()
	{
		lock (sync)
		{
			return projects.ToArray();
		}
	}

	public WorkspaceProjectRow AddProject(WorkspaceProjectRow project)
	{
		ArgumentNullException.ThrowIfNull(project);
		ArgumentException.ThrowIfNullOrWhiteSpace(project.Namespace);
		ArgumentException.ThrowIfNullOrWhiteSpace(project.GitRepo);
		lock (sync)
		{
			projects.RemoveAll(item => string.Equals(item.Namespace, project.Namespace, StringComparison.Ordinal));
			projects.Add(project);
			return project;
		}
	}

	public async Task PullProjectAsync(string projectNamespace, CancellationToken cancellationToken = default)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(projectNamespace);
		WorkspaceProjectRow project;
		string? accessToken;
		lock (sync)
		{
			project = projects.FirstOrDefault(item => string.Equals(item.Namespace, projectNamespace, StringComparison.Ordinal))
				?? throw new KeyNotFoundException($"Unknown workspace project '{projectNamespace}'.");
			accessToken = GithubAccessToken;
		}
		if (string.IsNullOrWhiteSpace(accessToken))
		{
			throw new InvalidOperationException("A GitHub access token is required to pull a project.");
		}
		await Services.GitService.PullAsync(project, accessToken, cancellationToken);
	}

	public GitStatus GetGitStatus()
	{
		lock (sync)
		{
			return new GitStatus
			{
				GithubAccessTokenExists = !string.IsNullOrWhiteSpace(GithubAccessToken),
				GithubAccessTokenExpiresAtUtc = GithubAccessTokenExpiresAtUtc
			};
		}
	}

	public void SetGithubAccessToken(string token, DateTime expiresAtUtc)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(token);
		lock (sync)
		{
			GithubAccessToken = token;
			GithubAccessTokenExpiresAtUtc = expiresAtUtc.ToUniversalTime();
		}
	}

	public TaskLoop? CurrentLoop { get; private set; }

	public IReadOnlyList<OllamaChatMessage> GetChatHistory()
	{
		lock (sync)
		{
			var history = chatHistory.ToList();
			if (CurrentLoop is not null)
			{
				var seconds = (DateTime.UtcNow - CurrentLoop.StartedAtUtc).TotalSeconds;
				history.Add(new OllamaChatMessage("Harness", $"Loop {CurrentLoop.Loops} (for {seconds:0} seconds)"));
			}

			return history;
		}
	}

	public bool TryStartLoop(OllamaChatMessage message, out TaskLoop loop)
	{
		lock (sync)
		{
			if (CurrentLoop is not null)
			{
				loop = null!;
				return false;
			}

			loop = new TaskLoop();
			CurrentLoop = loop;
			return true;
		}
	}

	public IReadOnlyList<OllamaChatMessage> GetChatSnapshot()
	{
		lock (sync)
		{
			return chatHistory.ToList();
		}
	}

	public void CompleteLoop(TaskLoop loop, IReadOnlyList<OllamaChatMessage> messages)
	{
		lock (sync)
		{
			if (!ReferenceEquals(CurrentLoop, loop) || !loop.TryComplete())
			{
				return;
			}

			chatHistory.AddRange(messages);
			CurrentLoop = null;
		}
	}

	public void TimeoutLoop(TaskLoop loop)
	{
		lock (sync)
		{
			if (!ReferenceEquals(CurrentLoop, loop) || !loop.TryComplete())
			{
				return;
			}

			loop.Cancel();
			chatHistory.Add(new OllamaChatMessage("Harness", "Loop timed out after 120 seconds."));
			CurrentLoop = null;
		}
	}
}

public sealed class TaskLoop
{
	private readonly CancellationTokenSource cancellationSource = new();
	private readonly CancellationTokenSource timeoutCancellationSource = new();
	private int loops;
	private int completedState;

	public DateTime StartedAtUtc { get; } = DateTime.UtcNow;

	public int Loops => Volatile.Read(ref loops);

	public CancellationToken CancellationToken => cancellationSource.Token;

	public Task TimeoutTask { get; private set; } = Task.CompletedTask;

	public int Increment()
	{
		return Interlocked.Increment(ref loops);
	}

	public void StartTimeout(TimeSpan timeout, Func<Task> onTimeout)
	{
		if (timeout <= TimeSpan.Zero)
		{
			throw new ArgumentOutOfRangeException(nameof(timeout));
		}

		ArgumentNullException.ThrowIfNull(onTimeout);
		TimeoutTask = Task.Run(async () =>
		{
			try
			{
				await Task.Delay(timeout, timeoutCancellationSource.Token);
				await onTimeout();
			}
			catch (OperationCanceledException) when (timeoutCancellationSource.IsCancellationRequested)
			{
			}
		});
	}

	public bool TryComplete()
	{
		if (Interlocked.Exchange(ref completedState, 1) != 0)
		{
			return false;
		}

		timeoutCancellationSource.Cancel();
		return true;
	}

	public void Cancel()
	{
		cancellationSource.Cancel();
	}
}