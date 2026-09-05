public class TabLabel : Component, IRequires<Element>
{
	public string Text { get; set; }

	public TabLabel(Entity entity, string text) : base(entity)
	{
		Text = text;
	}
}