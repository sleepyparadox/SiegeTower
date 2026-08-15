namespace SiegeTower.Client.Screens.Common;

public sealed class BreadCrumb
{
	public BreadCrumb()
	{
	}

	public BreadCrumb(string text, string uri)
	{
		Text = text;
		Uri = uri;
	}	

	public string Text { get; init; } = string.Empty;

	public string Uri { get; init; } = string.Empty;
}
