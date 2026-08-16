using SiegeTower.WorkspaceHarness;
using SiegeTower.Data.Ollama;
using SiegeTower.WorkspaceHarness.Services.Ollama;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<FileService>();
builder.Services.AddSingleton<WorkspaceContext>();
builder.Services.AddHttpClient<OllamaService>(client =>
{
	client.BaseAddress = new Uri(builder.Configuration["Ollama:Url"] ?? "http://st-ollama.siegetower.svc.cluster.local:11434/");
});
var app = builder.Build();

app.MapGet("api/health", () => Results.Ok(new { status = "ok" }));
app.MapGet("api/file", (FileService fileService, bool contents = false) => fileService.GetFiles(contents));
app.MapGet("api/chat", (WorkspaceContext context) => Results.Ok(context.ChatHistory));
app.MapPost("api/chat", async (OllamaChatMessage message, WorkspaceContext context, OllamaService ollamaService, CancellationToken cancellationToken) =>
{
	if (!string.Equals(message.Role, "user", StringComparison.OrdinalIgnoreCase) || string.IsNullOrWhiteSpace(message.Content))
	{
		return Results.BadRequest("A non-empty user message is required.");
	}

	context.ChatHistory.Add(message);
	var response = string.Empty;
	await ollamaService.ChatAsync(context.ChatHistory, token => response += token, cancellationToken);
	context.ChatHistory.Add(new OllamaChatMessage("assistant", response));
	return Results.Ok(context.ChatHistory);
});

app.Run();
