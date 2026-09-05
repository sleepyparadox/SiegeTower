public sealed class TitleLayout : ScreenLayoutChild
{
	public string Title { get; set; }

	public TitleLayout(Entity entity, string title) : base(entity)
	{
		Title = title;
	}
}