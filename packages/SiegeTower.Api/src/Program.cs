using SiegeTower.Data;
using k8s;
using k8s.Models;

const string workspaceNamespace = "siegetower-workspace";

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
	var pods = await client.CoreV1.ListNamespacedPodAsync(workspaceNamespace, cancellationToken: cancellationToken);
	return pods.Items.Select(pod => new Pod(pod.Metadata.Name, pod.Metadata.NamespaceProperty));
});

app.MapPost("api/pod", async (CreatePodRequest request, IKubernetes client, CancellationToken cancellationToken) =>
{
	if (string.IsNullOrWhiteSpace(request.Name) || !request.Name.StartsWith("st-workspace-", StringComparison.Ordinal))
	{
		return Results.BadRequest("Pod name must start with 'st-workspace-'.");
	}

	var labels = new Dictionary<string, string> { ["app"] = request.Name };
	var podResource = new V1Pod
	{
		Metadata = new V1ObjectMeta
		{
			Name = request.Name,
			NamespaceProperty = workspaceNamespace,
			Labels = labels
		},
		Spec = new V1PodSpec
		{
			Containers =
			[
				new V1Container
				{
					Name = request.Name,
					Image = Image.Workspace,
					ImagePullPolicy = "Never",
					Ports = [new V1ContainerPort { ContainerPort = 80 }]
				}
			]
		}
	};

	var service = new V1Service
	{
		Metadata = new V1ObjectMeta { Name = request.Name, NamespaceProperty = workspaceNamespace },
		Spec = new V1ServiceSpec
		{
			Selector = labels,
			Ports = [new V1ServicePort { Name = "http", Port = 80, TargetPort = 80 }]
		}
	};

	try
	{
		await client.CoreV1.CreateNamespacedPodAsync(podResource, workspaceNamespace, cancellationToken: cancellationToken);
		await client.CoreV1.CreateNamespacedServiceAsync(service, workspaceNamespace, cancellationToken: cancellationToken);
	}
	catch
	{
		try
		{
			await client.CoreV1.DeleteNamespacedPodAsync(request.Name, workspaceNamespace, cancellationToken: cancellationToken);
		}
		catch
		{
		}

		throw;
	}

	return Results.Created($"api/pod/{request.Name}", new Pod(request.Name, workspaceNamespace));
});

app.MapDelete("api/workspace/{name}", async (string name, IKubernetes client, CancellationToken cancellationToken) =>
{
	if (!name.StartsWith("st-workspace-", StringComparison.Ordinal))
	{
		return Results.BadRequest("Workspace name must start with 'st-workspace-'.");
	}

	var found = false;
	try
	{
		await client.CoreV1.DeleteNamespacedServiceAsync(name, workspaceNamespace, cancellationToken: cancellationToken);
		found = true;
	}
	catch (k8s.Autorest.HttpOperationException exception) when (exception.Response.StatusCode == System.Net.HttpStatusCode.NotFound)
	{
	}

	try
	{
		await client.CoreV1.DeleteNamespacedPodAsync(name, workspaceNamespace, cancellationToken: cancellationToken);
		found = true;
	}
	catch (k8s.Autorest.HttpOperationException exception) when (exception.Response.StatusCode == System.Net.HttpStatusCode.NotFound)
	{
	}

	return found ? Results.NoContent() : Results.NotFound();
});

app.Run();

public sealed record CreatePodRequest(string Name);