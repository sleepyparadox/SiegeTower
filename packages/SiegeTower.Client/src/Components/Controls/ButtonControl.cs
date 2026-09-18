using SiegeTower.Client;

public sealed class ButtonControl : Component, IControlComponent
{
	public string Text { get; set; }
	public Action<Session>? Action { get; set; }

	public ButtonControl(Entity entity, string text, Action<Session>? action = null) : base(entity)
	{
		Text = text;
		Action = action;
	}
}