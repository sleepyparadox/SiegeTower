public class Hyperlink : Component
{
	public string Uri { get; set; }

	public bool IsInternal { get; set; }

	public Hyperlink(Entity entity, string url, bool isInternal)
		: base(entity)
	{
		Uri = url;
		IsInternal = isInternal;
	}
}