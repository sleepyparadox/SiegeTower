public sealed class ComboBoxControl : Component, IControlComponent
{
	public string Value { get; set; }

	public ComboBoxControl(Entity entity, string value) : base(entity)
	{
		Value = value;
	}
}