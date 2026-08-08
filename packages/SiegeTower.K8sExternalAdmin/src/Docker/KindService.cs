namespace SiegeTower.K8sExternalAdmin.Docker;

public sealed class KindService
{
	public void Load(string cluster, string[] tags)
	{
		if (string.IsNullOrWhiteSpace(cluster))
		{
			throw new ArgumentException("A kind cluster name is required.", nameof(cluster));
		}

		if (tags.Length == 0)
		{
			throw new ArgumentException("At least one Docker image tag is required.", nameof(tags));
		}

		foreach (var tag in tags)
		{
			LogService.Info($"Loading Docker image '{tag}' into Kind cluster '{cluster}'.");
			CommandRunner.Run("kind", ["load", "docker-image", tag, "--name", cluster]);
		}
	}
}