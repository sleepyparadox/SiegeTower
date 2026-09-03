public class MenuItemLabel : Component, IRequires<Element>
{
	public string Text { get; set; }

	public MenuItemLabel(Entity entity, string text) : base(entity)
	{
		Text = text;
	}
}