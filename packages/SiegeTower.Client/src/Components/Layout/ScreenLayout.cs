public sealed class ScreenLayout : Component, IParentOf<ScreenLayoutChild>
{
	public ComponentRefList<ScreenLayoutChild> Children { get; set; } = new();

	public ScreenLayout(Entity entity) : base(entity)
	{
	}
}

public abstract class ScreenLayoutChild : Component, IChildOf<ScreenLayout>
{
	public ComponentRef<ScreenLayout> Parent { get; set; } = new();

	protected ScreenLayoutChild(Entity entity) : base(entity)
	{
	}
}