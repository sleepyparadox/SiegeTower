public class BreadCrumb : Component, IRequires<Element>
{
	public string Text { get; set; }

	public int Index { get; set; }

	public BreadCrumb(Entity entity, string text, int index = 0)
		: base(entity)
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
		var entity = storage.NewEntity();
		entity.AddComponent(e => new Element(e, $"breadcrumb-{index}"));
		var breadCrumb = entity.AddComponent(e => new BreadCrumb(e, text, index));
		breadCrumb.Entity.AddComponent(e => new Hyperlink(e, uri, uriIsInternal));
		return breadCrumb;
	}
}