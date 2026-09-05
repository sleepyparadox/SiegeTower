public class BreadCrumb : Component, IChildOf<TitleLayout>
{
	public string Text { get; set; }
	public string Url { get; set; }

	public int Index { get; set; }

	public ComponentRef<TitleLayout> Parent { get; set; } = new();

	public BreadCrumb(Entity entity, string text, string url, int index = 0)
		: base(entity)
	{
		ArgumentException.ThrowIfNullOrEmpty(text);
		ArgumentException.ThrowIfNullOrEmpty(url);
		Text = text;
		Url = url;
		Index = index;
	}
}

public static class BreadCrumbEntity
{
	public static BreadCrumb AddNewBreadCrumbEntity(this EntityStorage storage, TitleLayout titleLayout, string text, string uri, int index)
	{
		ArgumentNullException.ThrowIfNull(titleLayout);

		var entity = titleLayout.Entity.EntityStorage.NewEntity();
		var breadCrumb = entity.AddComponent(e => new BreadCrumb(e, text, uri, index));
		ParentingSystem.AttachParentChild<TitleLayout, BreadCrumb>(titleLayout, breadCrumb);
		return breadCrumb;
	}
}