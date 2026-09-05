namespace SiegeTower.Client;

public static class ToolbarSystem
{
	public static void HandleEvent(Session session, SessionEvent sessionEvent)
	{
		if (sessionEvent.IsCanceled)
		{
			return;
		}

		switch (sessionEvent)
		{
			case ToolbarDragStarted started:
				Start(session.ActiveScreen, started.Toolbar);
				break;
			case ToolbarDragTargetChanged changed:
				SetTarget(session.ActiveScreen, changed.TargetToolbar, changed.Position);
				break;
			case ToolbarDragStopped stopped:
				SetTarget(session.ActiveScreen, stopped.TargetToolbar, stopped.Position);
				Complete(session.ActiveScreen);
				break;
			case ToolbarDragCanceled:
				ClearOperation(session.ActiveScreen);
				break;
		}
	}

	static void Start(Screen screen, Toolbar toolbar)
	{
		if (FindLayout(screen, toolbar) is null)
		{
			return;
		}

		ClearOperation(screen);
		screen.NewEntity().AddComponent(entity => new ToolbarDragOperation(entity, toolbar));
	}

	static void SetTarget(Screen screen, Toolbar targetToolbar, ToolbarDropPosition position)
	{
		var operation = Operation(screen);
		if (operation is null || FindLayout(screen, targetToolbar) is null)
		{
			return;
		}

		operation.TargetToolbar = targetToolbar;
		operation.TargetPosition = position;
	}

	static void Complete(Screen screen)
	{
		var operation = Operation(screen);
		var toolbar = operation?.Toolbar.Get();
		var targetToolbar = operation?.TargetToolbar.Get();
		if (operation is null || toolbar is null || targetToolbar is null || operation.TargetPosition is null)
		{
			ClearOperation(screen);
			return;
		}

		var layout = FindLayout(screen, toolbar);
		if (layout is null || layout != FindLayout(screen, targetToolbar) || toolbar == targetToolbar)
		{
			ClearOperation(screen);
			return;
		}

		layout.DetachChild(toolbar);
		var targetIndex = layout.Children.IndexOf(targetToolbar);
		switch (operation.TargetPosition)
		{
			case ToolbarDropPosition.Left:
				toolbar.RowIndex = targetToolbar.RowIndex;
				layout.Children.Insert(targetIndex, toolbar);
				break;
			case ToolbarDropPosition.Right:
				toolbar.RowIndex = targetToolbar.RowIndex;
				layout.Children.Insert(targetIndex + 1, toolbar);
				break;
			case ToolbarDropPosition.Top:
				InsertRow(layout, toolbar, targetToolbar.RowIndex);
				break;
			case ToolbarDropPosition.Bottom:
				InsertRow(layout, toolbar, targetToolbar.RowIndex + 1);
				break;
		}

		((IChildOf<ToolbarLayout>)toolbar).Parent = layout;
		NormalizeRows(layout);
		ClearOperation(screen);
	}

	static void InsertRow(ToolbarLayout layout, Toolbar toolbar, int rowIndex)
	{
		foreach (var existingToolbar in layout.Children.Values.Where(existingToolbar => existingToolbar.RowIndex >= rowIndex))
		{
			existingToolbar.RowIndex++;
		}

		toolbar.RowIndex = rowIndex;
		var insertionIndex = layout.Children.Values.TakeWhile(existingToolbar => existingToolbar.RowIndex < rowIndex).Count();
		layout.Children.Insert(insertionIndex, toolbar);
	}

	static void NormalizeRows(ToolbarLayout layout)
	{
		var rowIndex = 0;
		foreach (var row in layout.Children.Values.GroupBy(toolbar => toolbar.RowIndex))
		{
			foreach (var toolbar in row)
			{
				toolbar.RowIndex = rowIndex;
			}
			rowIndex++;
		}
	}

	static ToolbarLayout? FindLayout(Screen screen, Toolbar toolbar)
		=> screen.SelectComponents<ToolbarLayout>().SingleOrDefault(layout => layout.Children.Values.Contains(toolbar));

	static ToolbarDragOperation? Operation(Screen screen)
		=> screen.SelectComponents<ToolbarDragOperation>().SingleOrDefault();

	static void ClearOperation(Screen screen)
	{
		Operation(screen)?.Entity.TryDeleteEntity();
	}
}