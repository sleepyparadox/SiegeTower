using System.Security.Cryptography;
using System.Text;
using SiegeTower.K8sExternalAdmin.Docker.DockerFileOperation;

namespace SiegeTower.K8sExternalAdmin.Docker;

public static class DockerService
{
	private const string BuildHashLabel = "siegetower.build-hash";

	public static void Build(IDockerFileOperation[] operations, string[] tags, IReadOnlyDictionary<string, string>? contextDirectories = null)
	{
		if (tags.Length == 0)
		{
			throw new ArgumentException("At least one Docker image tag is required.", nameof(tags));
		}

		var buildHash = CalculateBuildHash(operations, contextDirectories);
		if (tags.All(tag => GetBuildHash(tag) == buildHash))
		{
			LogService.Siege($"Docker image '{string.Join("', '", tags)}' is up to date; skipping rebuild.");
			return;
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
			var arguments = new List<string> { "build", "--label", $"{BuildHashLabel}={buildHash}" };
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

	static string? GetBuildHash(string tag)
	{
		try
		{
			return CommandRunner.RunAndCapture(
				"docker",
				["image", "inspect", tag, "--format", $"{{{{index .Config.Labels \"{BuildHashLabel}\"}}}}"]).Trim() switch
			{
				"<no value>" or "" => null,
				var hash => hash
			};
		}
		catch (InvalidOperationException)
		{
			return null;
		}
	}

	static string CalculateBuildHash(IDockerFileOperation[] operations, IReadOnlyDictionary<string, string>? contextDirectories)
	{
		using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
		AddText(hash, "operations\n");
		foreach (var operation in operations)
		{
			AddText(hash, operation.ToString());
			AddText(hash, "\n");
		}

		if (contextDirectories is not null)
		{
			foreach (var (source, destination) in contextDirectories.OrderBy(item => item.Key, StringComparer.Ordinal))
			{
				AddText(hash, $"context:{destination}\n");
				foreach (var file in Directory.GetFiles(source, "*", SearchOption.AllDirectories).Order(StringComparer.Ordinal))
				{
					AddText(hash, Path.GetRelativePath(source, file));
					AddText(hash, "\n");
					using var stream = File.OpenRead(file);
					var buffer = new byte[81920];
					int bytesRead;
					while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
					{
						hash.AppendData(buffer, 0, bytesRead);
					}
					AddText(hash, "\n");
				}
			}
		}

		return Convert.ToHexString(hash.GetHashAndReset()).ToLowerInvariant();
	}

	static void AddText(IncrementalHash hash, string value)
	{
		hash.AppendData(Encoding.UTF8.GetBytes(value));
	}

	static void CopyDirectory(string source, string destination)
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