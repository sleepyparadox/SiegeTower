public class MenuComponent : Component, IRequires<Element>
{
	public bool IsOpen { get; set; }

	public MenuComponent(Entity entity)
		: base(entity)
	{
	}

	public void Toggle() => IsOpen = !IsOpen;
}