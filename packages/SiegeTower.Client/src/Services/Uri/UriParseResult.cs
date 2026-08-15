namespace SiegeTower.Client.Services.Uri;

public sealed record UriParseResult(string[] PathParts, Dictionary<string, string> Args);
