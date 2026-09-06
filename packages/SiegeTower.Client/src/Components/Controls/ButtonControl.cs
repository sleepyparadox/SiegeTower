public sealed class ButtonControl : Component, IControlComponent
{
	public string Text { get; set; }

	public ButtonControl(Entity entity, string text) : base(entity)
	{
		Text = text;
	}
}