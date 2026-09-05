public class Component
{
	public Entity Entity { get; set; }

	public Component(Entity entity)
	{
		Entity = entity;
	}
}