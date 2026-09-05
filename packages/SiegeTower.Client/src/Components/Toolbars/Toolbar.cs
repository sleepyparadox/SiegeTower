public sealed class Toolbar : Component, IChildOf<ToolbarLayout>, IParentOf<ToolbarControl>
{
	public int RowIndex { get; set; }
	public ComponentRef<ToolbarLayout> Parent { get; set; } = new();
	public ComponentRefList<ToolbarControl> Children { get; set; } = new();

	public Toolbar(Entity entity, int rowIndex)
		: base(entity)
	{
		RowIndex = rowIndex;
	}
}