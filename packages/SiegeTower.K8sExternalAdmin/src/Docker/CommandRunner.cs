using System.Diagnostics;

namespace SiegeTower.K8sExternalAdmin.Docker;

internal static class CommandRunner
{
	public static void Run(string command, IEnumerable<string> arguments)
	{
		var argumentList = arguments.ToArray();
		LogService.Info($"Running '{command} {string.Join(' ', argumentList)}'.");
		using var process = new Process
		{
			StartInfo = new ProcessStartInfo
			{
				FileName = command,
				RedirectStandardError = true,
				RedirectStandardOutput = true,
				UseShellExecute = false,
				CreateNoWindow = true
			}
		};

		foreach (var argument in argumentList)
		{
			process.StartInfo.ArgumentList.Add(argument);
		}

		if (!process.Start())
		{
			throw new InvalidOperationException($"Unable to start '{command}'.");
		}

		var output = process.StandardOutput.ReadToEnd();
		var error = process.StandardError.ReadToEnd();
		process.WaitForExit();

		if (process.ExitCode != 0)
		{
			throw new InvalidOperationException($"'{command}' failed with exit code {process.ExitCode}: {error.Trim()}");
		}

		if (!string.IsNullOrWhiteSpace(output))
		{
			Console.Write(output);
		}

		LogService.Info($"'{command}' completed successfully.");
	}
}