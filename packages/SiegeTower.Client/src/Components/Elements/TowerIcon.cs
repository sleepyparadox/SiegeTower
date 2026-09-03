public class TowerIcon : Component, IRequires<Element>
{
	public string Icon { get; set; }

	public TowerIcon(Entity entity) : this(entity, "fa-solid fa-tower-observation") { }

	public TowerIcon(Entity entity, string icon = "fa-solid fa-tower-observation") : base(entity)
	{
		Icon = icon;
	}
}