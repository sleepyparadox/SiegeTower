using k8s;
using SiegeTower.K8sExternalAdmin.Docker;
using SiegeTower.K8sExternalAdmin.Images;

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

			if (args is ["logs", var pod])
			{
				await LogsAsync(pod);
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

	static async Task PushAsync()
	{
		LogService.Info($"Building Docker image '{LoadBalancer.ImageName}'.");
		LoadBalancer.Build();
		LogService.Info($"Building Docker image '{Api.ImageName}'.");
		Api.Build();
		LogService.Info($"Building Docker image '{Workspace.ImageName}'.");
		Workspace.Build();
		LogService.Info($"Building Docker image '{Ollama.ImageName}'.");
		Ollama.Build();

		var config = KubernetesClientConfiguration.BuildConfigFromConfigFile();
		if (!config.CurrentContext.StartsWith("kind-", StringComparison.Ordinal))
		{
			throw new InvalidOperationException($"Current context '{config.CurrentContext}' is not a kind cluster.");
		}

		var cluster = config.CurrentContext["kind-".Length..];
		LogService.Info($"Loading SiegeTower images into Kind cluster '{cluster}'.");
		KindService.Load(cluster, [LoadBalancer.ImageName, Api.ImageName, Workspace.ImageName, Ollama.ImageName]);

		LogService.Info("Applying SiegeTower Deployments and Services to Kubernetes.");
		await KubernetesService.PushAsync(new Kubernetes(config), LoadBalancer.ImageName, Api.ImageName, Workspace.ImageName, Ollama.ImageName);
		LogService.Info("SiegeTower is available at http://localhost:5006/ when the Kind host port mapping is configured.");
	}

	static void PrintCurrentContext()
	{
		var config = KubernetesClientConfiguration.BuildConfigFromConfigFile();
		Console.WriteLine(config.CurrentContext);
		LogService.Info($"Current Kubernetes context: {config.CurrentContext}");
	}

	static async Task GetAsync(string resource)
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

	static async Task LogsAsync(string pod, string @namespace = "siegetower")
	{
		if (string.IsNullOrWhiteSpace(pod))
		{
			throw new ArgumentException("A pod name is required.", nameof(pod));
		}

		var config = KubernetesClientConfiguration.BuildConfigFromConfigFile();
		var client = new Kubernetes(config);
		await using var logStream = await client.CoreV1.ReadNamespacedPodLogAsync(
			name: pod,
			namespaceParameter: @namespace,
			follow: false);
		using var reader = new StreamReader(logStream);
		Console.Write(await reader.ReadToEndAsync());
	}

	static void PrintUsage()
	{
		Console.Error.WriteLine("Usage: siegetower-k8s-external-admin push");
		Console.Error.WriteLine("       siegetower-k8s-external-admin config current-context");
		Console.Error.WriteLine("       siegetower-k8s-external-admin get <pods|services|deployments|namespaces>");
		Console.Error.WriteLine("       siegetower-k8s-external-admin logs <pod>");
	}
}