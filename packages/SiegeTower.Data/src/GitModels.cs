using System.Text.Json.Serialization;

namespace SiegeTower.Data;

public sealed record GithubAccessTokenRequest
{
	public string AppId { get; set; } = string.Empty;

	public string InstallationId { get; set; } = string.Empty;

	public string PrivateKey { get; set; } = string.Empty;
}

public sealed record GitStatus
{
	public bool GithubAccessTokenExists { get; set; }
	public DateTime? GithubAccessTokenExpiresAtUtc { get; set; }
}

public sealed record GithubAccessToken
{
	public string Token { get; set; } = string.Empty;

	[JsonPropertyName("expires_at")]
	public DateTime ExpiresAt { get; set; }
}