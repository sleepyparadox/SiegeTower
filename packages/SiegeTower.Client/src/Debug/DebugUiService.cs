using SiegeTower.Client.Services.Uri;

namespace SiegeTower.Client.Debug;

public static class DebugUiService
{
	public static bool IsDebugUrl(string uri)
	{
		ArgumentNullException.ThrowIfNull(uri);
		var parsedUri = UriService.Parse(uri, string.Empty);
		return parsedUri.Args.TryGetValue("debug", out var value)
			&& string.Equals(value, "true", StringComparison.OrdinalIgnoreCase);
	}
}