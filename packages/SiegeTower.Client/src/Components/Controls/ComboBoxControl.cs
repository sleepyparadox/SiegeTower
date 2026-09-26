public sealed class ComboBoxControl : Component, IControlComponent
{
	public string Value { get; set; }
	public List<string> Items { get; set; } = [];

	public ComboBoxControl(Entity entity, string value) : base(entity)
	{
		Value = value;
	}
}