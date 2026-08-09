using SiegeTower.K8sExternalAdmin.Docker;
using SiegeTower.K8sExternalAdmin.Docker.DockerFileOperation;

namespace SiegeTower.K8sExternalAdmin.Images;

public static class Ollama
{
	public const string ImageName = "st-ollama";

	private const string DownloadUrl = "https://ollama.com/download/ollama-linux-amd64.tar.zst";
	private const string ArchiveName = "ollama-linux-amd64.tar.zst";

	public static void Build()
	{
		var repositoryRoot = FindRepositoryRoot();
		var downloadDirectory = Path.Combine(repositoryRoot, "packages", "SiegeTower.K8sExternalAdmin", "download");
		var archivePath = Path.Combine(downloadDirectory, ArchiveName);
		Directory.CreateDirectory(downloadDirectory);

		if (!File.Exists(archivePath))
		{
			LogService.Info($"Downloading Ollama archive to '{archivePath}'.");
			using var client = new HttpClient();
			using var response = client.GetAsync(DownloadUrl, HttpCompletionOption.ResponseHeadersRead).GetAwaiter().GetResult();
			response.EnsureSuccessStatusCode();
			using var source = response.Content.ReadAsStream();
			using var destination = File.Create(archivePath);
			source.CopyTo(destination);
		}

		DockerService.Build(
		[
			new From("ubuntu:24.04"),
			new Copy(ArchiveName, $"/tmp/{ArchiveName}"),
			new Run($"apt-get update && apt-get install -y ca-certificates zstd && update-ca-certificates && zstd -dc /tmp/{ArchiveName} | tar -x -C /usr && rm -f /tmp/{ArchiveName} && rm -rf /var/lib/apt/lists/*"),
			new Expose(11434),
			new Cmd("OLLAMA_HOST=0.0.0.0:11434 ollama serve")
		],
		[ImageName],
		new Dictionary<string, string>
		{
			[downloadDirectory] = "."
		});
	}

	static string FindRepositoryRoot()
	{
		var directory = new DirectoryInfo(AppContext.BaseDirectory);
		while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "siegetrain.packages.json")))
		{
			directory = directory.Parent;
		}

		return directory?.FullName ?? throw new DirectoryNotFoundException("Could not locate the SiegeTower repository root.");
	}
}