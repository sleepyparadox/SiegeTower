public sealed class TitleLayout : ScreenLayoutChild, IParentOf<BreadCrumb>
{
	public string Title { get; set; }
	public ComponentRefList<BreadCrumb> Children { get; set; } = new();

	public TitleLayout(Entity entity, string title) : base(entity)
	{
		Title = title;
	}
}