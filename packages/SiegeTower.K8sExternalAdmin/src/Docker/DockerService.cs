using SiegeTower.K8sExternalAdmin.Docker.DockerFileOperation;

namespace SiegeTower.K8sExternalAdmin.Docker;

public sealed class DockerService
{
	public void Build(IDockerFileOperation[] operations, string[] tags, IReadOnlyDictionary<string, string>? contextDirectories = null)
	{
		if (tags.Length == 0)
		{
			throw new ArgumentException("At least one Docker image tag is required.", nameof(tags));
		}

		var context = Path.Combine(Path.GetTempPath(), $"siegetower-docker-{Guid.NewGuid():N}");
		Directory.CreateDirectory(context);

		try
		{
			if (contextDirectories is not null)
			{
				foreach (var (source, destination) in contextDirectories)
				{
					CopyDirectory(source, Path.Combine(context, destination));
				}
			}

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

	private static void CopyDirectory(string source, string destination)
	{
		if (!Directory.Exists(source))
		{
			throw new DirectoryNotFoundException($"Docker build context directory '{source}' was not found.");
		}

		Directory.CreateDirectory(destination);

		foreach (var directory in Directory.GetDirectories(source, "*", SearchOption.AllDirectories))
		{
			Directory.CreateDirectory(Path.Combine(destination, Path.GetRelativePath(source, directory)));
		}

		foreach (var file in Directory.GetFiles(source, "*", SearchOption.AllDirectories))
		{
			var target = Path.Combine(destination, Path.GetRelativePath(source, file));
			Directory.CreateDirectory(Path.GetDirectoryName(target)!);
			File.Copy(file, target);
		}
	}
}