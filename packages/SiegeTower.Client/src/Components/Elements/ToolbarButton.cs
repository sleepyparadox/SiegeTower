public class ToolbarButton : Component, IRequires<Element>
{
	public string Text { get; set; }

	public ToolbarButton(Entity entity, string text) : base(entity)
	{
		Text = text;
	}
}