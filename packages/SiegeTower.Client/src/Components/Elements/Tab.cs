public class Tab : Component, IRequires<Element>, IRequires<Hyperlink>
{
	public string Text { get; set; }

	public Tab(Entity entity, string text) : base(entity)
	{
		Text = text;
	}
}