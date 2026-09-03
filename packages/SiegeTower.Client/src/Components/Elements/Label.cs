public class Label : Component, IRequires<Element>
{
	public string Value { get; set; }

	public Label(Entity entity, string value) : base(entity)
	{
		Value = value;
	}
}