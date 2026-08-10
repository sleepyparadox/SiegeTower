using System.Text.Json;

namespace SiegeTower.K8sExternalAdmin.Docker;

public static class KindService
{
	public static void Load(string cluster, string[] tags)
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
			if (IsAlreadyLoaded(cluster, tag))
			{
				LogService.Siege($"Docker image '{tag}' with the same build hash is already loaded in Kind cluster '{cluster}'; skipping load.");
				continue;
			}

			LogService.Info($"Loading Docker image '{tag}' into Kind cluster '{cluster}'.");
			CommandRunner.Run("kind", ["load", "docker-image", tag, "--name", cluster]);
		}
	}

	static bool IsAlreadyLoaded(string cluster, string tag)
	{
		try
		{
			var localBuildHash = GetLocalBuildHash(tag);
			if (string.IsNullOrWhiteSpace(localBuildHash))
			{
				return false;
			}

			var node = CommandRunner.RunAndCapture("kind", ["get", "nodes", "--name", cluster]).Split('\n', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
			if (string.IsNullOrWhiteSpace(node))
			{
				return false;
			}

			var nodeImage = NormalizeTag(tag);
			var imageJson = CommandRunner.RunAndCapture("docker", ["exec", node, "crictl", "inspecti", nodeImage]);
			using var document = JsonDocument.Parse(imageJson);
			var nodeBuildHash = document.RootElement
				.GetProperty("info")
				.GetProperty("imageSpec")
				.GetProperty("config")
				.GetProperty("Labels")
				.GetProperty("siegetower.build-hash")
				.GetString();

			return string.Equals(localBuildHash, nodeBuildHash, StringComparison.Ordinal);
		}
		catch (Exception exception)
		{
			LogService.Info($"Could not compare image '{tag}' with the Kind node: {exception.Message}. Loading it.");
		}

		return false;
	}

	static string? GetLocalBuildHash(string tag)
	{
		var result = CommandRunner.RunAndCapture(
			"docker",
			["image", "inspect", tag, "--format", "{{index .Config.Labels \"siegetower.build-hash\"}}"]).Trim();
		return result is "" or "<no value>" ? null : result;
	}

	static string NormalizeTag(string tag)
	{
		if (tag.Contains('/', StringComparison.Ordinal))
		{
			return tag.Contains(':', StringComparison.Ordinal) ? tag : $"{tag}:latest";
		}

		return $"docker.io/library/{tag}:latest";
	}
}