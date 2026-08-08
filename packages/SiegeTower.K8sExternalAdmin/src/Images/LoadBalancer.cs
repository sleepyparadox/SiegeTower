using System.Text;
using SiegeTower.K8sExternalAdmin.Docker;
using SiegeTower.K8sExternalAdmin.Docker.DockerFileOperation;

namespace SiegeTower.K8sExternalAdmin.Images;

public static class LoadBalancer
{
	public const string ImageName = "st-load-balancer";

	public static void Build(DockerService docker)
	{
		var clientDist = Path.Combine(FindRepositoryRoot(), "packages", "SiegeTower.Client", "dist", "wwwroot");

		docker.Build(
		[
			new From("nginx"),
			new Run("mkdir -p /var/siegetower"),
			new Run("rm -f /var/log/nginx/access.log /var/log/nginx/error.log && touch /var/log/nginx/access.log /var/log/nginx/error.log && chown nginx:nginx /var/log/nginx/access.log /var/log/nginx/error.log && chmod 644 /var/log/nginx/access.log /var/log/nginx/error.log"),
			new Run(CreateWriteFileCommand("/etc/nginx/conf.d/default.conf", Configuration)),
			new Copy("client", "/var/siegetower")
		],
		[ImageName],
		new Dictionary<string, string>
		{
			[clientDist] = "client"
		});
	}

	private static string FindRepositoryRoot()
	{
		var directory = new DirectoryInfo(AppContext.BaseDirectory);
		while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "siegetrain.packages.json")))
		{
			directory = directory.Parent;
		}

		return directory?.FullName ?? throw new DirectoryNotFoundException("Could not locate the SiegeTower repository root.");
	}

	private static string CreateWriteFileCommand(string path, string contents)
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

	    location /api/ {
	        proxy_pass http://st-tower:80;
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
}
