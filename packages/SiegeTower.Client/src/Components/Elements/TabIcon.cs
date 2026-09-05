public class TabIcon : Component, IRequires<Element>
{
	public string Icon { get; set; }

	public TabIcon(Entity entity, string icon) : base(entity)
	{
		Icon = icon;
	}
}