using System.Text;
using SiegeTower.K8sExternalAdmin.Docker;
using SiegeTower.K8sExternalAdmin.Docker.DockerFileOperation;

namespace SiegeTower.K8sExternalAdmin.Images;

public static class Workspace
{
	public const string ImageName = "st-workspace";

	public static void Build(DockerService docker)
	{
		docker.Build(
		[
			new From("nginx"),
			new Run(WriteFile("/etc/nginx/conf.d/default.conf", Configuration))
		],
		[ImageName]);
	}

	private static string WriteFile(string path, string contents)
	{
		var encoded = Convert.ToBase64String(Encoding.UTF8.GetBytes(contents));
		return $"printf '%s' '{encoded}' | base64 -d > {path}";
	}

	private const string Configuration = """
	server {
	    listen 80;
	    location /api {
	        default_type text/plain;
	        return 200 "workspace connected\\n";
	    }
	}
	""";
}
