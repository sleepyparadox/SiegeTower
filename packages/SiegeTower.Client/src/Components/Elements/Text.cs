public class Text : Component, IRequires<Element>
{
	public string Value { get; set; }

	public Text(Entity entity, string value) : base(entity)
	{
		Value = value;
	}
}