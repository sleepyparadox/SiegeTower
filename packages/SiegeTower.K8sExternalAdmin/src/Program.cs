using k8s;

if (args is ["push"])
{
	Console.WriteLine("ok");
	return;
}

if (args is ["config", "current-context"])
{
	var config = KubernetesClientConfiguration.BuildConfigFromConfigFile();
	Console.WriteLine(config.CurrentContext);
	return;
}

if (args is ["get", var resource])
{
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

	return;
}

Console.Error.WriteLine("Usage: siegetower-k8s-external-admin push");
Console.Error.WriteLine("       siegetower-k8s-external-admin kubectl config current-context");
Console.Error.WriteLine("       siegetower-k8s-external-admin get <pods|services|deployments|namespaces>");
Environment.ExitCode = 1;
