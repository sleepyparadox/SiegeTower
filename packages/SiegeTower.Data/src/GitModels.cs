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