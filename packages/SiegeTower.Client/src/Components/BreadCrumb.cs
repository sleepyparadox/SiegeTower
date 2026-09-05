public class BreadCrumb : Component, IChildOf<TitleLayout>, IRequires<Hyperlink>
{
	public string Text { get; set; }

	public int Index { get; set; }

	public ComponentRef<TitleLayout> Parent { get; set; } = new();

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
	public static BreadCrumb AddNewBreadCrumbEntity(this EntityStorage storage, TitleLayout titleLayout, string text, string uri, bool uriIsInternal, int index)
	{
		ArgumentNullException.ThrowIfNull(titleLayout);

		var entity = titleLayout.Entity.EntityStorage.NewEntity();
		entity.AddComponent(e => new Hyperlink(e, uri, uriIsInternal));
		var breadCrumb = entity.AddComponent(e => new BreadCrumb(e, text, index));
		ParentingSystem.AttachParentChild<TitleLayout, BreadCrumb>(titleLayout, breadCrumb);
		return breadCrumb;
	}
}