using k8s;
using k8s.Autorest;
using k8s.Models;

namespace SiegeTower.K8sExternalAdmin;

public sealed class KubernetesService(IKubernetes client)
{
	public async Task PushNginxAsync(string image, string @namespace = "siegetower")
	{
		await EnsureNamespaceAsync(@namespace);

		var labels = new Dictionary<string, string> { ["app"] = "test-nginx" };
		var deployment = new V1Deployment
		{
			Metadata = new V1ObjectMeta { Name = "test-nginx", NamespaceProperty = @namespace },
			Spec = new V1DeploymentSpec
			{
				Replicas = 1,
				Selector = new V1LabelSelector { MatchLabels = labels },
				Template = new V1PodTemplateSpec
				{
					Metadata = new V1ObjectMeta { Labels = labels },
					Spec = new V1PodSpec
					{
						Containers =
						[
							new V1Container
							{
								Name = "nginx",
								Image = image,
								ImagePullPolicy = "Never",
								Ports = [new V1ContainerPort { ContainerPort = 80 }]
							}
						]
					}
				}
			}
		};

		try
		{
			await client.AppsV1.ReadNamespacedDeploymentAsync(deployment.Metadata.Name, @namespace);
			await client.AppsV1.ReplaceNamespacedDeploymentAsync(deployment, deployment.Metadata.Name, @namespace);
		}
		catch (HttpOperationException exception) when (exception.Response.StatusCode == System.Net.HttpStatusCode.NotFound)
		{
			await client.AppsV1.CreateNamespacedDeploymentAsync(deployment, @namespace);
		}

		var service = new V1Service
		{
			Metadata = new V1ObjectMeta { Name = "test-nginx", NamespaceProperty = @namespace },
			Spec = new V1ServiceSpec
			{
				Type = "NodePort",
				Selector = labels,
				Ports = [new V1ServicePort { Name = "http", Port = 5006, TargetPort = 80, NodePort = 30006 }]
			}
		};

		try
		{
			await client.CoreV1.ReadNamespacedServiceAsync(service.Metadata.Name, @namespace);
			await client.CoreV1.ReplaceNamespacedServiceAsync(service, service.Metadata.Name, @namespace);
		}
		catch (HttpOperationException exception) when (exception.Response.StatusCode == System.Net.HttpStatusCode.NotFound)
		{
			await client.CoreV1.CreateNamespacedServiceAsync(service, @namespace);
		}
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