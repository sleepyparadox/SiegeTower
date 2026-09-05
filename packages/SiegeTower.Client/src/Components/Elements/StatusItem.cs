public class StatusItem : Component, IRequires<Element>
{
	public string Text { get; set; }

	public StatusItem(Entity entity, string text) : base(entity)
	{
		Text = text;
	}
}