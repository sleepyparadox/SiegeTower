using SiegeTower.Data;
using SiegeTower.Data.Graph.File;
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
app.MapPost("api/file", (FileRow file, WorkspaceHarness harness) =>
{
	if (file.Contents is null)
	{
		return Results.BadRequest("File contents are required.");
	}

	return Results.Ok(harness.WorkspaceContext.Services.FileService.WriteFile(file.Path, file.Contents));
});
app.MapGet("api/git/repo", async (WorkspaceHarness harness) => Results.Ok(await harness.GetGitReposAsync()));
app.MapGet("api/operation", (WorkspaceHarness harness) => Results.Ok(harness.GetOperations()));
app.MapGet("api/operation/all/log", (WorkspaceHarness harness, long? minCreatedAtUtcTicks = null) => Results.Ok(harness.GetOperationLogs(minCreatedAtUtcTicks)));
app.MapPost("api/operation", (OperationRow operation, WorkspaceHarness harness) =>
{
	if (!harness.TryStartOperation(operation))
	{
		return Results.BadRequest("An operation is already in progress.");
	}

	return Results.Accepted("api/operation", operation);
});
app.MapGet("api/workspace/settings", (WorkspaceHarness harness) => Results.Ok(harness.WorkspaceContext.Settings));
app.MapPost("api/workspace/settings", async (WorkspaceContext.WorkspaceSettings settings, WorkspaceHarness harness) =>
{
	var hasName = !string.IsNullOrWhiteSpace(settings.GitUserName);
	var hasEmail = !string.IsNullOrWhiteSpace(settings.GitUserEmail);
	if (hasName != hasEmail)
	{
		return Results.BadRequest("Git author name and email must both be set, or both be left blank.");
	}

	if (hasName)
	{
		await harness.WorkspaceContext.Services.GitService.ConfigureIdentityAsync(settings.GitUserName!, settings.GitUserEmail!);
	}

	harness.WorkspaceContext.UpdateSettings(settings);
	return Results.Ok(harness.WorkspaceContext.Settings);
});
app.Run();
