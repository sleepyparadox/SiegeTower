public sealed class GridCellControl : Component, IChildOf<GridControl>
{
	public int Row { get; }
	public int Column { get; }
	public int RowSpan { get; }
	public int ColumnSpan { get; }
	public ComponentRef<GridControl> Parent { get; set; } = new();

	public GridCellControl(Entity entity, int row, int column, int rowSpan = 1, int columnSpan = 1) : base(entity)
	{
		Row = row;
		Column = column;
		RowSpan = rowSpan;
		ColumnSpan = columnSpan;
	}
}