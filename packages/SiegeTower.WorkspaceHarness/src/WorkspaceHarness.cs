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

	public IReadOnlyList<OperationLogRow> GetOperationLogs()
	{
		lock (Cache)
		{
			return Cache.GetPrimaryIndex<OperationLogRowKey, OperationLogRow>().Scan().ToArray();
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

		_ = Task.Run(() => CompleteOperationAsync(operation));
		return true;
	}

	private async Task CompleteOperationAsync(OperationRow operation)
	{
		try
		{
			if (operation.Operation.Prompt is not null)
			{
				await HandlePromptOperationAsync(operation, operation.Operation.Prompt);
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

	private async Task HandlePromptOperationAsync(OperationRow operation, PromptOperation prompt)
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