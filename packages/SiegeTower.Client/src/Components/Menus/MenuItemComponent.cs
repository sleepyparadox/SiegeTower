public class MenuItemComponent : Component, IRequires<Element>, IRequires<Hyperlink>
{
	public string Text { get; }

	public string Icon { get; }

	public MenuItemComponent(Entity entity, string text, string icon)
		: base(entity)
	{
		ArgumentNullException.ThrowIfNull(text);
		ArgumentNullException.ThrowIfNull(icon);
		Text = text;
		Icon = icon;
	}
}