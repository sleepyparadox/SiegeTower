public class Subwindow : Component, IRequires<Element>
{
	public string Title { get; set; }

	public Subwindow(Entity entity, string title) : base(entity)
	{
		Title = title;
	}
}