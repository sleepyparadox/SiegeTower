public sealed class Toolbar : Component, IChildOf<ToolbarLayout>
{
	public int RowIndex { get; set; }
	public ComponentRef<ToolbarLayout> Parent { get; set; } = new();

	public Toolbar(Entity entity, int rowIndex)
		: base(entity)
	{
		RowIndex = rowIndex;
	}
}