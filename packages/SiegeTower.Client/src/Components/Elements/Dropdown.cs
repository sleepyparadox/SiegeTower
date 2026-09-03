public class Dropdown : Component, IRequires<Element>
{
	public bool IsOpen { get; set; }

	public Dropdown(Entity entity) : base(entity) { }

	public void Toggle() => IsOpen = !IsOpen;
}