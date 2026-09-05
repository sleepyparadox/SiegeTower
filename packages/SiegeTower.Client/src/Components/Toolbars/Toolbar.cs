public class Toolbar : Component, IRequires<Element>, IChildOf<ToolbarLayout>
{
	public string Title { get; set; }
	public ComponentRef<ToolbarLayout> Parent { get; set; } = new();

	public Toolbar(Entity entity)
		: base(entity)
	{
		Title = "Toolbar";
	}

	public Toolbar(Entity entity, string title)
		: base(entity)
	{
		Title = title;
	}
}