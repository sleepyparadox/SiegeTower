public class MenuItemIcon : Component, IRequires<Element>
{
	public string Icon { get; set; }

	public MenuItemIcon(Entity entity, string icon) : base(entity)
	{
		Icon = icon;
	}
}