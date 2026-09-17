public sealed class ButtonControl : Component, IControlComponent
{
	public string Text { get; set; }
	public string? Uri { get; set; }

	public ButtonControl(Entity entity, string text, string? uri = null) : base(entity)
	{
		Text = text;
		Uri = uri;
	}
}