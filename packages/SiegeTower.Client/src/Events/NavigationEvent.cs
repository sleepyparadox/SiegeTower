namespace SiegeTower.Client;

public sealed class NavigationEvent : SessionEvent
{
	public string Uri { get; }
	public Hyperlink? Hyperlink { get; }

	public NavigationEvent(Hyperlink hyperlink, EventArgs? userInteractionEvent = null)
		: base(userInteractionEvent)
	{
		ArgumentNullException.ThrowIfNull(hyperlink);
		Hyperlink = hyperlink;
		Uri = hyperlink.Uri;
	}

	public NavigationEvent(string uri, EventArgs? userInteractionEvent = null)
		: base(userInteractionEvent)
	{
		ArgumentException.ThrowIfNullOrEmpty(uri);
		Uri = uri;
	}
}