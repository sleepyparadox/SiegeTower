using System.Text;
using SiegeTower.K8sExternalAdmin.Docker;
using SiegeTower.K8sExternalAdmin.Docker.DockerFileOperation;

namespace SiegeTower.K8sExternalAdmin.Images;

public static class LoadBalancer
{
	public const string ImageName = "st-load-balancer";

	public static void Build(DockerService docker)
	{
		docker.Build(
		[
			new From("nginx"),
			new Run(WriteFile("/etc/nginx/conf.d/default.conf", Configuration)),
			new Run(WriteFile("/usr/share/nginx/html/index.html", IndexPage))
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

	    location = / {
	        alias /usr/share/nginx/html/index.html;
	    }

	    location = /api {
	        proxy_pass http://st-tower:80/api;
	    }

	    location = /api/ {
	        proxy_pass http://st-tower:80/api/;
	    }

	    location /workspace/1/ {
	        proxy_pass http://st-workspace-1:80/api/;
	    }

	    location /workspace/2/ {
	        proxy_pass http://st-workspace-2:80/api/;
	    }

	    location / {
	        try_files $uri $uri/ /index.html;
	    }
	}
	""";

	private const string IndexPage = """
	<!doctype html>
	<html lang="en">
	<head><meta charset="utf-8"><title>SiegeTower</title></head>
	<body>
		<h1>SiegeTower</h1>
		<ul>
			<li><a href="/api/">Tower API</a></li>
			<li><a href="/workspace/1/api">Workspace 1 API</a></li>
			<li><a href="/workspace/2/api">Workspace 2 API</a></li>
		</ul>
	</body>
	</html>
	""";
}
