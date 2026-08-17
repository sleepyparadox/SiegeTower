using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Serialization;
using SiegeTower.Data;

namespace SiegeTower.WorkspaceHarness.Services;

public sealed class GitHubService
{
	readonly HttpClient httpClient;

	public GitHubService(HttpClient httpClient)
	{
		this.httpClient = httpClient;
	}

	public async Task<GitHubAccessToken> CreateAccessTokenAsync(GithubAccessTokenRequest request, CancellationToken cancellationToken = default)
	{
		ArgumentNullException.ThrowIfNull(request);
		ArgumentException.ThrowIfNullOrWhiteSpace(request.AppId);
		ArgumentException.ThrowIfNullOrWhiteSpace(request.InstallationId);
		ArgumentException.ThrowIfNullOrWhiteSpace(request.PrivateKey);

		using var requestMessage = new HttpRequestMessage(HttpMethod.Post, $"app/installations/{Uri.EscapeDataString(request.InstallationId)}/access_tokens");
		requestMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));
		requestMessage.Headers.UserAgent.ParseAdd("SiegeTower");
		requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", CreateAppJwt(request.AppId, request.PrivateKey));

		using var response = await httpClient.SendAsync(requestMessage, cancellationToken);
		response.EnsureSuccessStatusCode();
		return await response.Content.ReadFromJsonAsync<GitHubAccessToken>(cancellationToken: cancellationToken)
			?? throw new InvalidOperationException("GitHub returned an empty access token response.");
	}

	string CreateAppJwt(string appId, string privateKey)
	{
		var issuedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
		var header = Encode(new { alg = "RS256", typ = "JWT" });
		var payload = Encode(new { iat = issuedAt - 60, exp = issuedAt + 540, iss = appId });
		var unsignedToken = $"{header}.{payload}";
		using var rsa = RSA.Create();
		rsa.ImportFromPem(privateKey);
		var signature = rsa.SignData(Encoding.UTF8.GetBytes(unsignedToken), HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
		return $"{unsignedToken}.{Base64UrlEncode(signature)}";
	}

	static string Encode(object value)
	{
		return Base64UrlEncode(Encoding.UTF8.GetBytes(System.Text.Json.JsonSerializer.Serialize(value)));
	}

	static string Base64UrlEncode(byte[] value)
	{
		return Convert.ToBase64String(value).TrimEnd('=').Replace('+', '-').Replace('/', '_');
	}
}

public sealed class GitHubAccessToken
{
	public string Token { get; set; } = string.Empty;

	[JsonPropertyName("expires_at")]
	public DateTime ExpiresAt { get; set; }
}