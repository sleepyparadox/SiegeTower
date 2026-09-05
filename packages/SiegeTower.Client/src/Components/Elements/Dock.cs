public class Dock : Component, IRequires<Element>
{
	public string Region { get; set; }
	public string Title { get; set; }

	public Dock(Entity entity, string region, string title = "Dock") : base(entity)
	{
		Region = region;
		Title = title;
	}
}