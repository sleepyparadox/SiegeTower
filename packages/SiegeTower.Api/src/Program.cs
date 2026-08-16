using SiegeTower.Data;
using SiegeTower.Api;
using k8s;
using k8s.Models;
using Npgsql;

const string workspaceNamespace = "siegetower-workspace";

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<IKubernetes>(_ =>
{
	var config = KubernetesClientConfiguration.InClusterConfig();
	return new Kubernetes(config);
});

await DatabaseInitializer.InitializeAsync(builder.Configuration);

var app = builder.Build();

app.MapGet("api/tasks", async (IConfiguration configuration, CancellationToken cancellationToken) =>
{
	await using var connection = DatabaseInitializer.CreateApplicationConnection(configuration);
	await connection.OpenAsync(cancellationToken);
	await using var command = new NpgsqlCommand("SELECT id, name, description FROM task ORDER BY id", connection);
	await using var reader = await command.ExecuteReaderAsync(cancellationToken);

	var tasks = new List<TaskRow>();
	while (await reader.ReadAsync(cancellationToken))
	{
		tasks.Add(new TaskRow(reader.GetGuid(0), reader.GetString(1), reader.GetString(2)));
	}

	return tasks;
});

app.MapPost("api/tasks", async (CreateTaskRequest request, IConfiguration configuration, CancellationToken cancellationToken) =>
{
	if (string.IsNullOrWhiteSpace(request.Name))
	{
		return Results.BadRequest("Task name is required.");
	}

	var task = new TaskRow(Guid.NewGuid(), request.Name.Trim(), request.Description ?? string.Empty);
	await using var connection = DatabaseInitializer.CreateApplicationConnection(configuration);
	await connection.OpenAsync(cancellationToken);
	await using var command = new NpgsqlCommand(
		"INSERT INTO task (id, name, description) VALUES (@id, @name, @description)",
		connection);
	command.Parameters.AddWithValue("id", task.Id);
	command.Parameters.AddWithValue("name", task.Name);
	command.Parameters.AddWithValue("description", task.Description);
	await command.ExecuteNonQueryAsync(cancellationToken);

	return Results.Created($"api/tasks/{task.Id}", task);
});

app.MapGet("api/workspace", async (IKubernetes client, CancellationToken cancellationToken) =>
{
	var pods = await client.CoreV1.ListNamespacedPodAsync(workspaceNamespace, cancellationToken: cancellationToken);
	return pods.Items.Select(pod => new WorkspaceRow(
		pod.Metadata.Name.StartsWith("st-workspace-", StringComparison.Ordinal)
			? pod.Metadata.Name["st-workspace-".Length..]
			: pod.Metadata.Name,
		pod.Metadata.NamespaceProperty));
});

app.MapPost("api/workspace", async (CreateWorkspaceRequest request, IKubernetes client, CancellationToken cancellationToken) =>
{
	if (string.IsNullOrWhiteSpace(request.Name) || !request.Name.StartsWith("st-workspace-", StringComparison.Ordinal))
	{
		return Results.BadRequest("Workspace name must start with 'st-workspace-'.");
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

	return Results.Created($"api/workspace/{request.Name}", new WorkspaceRow(request.Name, workspaceNamespace));
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

public sealed record CreateWorkspaceRequest(string Name);
public sealed record CreateTaskRequest(string Name, string? Description);