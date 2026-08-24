using SiegeTower.Data;
using SiegeTower.WorkspaceHarness;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddHttpClient("Ollama", client =>
{
	client.BaseAddress = new Uri(builder.Configuration["Ollama:Url"] ?? "http://st-ollama.siegetower.svc.cluster.local:11434/");
	client.Timeout = Timeout.InfiniteTimeSpan;
});
builder.Services.AddSingleton<WorkspaceHarness>();
var app = builder.Build();

app.MapGet("api/health", () => Results.Ok(new { status = "ok" }));
app.MapGet("api/file", (WorkspaceHarness harness, bool contents = false) => harness.WorkspaceContext.Services.FileService.GetFiles(contents));
app.MapGet("api/operation", (WorkspaceHarness harness) => Results.Ok(harness.GetOperations()));
app.MapGet("api/operation/all/log", (WorkspaceHarness harness) => Results.Ok(harness.GetOperationLogs()));
app.MapPost("api/operation", (OperationRow operation, WorkspaceHarness harness) =>
{
	if (!harness.TryStartOperation(operation))
	{
		return Results.BadRequest("An operation is already in progress.");
	}

	return Results.Accepted("api/operation", operation);
});
app.MapGet("api/workspace/settings", (WorkspaceHarness harness) => Results.Ok(harness.WorkspaceContext.Settings));
app.MapPost("api/workspace/settings", (WorkspaceContext.WorkspaceSettings settings, WorkspaceHarness harness) =>
{
	harness.WorkspaceContext.UpdateSettings(settings);
	return Results.Ok(harness.WorkspaceContext.Settings);
});
app.Run();
