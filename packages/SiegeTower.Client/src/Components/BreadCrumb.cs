public class BreadCrumb : Component
{
	public string Text { get; set; }

	public int Index { get; set; }

	public BreadCrumb(EntityStorage entityStorage, Guid entityID, string text, int index = 0) 
		: base(entityStorage, entityID)
	{
        ArgumentNullException.ThrowIfNull(text);
		Text = text;
		Index = index;
	}
}

public static class BreadCrumbEntity
{
	public static BreadCrumb AddNewBreadCrumbEntity(this EntityStorage storage, string text, string uri, bool uriIsInternal, int index)
	{
		var breadCrumb = storage.AddNewEntityAndComponent((s, e) => new BreadCrumb(s, e, text, index));
		breadCrumb.AddComponent((s, e) => new Hyperlink(s, e, uri, uriIsInternal));
		return breadCrumb;
	}
}