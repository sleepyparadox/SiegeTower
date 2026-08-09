namespace SiegeTower.Client;

public sealed class SessionContext
{
	public required string ApiBase { get; init; }

	public IDictionary<string, string> Values { get; } = new Dictionary<string, string>();
}