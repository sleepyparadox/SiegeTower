using SiegeTower.Data;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("api/workspace", () => new[]
{
	new Workspace("1"),
	new Workspace("2")
});

app.MapGet("api/pod", () => new[]
{
	new Pod("st-workspace-1"),
	new Pod("st-workspace-2")
});

app.Run();