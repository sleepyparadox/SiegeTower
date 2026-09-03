public class ExampleScreenComponent : Component
{
	public ExampleScreenMode Mode { get; }

	public ExampleScreenComponent(Entity entity, ExampleScreenMode mode = ExampleScreenMode.Home) : base(entity)
	{
		Mode = mode;
	}
}