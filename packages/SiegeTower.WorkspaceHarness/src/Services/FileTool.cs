using System.Text.Json;
using SiegeTower.Data.Graph.File;
using SiegeTower.Data.Ollama;

namespace SiegeTower.WorkspaceHarness.Services;

public sealed class FileTool
{
	private readonly FileService fileService;

	public FileTool(FileService fileService)
	{
		this.fileService = fileService;
	}

	public IReadOnlyList<OllamaToolDefinition> Definitions =>
	[
		new()
		{
			Function = new()
			{
				Name = "get_files",
				Description = "List files in the workspace, optionally including their contents.",
				Parameters = new { type = "object", properties = new { includeContents = new { type = "boolean" } }, required = Array.Empty<string>() }
			}
		},
		new()
		{
			Function = new()
			{
				Name = "search_files",
				Description = "Search workspace file names and text contents.",
				Parameters = new { type = "object", properties = new { searchTerm = new { type = "string" }, includeContents = new { type = "boolean" } }, required = new[] { "searchTerm" } }
			}
		},
		new()
		{
			Function = new()
			{
				Name = "write_file",
				Description = "Write text to a file within the workspace.",
				Parameters = new { type = "object", properties = new { path = new { type = "string" }, contents = new { type = "string" } }, required = new[] { "path", "contents" } }
			}
		}
	];

	public object Invoke(OllamaToolCall call)
	{
		var arguments = call.Function.Arguments;
		return call.Function.Name switch
		{
			"get_files" => fileService.GetFiles(GetBoolean(arguments, "includeContents")),
			"search_files" => fileService.SearchFiles(arguments.GetProperty("searchTerm").GetString() ?? string.Empty, GetBoolean(arguments, "includeContents")),
			"write_file" => fileService.WriteFile(arguments.GetProperty("path").GetString() ?? string.Empty, arguments.GetProperty("contents").GetString() ?? string.Empty),
			_ => throw new InvalidOperationException($"Unknown Ollama tool '{call.Function.Name}'.")
		};
	}

	private static bool GetBoolean(JsonElement arguments, string name)
	{
		return arguments.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.True;
	}
}