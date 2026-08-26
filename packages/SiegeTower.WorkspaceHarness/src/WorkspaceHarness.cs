using System.Text.Json;
using SiegeTower.Data;
using SiegeTower.Data.Ollama;
using SiegeTower.GraphQuery;

namespace SiegeTower.WorkspaceHarness;

public sealed class WorkspaceHarness
{
	public WorkspaceHarness(IConfiguration configuration, IHttpClientFactory httpClientFactory)
	{
		WorkspaceContext = new WorkspaceContext(configuration, httpClientFactory);
		Cache = new GraphCache();
	}

	public WorkspaceContext WorkspaceContext { get; }

	public GraphCache Cache { get; }

	public OperationRow? CurrentOperation { get; private set; }

	public IReadOnlyList<OperationRow> GetOperations()
	{
		lock (Cache)
		{
			return Cache.GetPrimaryIndex<Guid, OperationRow>().Scan().ToArray();
		}
	}

	public IReadOnlyList<OperationLogRow> GetOperationLogs(long? minCreatedAtUtcTicks = null)
	{
		lock (Cache)
		{
			var minCreatedAt = minCreatedAtUtcTicks.HasValue
				? new DateTime(minCreatedAtUtcTicks.Value, DateTimeKind.Utc)
				: (DateTime?)null;
			return Cache.GetPrimaryIndex<OperationLogRowKey, OperationLogRow>()
				.Scan()
				.Where(log => minCreatedAt is null || log.CreatedAt >= minCreatedAt.Value)
				.ToArray();
		}
	}

	public bool TryStartOperation(OperationRow operation)
	{
		ArgumentNullException.ThrowIfNull(operation);
		lock (Cache)
		{
			if (CurrentOperation is not null)
			{
				return false;
			}

			CurrentOperation = operation;
			if (operation.CreatedAt == default)
			{
				operation.CreatedAt = DateTime.UtcNow;
			}
			operation.Cache = Cache;
			((GraphIndexUniqueGuid<OperationRow>)Cache.GetPrimaryIndex<Guid, OperationRow>()).Store([operation]);
		}

		_ = Task.Run(() => PerformOperationAsync(operation));
		return true;
	}

	private async Task PerformOperationAsync(OperationRow operation)
	{
		try
		{
			if (operation.Operation.GitClone is not null)
			{
				await PerformGitCloneOperationAsync(operation, operation.Operation.GitClone);
			}
			else if (operation.Operation.GitCreateBranch is not null)
			{
				await PerformGitCreateBranchOperationAsync(operation, operation.Operation.GitCreateBranch);
			}
			else if (operation.Operation.GitPushOperation is not null)
			{
				await PerformGitPushOperationAsync(operation, operation.Operation.GitPushOperation);
			}
			else if (operation.Operation.GitCommitOperation is not null)
			{
				await PerformGitCommitOperationAsync(operation, operation.Operation.GitCommitOperation);
			}
			else if (operation.Operation.Prompt is not null)
			{
				await PerformPromptOperationAsync(operation, operation.Operation.Prompt);
			}
			else
			{
				AddLog(operation, "Operation completed without a handler.");
			}
		}
		catch (Exception exception)
		{
			AddLog(operation, exception.Message);
		}
		finally
		{
			lock (Cache)
			{
				if (ReferenceEquals(CurrentOperation, operation))
				{
					CurrentOperation = null;
				}
			}
		}
	}

	private async Task PerformGitCloneOperationAsync(OperationRow operation, GitCloneOperation gitClone)
	{
		AddLog(operation, "Git clone started.");
		var result = await WorkspaceContext.Services.GitService.CloneAsync(gitClone, WorkspaceContext.GetGitAccessToken());
		AddLog(operation, result.Output);
	}

	private async Task PerformGitCreateBranchOperationAsync(OperationRow operation, GitCreateBranchOperation gitCreateBranch)
	{
		AddLog(operation, "Git branch creation started.");
		var result = await WorkspaceContext.Services.GitService.CreateBranchAsync(gitCreateBranch, WorkspaceContext.GetGitAccessToken());
		AddLog(operation, result.Output);
	}

	private async Task PerformGitPushOperationAsync(OperationRow operation, GitPushOperation gitPush)
	{
		AddLog(operation, "Git push started.");
		var result = await WorkspaceContext.Services.GitService.PushAsync(gitPush, WorkspaceContext.GetGitAccessToken());
		AddLog(operation, result.Output);
	}

	private async Task PerformGitCommitOperationAsync(OperationRow operation, GitCommitOperation gitCommit)
	{
		AddLog(operation, "Git commit started.");
		var result = await WorkspaceContext.Services.GitService.CommitAsync(gitCommit, WorkspaceContext.GetGitAccessToken());
		AddLog(operation, result.Output);
	}

	private async Task PerformPromptOperationAsync(OperationRow operation, PromptOperation prompt)
	{
		AddLog(operation, "Prompt started.");
		var messages = new List<OllamaChatMessage>
		{
			new("user", prompt.Prompt)
		};
		var response = await WorkspaceContext.Services.OllamaService.ChatAsync(
			messages,
			WorkspaceContext.Services.FileTool.Definitions,
			WorkspaceContext.PromptTimeout);
		for (var toolRound = 0; response.Message.ToolCalls is { Count: > 0 }; toolRound++)
		{
			if (toolRound >= 8)
			{
				throw new InvalidOperationException("Ollama exceeded the maximum number of file-tool calls for one operation.");
			}

			messages.Add(response.Message);
			foreach (var toolCall in response.Message.ToolCalls)
			{
				var toolResult = WorkspaceContext.Services.FileTool.Invoke(toolCall);
				messages.Add(new OllamaChatMessage("tool", JsonSerializer.Serialize(toolResult)));
			}

			response = await WorkspaceContext.Services.OllamaService.ChatAsync(
				messages,
				WorkspaceContext.Services.FileTool.Definitions,
				WorkspaceContext.PromptTimeout);
		}
		if (!response.Done)
		{
			throw new InvalidOperationException($"Ollama did not complete the response{(response.DoneReason is null ? string.Empty : $" ({response.DoneReason})") }.");
		}

		AddLog(operation, response.Message.Content ?? string.Empty);
	}

	private void AddLog(OperationRow operation, string message)
	{
		lock (Cache)
		{
			var log = new OperationLogRow
			{
				ID = Guid.NewGuid(),
				Operation_ID = operation.ID,
				CreatedAt = DateTime.UtcNow,
				Message = message
			};
			log.Cache = Cache;
			((GraphIndexUniqueOperationIDSelfID<OperationLogRow>)Cache.GetPrimaryIndex<OperationLogRowKey, OperationLogRow>()).Store([log]);
		}
	}
}