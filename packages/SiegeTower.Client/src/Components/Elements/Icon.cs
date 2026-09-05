public class Icon : Component, IRequires<Element>
{
	public string Class { get; set; }

	public Icon(Entity entity, string @class) : base(entity)
	{
		Class = @class;
	}
}