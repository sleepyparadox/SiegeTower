using System.Text.Json;
using SiegeTower.Data;
using SiegeTower.WorkspaceHarness;
using SiegeTower.Data.Ollama;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddHttpClient("GitHub", client =>
{
	client.BaseAddress = new Uri("https://api.github.com/");
	client.Timeout = TimeSpan.FromMinutes(2);
});
builder.Services.AddHttpClient("Ollama", client =>
{
	client.BaseAddress = new Uri(builder.Configuration["Ollama:Url"] ?? "http://st-ollama.siegetower.svc.cluster.local:11434/");
	client.Timeout = Timeout.InfiniteTimeSpan;
});
builder.Services.AddSingleton<WorkspaceContext>();
var app = builder.Build();

app.MapGet("api/health", () => Results.Ok(new { status = "ok" }));
app.MapGet("api/file", (WorkspaceContext context, bool contents = false) => context.Services.FileService.GetFiles(contents));
app.MapGet("api/chat", (WorkspaceContext context) => Results.Ok(context.GetChatHistory()));
app.MapGet("api/git", (WorkspaceContext context) => Results.Ok(context.GetGitStatus()));
app.MapGet("api/project", (WorkspaceContext context) => Results.Ok(context.GetProjects()));
app.MapPost("api/project", (WorkspaceProjectRow project, WorkspaceContext context) => Results.Ok(context.AddProject(project)));
app.MapPost("api/project/{namespace}/git-pull", async (string @namespace, WorkspaceContext context, CancellationToken cancellationToken) =>
{
	try
	{
		await context.PullProjectAsync(@namespace, cancellationToken);
		return Results.Ok();
	}
	catch (KeyNotFoundException exception)
	{
		return Results.NotFound(exception.Message);
	}
	catch (InvalidOperationException exception)
	{
		return Results.Problem(exception.Message, statusCode: StatusCodes.Status400BadRequest);
	}
});
app.MapPost("api/git/github-access-token", async (GithubAccessTokenRequest request, WorkspaceContext context, CancellationToken cancellationToken) =>
{
	if (string.IsNullOrWhiteSpace(request.AppId) || string.IsNullOrWhiteSpace(request.InstallationId) || string.IsNullOrWhiteSpace(request.PrivateKey))
	{
		return Results.BadRequest("GitHub App ID, installation ID, and private key are required.");
	}

	try
	{
		var accessToken = await context.Services.GitHubService.CreateAccessTokenAsync(request, cancellationToken);
		context.SetGithubAccessToken(accessToken.Token, accessToken.ExpiresAt);
		return Results.Ok(context.GetGitStatus());
	}
	catch (HttpRequestException exception)
	{
		return Results.Problem(exception.Message, statusCode: StatusCodes.Status502BadGateway);
	}
	catch (InvalidOperationException exception)
	{
		return Results.Problem(exception.Message, statusCode: StatusCodes.Status500InternalServerError);
	}
});
app.MapPost("api/chat", (OllamaChatMessage message, WorkspaceContext context) =>
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
			var assistantResponse = await context.Services.OllamaService.ChatAsync(loopMessages, context.Services.FileTool.Definitions, context.PromptTimeout, loop.CancellationToken);
			loopMessages.Add(assistantResponse.Message);
			for (var toolRound = 0; assistantResponse.Message.ToolCalls is { Count: > 0 }; toolRound++)
			{
				if (toolRound >= 8)
				{
					throw new InvalidOperationException("Ollama exceeded the maximum number of file-tool calls for one request.");
				}

				foreach (var toolCall in assistantResponse.Message.ToolCalls)
				{
					var toolResult = context.Services.FileTool.Invoke(toolCall);
					loopMessages.Add(new OllamaChatMessage("tool", JsonSerializer.Serialize(toolResult)));
				}

				loop.Increment();
				assistantResponse = await context.Services.OllamaService.ChatAsync(loopMessages, context.Services.FileTool.Definitions, context.PromptTimeout, loop.CancellationToken);
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
