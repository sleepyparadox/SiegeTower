public class ToolbarLabel : Component, IRequires<Element>
{
	public string Text { get; set; }

	public ToolbarLabel(Entity entity, string text) : base(entity)
	{
		Text = text;
	}
}