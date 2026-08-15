namespace SiegeTower.Client;

public sealed class SessionContext
{
	public string BaseUri { get; init; } = string.Empty;

	public string ApiBaseUri { get; init; } = string.Empty;

	public IDictionary<string, string> Values { get; } = new Dictionary<string, string>();
}
