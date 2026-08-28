public class Hyperlink : Component
{
	public string Uri { get; set; }

	public bool IsInternal { get; set; }

	public Hyperlink(EntityStorage entityStorage, Guid entityID, string url, bool isInternal) 
		: base(entityStorage, entityID)
	{
		Uri = url;
		IsInternal = isInternal;
	}
}