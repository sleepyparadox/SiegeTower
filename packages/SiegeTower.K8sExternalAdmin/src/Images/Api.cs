using SiegeTower.K8sExternalAdmin.Docker;
using SiegeTower.K8sExternalAdmin.Docker.DockerFileOperation;

namespace SiegeTower.K8sExternalAdmin.Images;

public static class Api
{
	public const string ImageName = "st-api";

	public static void Build()
	{
		var apiDist = Path.Combine(FindRepositoryRoot(), "packages", "SiegeTower.Api", "dist");

		DockerService.Build(
		[
			new From("ubuntu:24.04"),
			new Run("apt-get update && apt-get install -y ca-certificates wget && wget -q https://packages.microsoft.com/config/ubuntu/24.04/packages-microsoft-prod.deb -O /tmp/packages-microsoft-prod.deb && dpkg -i /tmp/packages-microsoft-prod.deb && rm /tmp/packages-microsoft-prod.deb && apt-get update && apt-get install -y aspnetcore-runtime-10.0 && rm -rf /var/lib/apt/lists/*"),
			new Run("mkdir -p /var/siegetower/api"),
			new Copy("api", "/var/siegetower/api"),
			new Workdir("/var/siegetower/api"),
			new Expose(80),
			new Cmd("ASPNETCORE_URLS=http://+:80 dotnet /var/siegetower/api/SiegeTower.Api.dll")
		],
		[ImageName],
		new Dictionary<string, string>
		{
			[apiDist] = "api"
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