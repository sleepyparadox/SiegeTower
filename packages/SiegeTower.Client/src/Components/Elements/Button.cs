public class Button : Component, IRequires<Element>
{
	public string Text { get; set; }

	public Button(Entity entity, string text) : base(entity)
	{
		Text = text;
	}
}