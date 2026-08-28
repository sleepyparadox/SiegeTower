public class Component
{
	public EntityStorage EntityStorage { get; set; }

	public Guid EntityID { get; set; }

	public Component(EntityStorage entityStorage, Guid entityID)
	{
		EntityID = entityID;
		EntityStorage = entityStorage;
	}
}