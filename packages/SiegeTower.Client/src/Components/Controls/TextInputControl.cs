public sealed class TextInputControl : Component, IControlComponent
{
	public string Value { get; set; }

	public TextInputControl(Entity entity, string value) : base(entity)
	{
		Value = value;
	}
}