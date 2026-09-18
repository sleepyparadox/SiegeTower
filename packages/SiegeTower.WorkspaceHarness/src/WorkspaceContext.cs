using System.Text.Json;
using SiegeTower.Data.Ollama;
using SiegeTower.Data;

namespace SiegeTower.WorkspaceHarness;

public sealed class WorkspaceContext
{
	private readonly object sync = new();

	public WorkspaceContext(IConfiguration configuration, IHttpClientFactory httpClientFactory)
	{
		Services = new WorkspaceServices(configuration, httpClientFactory);
	}

	public WorkspaceServices Services { get; }

	public TimeSpan PromptTimeout { get; } = TimeSpan.FromMinutes(5);

	public WorkspaceSettings Settings { get; } = new();

	public void UpdateSettings(WorkspaceSettings settings)
	{
		ArgumentNullException.ThrowIfNull(settings);
		lock (sync)
		{
			Settings.GitAccessToken = ParseGitAccessToken(settings.GitAccessToken);
		}
	}

	private static string? ParseGitAccessToken(string? value)
	{
		if (string.IsNullOrWhiteSpace(value))
		{
			return value;
		}

		var trimmedValue = value.Trim();
		if (trimmedValue.StartsWith('{'))
		{
			try
			{
				using var document = JsonDocument.Parse(trimmedValue);
				if (document.RootElement.TryGetProperty("token", out var token)
					&& token.ValueKind == JsonValueKind.String
					&& !string.IsNullOrWhiteSpace(token.GetString()))
				{
					return token.GetString()!.Trim();
				}
			}
			catch (JsonException)
			{
			}
		}

		return trimmedValue;
	}

	public string? GetGitAccessToken()
	{
		lock (sync)
		{
			return Settings.GitAccessToken;
		}
	}

		public sealed class WorkspaceSettings : Data.WorkspaceSettings
		{
		}
}