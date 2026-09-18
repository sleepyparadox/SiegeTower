public record Entity
{
	public EntityStorage EntityStorage { get; set; }

	public Guid ID { get; set; }

	public Entity(EntityStorage entityStorage, Guid id)
	{
		EntityStorage = entityStorage;
		ID = id;
	}
}