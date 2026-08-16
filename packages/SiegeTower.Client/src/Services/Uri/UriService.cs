namespace SiegeTower.Client.Services.Uri;

public static class UriService
{
	public static UriParseResult Parse(string uri, string baseUri)
	{
		ArgumentNullException.ThrowIfNull(uri);

		var currentUri = new System.Uri(uri, System.UriKind.RelativeOrAbsolute);
		if (!currentUri.IsAbsoluteUri)
		{
			currentUri = new System.Uri(CreateBaseUri(baseUri), uri.TrimStart('/'));
		}

		var basePath = CreateBaseUri(baseUri).AbsolutePath.TrimEnd('/');
		var path = currentUri.AbsolutePath;
		if (basePath.Length > 0 && path.StartsWith($"{basePath}/", StringComparison.Ordinal))
		{
			path = path[basePath.Length..];
		}
		else if (basePath.Length > 0 && string.Equals(path, basePath, StringComparison.Ordinal))
		{
			path = string.Empty;
		}

		var pathParts = path
			.Split('/', StringSplitOptions.RemoveEmptyEntries)
			.Select(System.Uri.UnescapeDataString)
			.ToArray();
		var args = ParseArgs(currentUri.Query);

		return new UriParseResult(pathParts, args);
	}

	private static System.Uri CreateBaseUri(string baseUri)
	{
		if (string.IsNullOrWhiteSpace(baseUri))
		{
			return new System.Uri("http://localhost/", System.UriKind.Absolute);
		}

		var parsedBaseUri = new System.Uri(baseUri, System.UriKind.RelativeOrAbsolute);
		return parsedBaseUri.IsAbsoluteUri
			? parsedBaseUri
			: new System.Uri($"http://localhost/{baseUri.Trim('/')}/", System.UriKind.Absolute);
	}

	private static Dictionary<string, string> ParseArgs(string query)
	{
		var args = new Dictionary<string, string>(StringComparer.Ordinal);
		foreach (var pair in query.TrimStart('?').Split('&', StringSplitOptions.RemoveEmptyEntries))
		{
			var separatorIndex = pair.IndexOf('=');
			var key = separatorIndex >= 0 ? pair[..separatorIndex] : pair;
			var value = separatorIndex >= 0 ? pair[(separatorIndex + 1)..] : string.Empty;
			if (key.Length == 0)
			{
				continue;
			}

			args[Decode(key)] = Decode(value);
		}

		return args;
	}

	private static string Decode(string value)
	{
		return System.Uri.UnescapeDataString(value.Replace('+', ' '));
	}
}
