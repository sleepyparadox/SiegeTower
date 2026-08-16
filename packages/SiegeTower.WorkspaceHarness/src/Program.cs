using System.Text.Json;
using SiegeTower.WorkspaceHarness;
using SiegeTower.Data.Ollama;
using SiegeTower.WorkspaceHarness.Services;
using SiegeTower.WorkspaceHarness.Services.Ollama;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<FileService>();
builder.Services.AddSingleton<FileTool>();
builder.Services.AddSingleton<WorkspaceContext>();
builder.Services.AddHttpClient<OllamaService>(client =>
{
	client.BaseAddress = new Uri(builder.Configuration["Ollama:Url"] ?? "http://st-ollama.siegetower.svc.cluster.local:11434/");
	client.Timeout = Timeout.InfiniteTimeSpan;
});
var app = builder.Build();

app.MapGet("api/health", () => Results.Ok(new { status = "ok" }));
app.MapGet("api/file", (FileService fileService, bool contents = false) => fileService.GetFiles(contents));
app.MapGet("api/chat", (WorkspaceContext context) => Results.Ok(context.GetChatHistory()));
app.MapPost("api/chat", (OllamaChatMessage message, WorkspaceContext context, FileTool fileTool, OllamaService ollamaService) =>
{
	if (!string.Equals(message.Role, "user", StringComparison.OrdinalIgnoreCase) || string.IsNullOrWhiteSpace(message.Content))
	{
		return Results.BadRequest("A non-empty user message is required.");
	}

	if (!context.TryStartLoop(message, out var loop))
	{
		return Results.Conflict(context.GetChatHistory());
	}

	var existingMessages = context.GetChatSnapshot();
	var initialMessages = existingMessages.Append(message).ToArray();
	loop.StartTimeout(context.PromptTimeout, () =>
	{
		context.TimeoutLoop(loop);
		return Task.CompletedTask;
	});
	_ = Task.Run(async () =>
	{
		var loopMessages = initialMessages.ToList();
		try
		{
			var assistantResponse = await ollamaService.ChatAsync(loopMessages, fileTool.Definitions, context.PromptTimeout, loop.CancellationToken);
			loopMessages.Add(assistantResponse.Message);
			for (var toolRound = 0; assistantResponse.Message.ToolCalls is { Count: > 0 }; toolRound++)
			{
				if (toolRound >= 8)
				{
					throw new InvalidOperationException("Ollama exceeded the maximum number of file-tool calls for one request.");
				}

				foreach (var toolCall in assistantResponse.Message.ToolCalls)
				{
					var toolResult = fileTool.Invoke(toolCall);
					loopMessages.Add(new OllamaChatMessage("tool", JsonSerializer.Serialize(toolResult)));
				}

				loop.Increment();
				assistantResponse = await ollamaService.ChatAsync(loopMessages, fileTool.Definitions, context.PromptTimeout, loop.CancellationToken);
				loopMessages.Add(assistantResponse.Message);
			}

			if (!assistantResponse.Done)
			{
				throw new InvalidOperationException($"Ollama did not complete the response{(assistantResponse.DoneReason is null ? string.Empty : $" ({assistantResponse.DoneReason})") }.");
			}

			context.CompleteLoop(loop, loopMessages.Skip(existingMessages.Count).ToArray());
		}
		catch (OperationCanceledException) when (loop.CancellationToken.IsCancellationRequested)
		{
			context.CompleteLoop(loop, []);
		}
		catch (Exception exception)
		{
			var completedMessages = loopMessages.Skip(existingMessages.Count).Append(new OllamaChatMessage("Harness", exception.Message));
			context.CompleteLoop(loop, completedMessages.ToArray());
		}
	}, CancellationToken.None);

	return Results.Accepted("api/chat", context.GetChatHistory());
});

app.Run();
