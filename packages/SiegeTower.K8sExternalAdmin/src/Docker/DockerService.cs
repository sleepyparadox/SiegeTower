using SiegeTower.K8sExternalAdmin.Docker.DockerFileOperation;

namespace SiegeTower.K8sExternalAdmin.Docker;

public sealed class DockerService
{
	public void Build(IDockerFileOperation[] operations, string[] tags)
	{
		if (tags.Length == 0)
		{
			throw new ArgumentException("At least one Docker image tag is required.", nameof(tags));
		}

		var context = Path.Combine(Path.GetTempPath(), $"siegetower-docker-{Guid.NewGuid():N}");
		Directory.CreateDirectory(context);

		try
		{
			var dockerFile = string.Join(Environment.NewLine, operations.Select(operation => operation.ToString()));
			LogService.Info($"Generated Dockerfile with {operations.Length} operation(s).");
			File.WriteAllText(Path.Combine(context, "Dockerfile"), dockerFile + Environment.NewLine);
			var arguments = new List<string> { "build" };
			foreach (var tag in tags)
			{
				arguments.Add("--tag");
				arguments.Add(tag);
			}

			arguments.Add(context);
			CommandRunner.Run("docker", arguments);
		}
		finally
		{
			Directory.Delete(context, recursive: true);
		}
	}
}