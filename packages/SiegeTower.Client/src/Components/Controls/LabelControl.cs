public sealed class LabelControl : Component, IControlComponent
{
	public string Value { get; set; }

	public LabelControl(Entity entity, string value) : base(entity)
	{
		Value = value;
	}
}