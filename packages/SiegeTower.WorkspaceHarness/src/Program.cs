using SiegeTower.WorkspaceHarness;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<FileService>();
var app = builder.Build();

app.MapGet("api/health", () => Results.Ok(new { status = "ok" }));
app.MapGet("api/file", (FileService fileService, bool contents = false) => fileService.GetFiles(contents));

app.Run();
