using SiegeTower.WorkspaceHarness.Services;
using SiegeTower.WorkspaceHarness.Services.Ollama;

namespace SiegeTower.WorkspaceHarness;

public sealed class WorkspaceServices
{
	public WorkspaceServices(IConfiguration configuration, IHttpClientFactory httpClientFactory)
	{
		FileService = new FileService(configuration);
		FileTool = new FileTool(FileService);
		GitService = new GitService(FileService);
		OllamaService = new OllamaService(httpClientFactory.CreateClient("Ollama"));
	}

	public FileService FileService { get; }

	public FileTool FileTool { get; }

	public GitService GitService { get; }

	public OllamaService OllamaService { get; }
}