using k8s;
using SiegeTower.K8sExternalAdmin.Docker;
using SiegeTower.K8sExternalAdmin.Docker.DockerFileOperation;

namespace SiegeTower.K8sExternalAdmin;

public sealed class CommandLineApplication
{
	public async Task<int> RunAsync(string[] args)
	{
		try
		{
			if (args is ["push"])
			{
				await PushAsync();
				return 0;
			}

			if (args is ["config", "current-context"])
			{
				PrintCurrentContext();
				return 0;
			}

			if (args is ["get", var resource])
			{
				await GetAsync(resource);
				return 0;
			}

			PrintUsage();
			return 1;
		}
		catch (Exception exception)
		{
			LogService.Error(exception.Message);
			return 1;
		}
	}

	private static async Task PushAsync()
	{
		const string image = "test-nginx";
		LogService.Info($"Building Docker image '{image}'.");
		new DockerService().Build([new From("nginx")], [image]);

		var config = KubernetesClientConfiguration.BuildConfigFromConfigFile();
		if (!config.CurrentContext.StartsWith("kind-", StringComparison.Ordinal))
		{
			throw new InvalidOperationException($"Current context '{config.CurrentContext}' is not a kind cluster.");
		}

		var cluster = config.CurrentContext["kind-".Length..];
		LogService.Info($"Loading image '{image}' into Kind cluster '{cluster}'.");
		new KindService().Load(cluster, [image]);

		LogService.Info("Applying nginx Deployment and Service to Kubernetes.");
		await new KubernetesService(new Kubernetes(config)).PushNginxAsync(image);
		LogService.Info($"Image '{image}' is available at http://localhost:5006/ when the Kind host port mapping is configured; Kubernetes NodePort is 30006.");
	}

	private static void PrintCurrentContext()
	{
		var config = KubernetesClientConfiguration.BuildConfigFromConfigFile();
		Console.WriteLine(config.CurrentContext);
		LogService.Info($"Current Kubernetes context: {config.CurrentContext}");
	}

	private static async Task GetAsync(string resource)
	{
		LogService.Info($"Listing Kubernetes resource '{resource}'.");
		var config = KubernetesClientConfiguration.BuildConfigFromConfigFile();
		var client = new Kubernetes(config);

		var names = resource.ToLowerInvariant() switch
		{
			"pod" or "pods" => (await client.CoreV1.ListPodForAllNamespacesAsync()).Items.Select(item => item.Metadata.Name),
			"service" or "services" => (await client.CoreV1.ListServiceForAllNamespacesAsync()).Items.Select(item => item.Metadata.Name),
			"deployment" or "deployments" => (await client.AppsV1.ListDeploymentForAllNamespacesAsync()).Items.Select(item => item.Metadata.Name),
			"namespace" or "namespaces" or "ns" => (await client.CoreV1.ListNamespaceAsync()).Items.Select(item => item.Metadata.Name),
			_ => throw new ArgumentException($"Unsupported resource '{resource}'. Try pods, services, deployments, or namespaces.")
		};

		Console.WriteLine("NAME");
		foreach (var name in names)
		{
			Console.WriteLine(name);
		}
	}

	private static void PrintUsage()
	{
		Console.Error.WriteLine("Usage: siegetower-k8s-external-admin push");
		Console.Error.WriteLine("       siegetower-k8s-external-admin config current-context");
		Console.Error.WriteLine("       siegetower-k8s-external-admin get <pods|services|deployments|namespaces>");
	}
}