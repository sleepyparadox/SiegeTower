using k8s;
using k8s.Autorest;
using k8s.Models;

namespace SiegeTower.K8sExternalAdmin;

public sealed class KubernetesService(IKubernetes client)
{
	public async Task PushAsync(
		string loadBalancerImage,
		string apiImage,
		string workspaceImage,
		string ollamaImage,
		string @namespace = "siegetower")
	{
		await EnsureNamespaceAsync(@namespace);
		await EnsureNamespaceAsync("siegetower-workspace");
		await EnsureApiServiceAccountAsync(@namespace);
		await RemoveLegacyApplicationsAsync(@namespace);

		await ApplyApplicationAsync("st-load-balancer", loadBalancerImage, @namespace, nodePort: 30006);
		await ApplyApplicationAsync("st-api", apiImage, @namespace);
		await ApplyApplicationAsync("st-ollama", ollamaImage, @namespace, port: 11434, persistOllamaModels: true);
	}

	private async Task RemoveLegacyApplicationsAsync(string @namespace)
	{
		foreach (var name in new[] { "st-tower", "st-workspace-1", "st-workspace-2", "test-nginx" })
		{
			try
			{
				await client.AppsV1.DeleteNamespacedDeploymentAsync(name, @namespace);
				LogService.Info($"Removed legacy {name} Deployment.");
			}
			catch (HttpOperationException exception) when (exception.Response.StatusCode == System.Net.HttpStatusCode.NotFound)
			{
			}

			try
			{
				await client.CoreV1.DeleteNamespacedServiceAsync(name, @namespace);
				LogService.Info($"Removed legacy {name} Service.");
			}
			catch (HttpOperationException exception) when (exception.Response.StatusCode == System.Net.HttpStatusCode.NotFound)
			{
			}
		}
	}

	private async Task ApplyApplicationAsync(string name, string image, string @namespace, int port = 80, int? nodePort = null, bool persistOllamaModels = false)
	{
		var labels = new Dictionary<string, string> { ["app"] = name };
		var deployment = new V1Deployment
		{
			Metadata = new V1ObjectMeta { Name = name, NamespaceProperty = @namespace },
			Spec = new V1DeploymentSpec
			{
				Replicas = 1,
				Selector = new V1LabelSelector { MatchLabels = labels },
				Template = new V1PodTemplateSpec
				{
					Metadata = new V1ObjectMeta
					{
						Labels = labels,
						Annotations = new Dictionary<string, string>
						{
							["siegetower.dev/restarted-at"] = DateTimeOffset.UtcNow.ToString("O")
						}
					},
					Spec = new V1PodSpec
					{
						ServiceAccountName = name == "st-api" ? "st-api" : null,
						Volumes = persistOllamaModels
							? [new V1Volume
							{
								Name = "ollama-models",
								HostPath = new V1HostPathVolumeSource { Path = "/var/lib/siegetower/ollama-models", Type = "DirectoryOrCreate" }
							}]
							: null,
						Containers =
						[
							new V1Container
							{
								Name = name,
								Image = image,
								ImagePullPolicy = "Never",
								Ports = [new V1ContainerPort { ContainerPort = port }],
								VolumeMounts = persistOllamaModels
									? [new V1VolumeMount { Name = "ollama-models", MountPath = "/root/.ollama" }]
									: null
							}
						]
					}
				}
			}
		};

		try
		{
			var existing = await client.AppsV1.ReadNamespacedDeploymentAsync(name, @namespace);
			deployment.Metadata.ResourceVersion = existing.Metadata.ResourceVersion;
			await client.AppsV1.ReplaceNamespacedDeploymentAsync(deployment, name, @namespace);
		}
		catch (HttpOperationException exception) when (exception.Response.StatusCode == System.Net.HttpStatusCode.NotFound)
		{
			await client.AppsV1.CreateNamespacedDeploymentAsync(deployment, @namespace);
		}

		await WaitForDeploymentAsync(name, @namespace);

		var service = new V1Service
		{
			Metadata = new V1ObjectMeta { Name = name, NamespaceProperty = @namespace },
			Spec = new V1ServiceSpec
			{
				Type = nodePort.HasValue ? "NodePort" : "ClusterIP",
				Selector = labels,
				Ports = [new V1ServicePort { Name = "http", Port = port, TargetPort = port, NodePort = nodePort }]
			}
		};

		try
		{
			var existing = await client.CoreV1.ReadNamespacedServiceAsync(name, @namespace);
			service.Metadata.ResourceVersion = existing.Metadata.ResourceVersion;
			await client.CoreV1.ReplaceNamespacedServiceAsync(service, name, @namespace);
		}
		catch (HttpOperationException exception) when (exception.Response.StatusCode == System.Net.HttpStatusCode.NotFound)
		{
			await client.CoreV1.CreateNamespacedServiceAsync(service, @namespace);
		}

		LogService.Info($"Applied Deployment and Service '{name}'.");
	}

	private async Task EnsureApiServiceAccountAsync(string @namespace)
	{
		const string serviceAccountName = "st-api";

		var serviceAccount = new V1ServiceAccount
		{
			Metadata = new V1ObjectMeta
			{
				Name = serviceAccountName,
				NamespaceProperty = @namespace
			}
		};

		try
		{
			var existing = await client.CoreV1.ReadNamespacedServiceAccountAsync(serviceAccountName, @namespace);
			serviceAccount.Metadata.ResourceVersion = existing.Metadata.ResourceVersion;
			await client.CoreV1.ReplaceNamespacedServiceAccountAsync(serviceAccount, serviceAccountName, @namespace);
		}
		catch (HttpOperationException exception) when (exception.Response.StatusCode == System.Net.HttpStatusCode.NotFound)
		{
			await client.CoreV1.CreateNamespacedServiceAccountAsync(serviceAccount, @namespace);
		}

		var binding = new V1ClusterRoleBinding
		{
			Metadata = new V1ObjectMeta { Name = serviceAccountName },
			RoleRef = new V1RoleRef
			{
				ApiGroup = "rbac.authorization.k8s.io",
				Kind = "ClusterRole",
				Name = "edit"
			},
			Subjects =
			[
				new Rbacv1Subject
				{
					Kind = "ServiceAccount",
					Name = serviceAccountName,
					NamespaceProperty = @namespace
				}
			]
		};

		try
		{
			var existing = await client.RbacAuthorizationV1.ReadClusterRoleBindingAsync(serviceAccountName);
			binding.Metadata.ResourceVersion = existing.Metadata.ResourceVersion;
			await client.RbacAuthorizationV1.ReplaceClusterRoleBindingAsync(binding, serviceAccountName);
		}
		catch (HttpOperationException exception) when (exception.Response.StatusCode == System.Net.HttpStatusCode.NotFound)
		{
			await client.RbacAuthorizationV1.CreateClusterRoleBindingAsync(binding);
		}

		LogService.Info($"Applied ServiceAccount and cluster edit binding '{serviceAccountName}'.");
	}

	private async Task WaitForDeploymentAsync(string name, string @namespace)
	{
		LogService.Info($"Waiting for Deployment '{name}' to become ready.");

		for (var attempt = 0; attempt < 60; attempt++)
		{
			var deployment = await client.AppsV1.ReadNamespacedDeploymentAsync(name, @namespace);
			var status = deployment.Status;
			if (status?.UpdatedReplicas == 1 && status.ReadyReplicas == 1 && status.AvailableReplicas == 1)
			{
				LogService.Info($"Deployment '{name}' is ready.");
				return;
			}

			await Task.Delay(TimeSpan.FromSeconds(1));
		}

		throw new TimeoutException($"Deployment '{name}' did not become ready within 60 seconds.");
	}

	private async Task EnsureNamespaceAsync(string @namespace)
	{
		try
		{
			await client.CoreV1.ReadNamespaceAsync(@namespace);
		}
		catch (HttpOperationException exception) when (exception.Response.StatusCode == System.Net.HttpStatusCode.NotFound)
		{
			LogService.Info($"Creating Kubernetes namespace '{@namespace}'.");
			await client.CoreV1.CreateNamespaceAsync(new V1Namespace
			{
				Metadata = new V1ObjectMeta { Name = @namespace }
			});
		}
	}
}
