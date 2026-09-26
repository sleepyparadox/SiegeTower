public sealed class GridCellLayoutControl : Component, IControlComponent, IChildOf<GridControl>, IRequires<ControlLayout>
{
	public int Row { get; }
	public int Column { get; }
	public int RowSpan { get; }
	public int ColumnSpan { get; }
	public ComponentRef<GridControl> Parent { get; set; } = new();

	public GridCellLayoutControl(Entity entity, int row, int column, int rowSpan = 1, int columnSpan = 1) : base(entity)
	{
		Row = row;
		Column = column;
		RowSpan = rowSpan;
		ColumnSpan = columnSpan;
	}
}