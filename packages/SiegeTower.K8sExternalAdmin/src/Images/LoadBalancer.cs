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
			new Run("mkdir -p /var/siegetower"),
			new Run("rm -f /var/log/nginx/access.log /var/log/nginx/error.log && touch /var/log/nginx/access.log /var/log/nginx/error.log && chown nginx:nginx /var/log/nginx/access.log /var/log/nginx/error.log && chmod 644 /var/log/nginx/access.log /var/log/nginx/error.log"),
			new Run(WriteFile("/etc/nginx/conf.d/default.conf", Configuration)),
			new Run(WriteFile("/var/siegetower/index.html", IndexPage))
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
	    root /var/siegetower;
	    access_log /var/log/nginx/access.log;
	    error_log /var/log/nginx/error.log notice;

	    location = / {
	        try_files /index.html =404;
	    }

	    location = /api {
	        proxy_pass http://st-tower:80/api;
	    }

	    location = /api/ {
	        proxy_pass http://st-tower:80/api/;
	    }

	    # DEBUG ONLY: expose the nginx access log temporarily.
	    location = /log/nginx/access.log {
	        default_type text/plain;
	        alias /var/log/nginx/access.log;
	    }

	    # DEBUG ONLY: expose the nginx error log temporarily.
	    location = /log/nginx/error.log {
	        default_type text/plain;
	        alias /var/log/nginx/error.log;
	    }

	    location ~ ^/workspace/(?<workspace_id>[0-9]+)(?<workspace_path>/api(?:/.*)?)$ {
	        resolver 10.96.0.10 valid=10s;
	        proxy_pass http://st-workspace-$workspace_id.siegetower.svc.cluster.local:80$workspace_path$is_args$args;
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
			<li><a href="/log/nginx/access.log">Nginx access log</a></li>
			<li><a href="/log/nginx/error.log">Nginx error log</a></li>
		</ul>
	</body>
	</html>
	""";
}
