using SiegeTower.Data;
using k8s;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<IKubernetes>(_ =>
{
	var config = KubernetesClientConfiguration.InClusterConfig();
	return new Kubernetes(config);
});

var app = builder.Build();

app.MapGet("api/workspace", () => new[]
{
	new Workspace("1"),
	new Workspace("2")
});

app.MapGet("api/pod", async (IKubernetes client, CancellationToken cancellationToken) =>
{
	var pods = await client.CoreV1.ListPodForAllNamespacesAsync(cancellationToken: cancellationToken);
	return pods.Items.Select(pod => new Pod(pod.Metadata.Name, pod.Metadata.NamespaceProperty));
});

app.Run();