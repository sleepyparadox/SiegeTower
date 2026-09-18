public sealed class GridControl : Component, IControlComponent, IParentOf<GridCellControl>, IParentOf<GridCellLayoutControl>
{
	ComponentRefList<GridCellControl> IParentOf<GridCellControl>.Children { get; set; } = new();
	ComponentRefList<GridCellLayoutControl> IParentOf<GridCellLayoutControl>.Children { get; set; } = new();

	public int ColumnCount { get; }

	public GridControl(Entity entity, int columnCount) : base(entity)
	{
		ArgumentOutOfRangeException.ThrowIfNegativeOrZero(columnCount);
		ColumnCount = columnCount;
	}
}