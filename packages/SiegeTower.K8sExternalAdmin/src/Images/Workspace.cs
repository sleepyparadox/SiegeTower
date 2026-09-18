using SiegeTower.K8sExternalAdmin.Docker;
using SiegeTower.K8sExternalAdmin.Docker.DockerFileOperation;

namespace SiegeTower.K8sExternalAdmin.Images;

public static class Workspace
{
	public const string ImageName = "st-workspace";

	public static void Build()
	{
		var workspaceDist = Path.Combine(FindRepositoryRoot(), "packages", "SiegeTower.WorkspaceHarness", "dist");

		DockerService.Build(
		[
			new From("ubuntu:24.04"),
			new Run("apt-get update && apt-get install -y ca-certificates git wget && wget -q https://packages.microsoft.com/config/ubuntu/24.04/packages-microsoft-prod.deb -O /tmp/packages-microsoft-prod.deb && dpkg -i /tmp/packages-microsoft-prod.deb && rm /tmp/packages-microsoft-prod.deb && apt-get update && apt-get install -y aspnetcore-runtime-10.0 && rm -rf /var/lib/apt/lists/*"),
			new Run("mkdir -p /var/siegetower/workspace"),
			new Run("mkdir -p /var/workspace && printf '%s' 'hello world' > /var/workspace/temp.txt && chmod -R 777 /var/workspace"),
			new Copy("workspace", "/var/siegetower/workspace"),
			new Workdir("/var/siegetower/workspace"),
			new Expose(80),
			new Cmd("Workspace__Root=/var/workspace ASPNETCORE_URLS=http://+:80 dotnet /var/siegetower/workspace/SiegeTower.WorkspaceHarness.dll")
		],
		[ImageName],
		new Dictionary<string, string>
		{
			[workspaceDist] = "workspace"
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
