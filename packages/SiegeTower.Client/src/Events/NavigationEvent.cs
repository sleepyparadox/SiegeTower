namespace SiegeTower.Client;

public sealed class NavigationEvent : SessionEvent
{
	public string Uri { get; }
	public BreadCrumb? BreadCrumb { get; }

	public NavigationEvent(BreadCrumb breadCrumb, EventArgs? userInteractionEvent = null)
		: base(userInteractionEvent)
	{
		ArgumentNullException.ThrowIfNull(breadCrumb);
		BreadCrumb = breadCrumb;
		Uri = breadCrumb.Url;
	}

	public NavigationEvent(string uri, EventArgs? userInteractionEvent = null)
		: base(userInteractionEvent)
	{
		ArgumentException.ThrowIfNullOrEmpty(uri);
		Uri = uri;
	}
}