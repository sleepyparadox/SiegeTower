using SiegeTower.Data;
using SiegeTower.Api;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
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
builder.Services.AddHttpClient("GitHub", client =>
{
	client.BaseAddress = new Uri("https://api.github.com/");
	client.Timeout = TimeSpan.FromMinutes(2);
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

app.MapPost("api/github-access-token", async (GithubAccessTokenRequest request, IHttpClientFactory httpClientFactory, CancellationToken cancellationToken) =>
{
	if (string.IsNullOrWhiteSpace(request.AppId)
		|| string.IsNullOrWhiteSpace(request.InstallationId)
		|| string.IsNullOrWhiteSpace(request.PrivateKey))
	{
		return Results.BadRequest("GitHub App ID, installation ID, and private key are required.");
	}

	using var requestMessage = new HttpRequestMessage(HttpMethod.Post, $"app/installations/{Uri.EscapeDataString(request.InstallationId)}/access_tokens");
	requestMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));
	requestMessage.Headers.UserAgent.ParseAdd("SiegeTower");
	requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", CreateAppJwt(request.AppId, request.PrivateKey));

	using var response = await httpClientFactory.CreateClient("GitHub").SendAsync(requestMessage, cancellationToken);
	response.EnsureSuccessStatusCode();
	return Results.Ok(await response.Content.ReadFromJsonAsync<GithubAccessToken>(cancellationToken: cancellationToken)
		?? throw new InvalidOperationException("GitHub returned an empty access token response."));
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
	var podName = $"st-workspace-{request.Name}";

	var labels = new Dictionary<string, string> { ["app"] = request.Name };
	var podResource = new V1Pod
	{
		Metadata = new V1ObjectMeta
		{
			Name = podName,
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
		Metadata = new V1ObjectMeta { Name = podName, NamespaceProperty = workspaceNamespace },
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
			await client.CoreV1.DeleteNamespacedPodAsync(podName, workspaceNamespace, cancellationToken: cancellationToken);
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
	var podName = $"st-workspace-{name}";

	var found = false;
	try
	{
		await client.CoreV1.DeleteNamespacedServiceAsync(podName, workspaceNamespace, cancellationToken: cancellationToken);
		found = true;
	}
	catch (k8s.Autorest.HttpOperationException exception) when (exception.Response.StatusCode == System.Net.HttpStatusCode.NotFound)
	{
	}

	try
	{
		await client.CoreV1.DeleteNamespacedPodAsync(podName, workspaceNamespace, cancellationToken: cancellationToken);
		found = true;
	}
	catch (k8s.Autorest.HttpOperationException exception) when (exception.Response.StatusCode == System.Net.HttpStatusCode.NotFound)
	{
	}

	return found ? Results.NoContent() : Results.NotFound();
});

app.MapDelete("api/workspace-all", async (IKubernetes client, CancellationToken cancellationToken) =>
{
	var services = await client.CoreV1.ListNamespacedServiceAsync(workspaceNamespace, cancellationToken: cancellationToken);
	foreach (var service in services.Items)
	{
		await client.CoreV1.DeleteNamespacedServiceAsync(service.Metadata.Name, workspaceNamespace, cancellationToken: cancellationToken);
	}

	var pods = await client.CoreV1.ListNamespacedPodAsync(workspaceNamespace, cancellationToken: cancellationToken);
	foreach (var pod in pods.Items)
	{
		await client.CoreV1.DeleteNamespacedPodAsync(pod.Metadata.Name, workspaceNamespace, cancellationToken: cancellationToken);
	}

	return Results.NoContent();
});

app.Run();

static string CreateAppJwt(string appId, string privateKey)
{
	var issuedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
	var header = Encode(new { alg = "RS256", typ = "JWT" });
	var payload = Encode(new { iat = issuedAt - 60, exp = issuedAt + 540, iss = appId });
	var unsignedToken = $"{header}.{payload}";
	using var rsa = RSA.Create();
	rsa.ImportFromPem(privateKey);
	var signature = rsa.SignData(Encoding.UTF8.GetBytes(unsignedToken), HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
	return $"{unsignedToken}.{Base64UrlEncode(signature)}";
}

static string Encode(object value) => Base64UrlEncode(Encoding.UTF8.GetBytes(System.Text.Json.JsonSerializer.Serialize(value)));

static string Base64UrlEncode(byte[] value) => Convert.ToBase64String(value).TrimEnd('=').Replace('+', '-').Replace('/', '_');

public sealed record CreateWorkspaceRequest(string Name);
public sealed record CreateTaskRequest(string Name, string? Description);