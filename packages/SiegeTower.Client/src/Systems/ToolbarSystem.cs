namespace SiegeTower.Client;

public static class ToolbarSystem
{
	public static Toolbar AddToolbar(this ToolbarLayout layout, int rowIndex)
	{
		ArgumentNullException.ThrowIfNull(layout);

		var entity = layout.Entity.EntityStorage.NewEntity();
		var toolbar = entity.AddComponent(e => new Toolbar(e, rowIndex));
		layout.AttachChildren(toolbar);
		return toolbar;
	}

	public static ToolbarControl AddToolbarControl<TControl>(this Toolbar toolbar, Func<Entity, TControl> createControl)
		where TControl : Component
	{
		ArgumentNullException.ThrowIfNull(toolbar);
		ArgumentNullException.ThrowIfNull(createControl);

		var entity = toolbar.Entity.EntityStorage.NewEntity();
		var toolbarControl = entity.AddComponent<ToolbarControl>();
		var control = entity.AddComponent(createControl);
		toolbar.AttachChildren(toolbarControl);
		return toolbarControl;
	}

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
			case ToolbarRowDragTargetChanged changed:
				SetRowTarget(session.ActiveScreen, changed.Layout, changed.RowIndex, changed.Position);
				break;
			case ToolbarRowDragStopped stopped:
				SetRowTarget(session.ActiveScreen, stopped.Layout, stopped.RowIndex, stopped.Position);
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
		operation.TargetRowIndex = null;
		operation.TargetPosition = position;
	}

	static void SetRowTarget(Screen screen, ToolbarLayout layout, int rowIndex, ToolbarDropPosition position)
	{
		var operation = Operation(screen);
		if (operation is null || !screen.SelectComponents<ToolbarLayout>().Contains(layout) || position is not (ToolbarDropPosition.Top or ToolbarDropPosition.Bottom))
		{
			return;
		}

		operation.TargetToolbar.Clear();
		operation.TargetRowIndex = rowIndex;
		operation.TargetPosition = position;
	}

	static void Complete(Screen screen)
	{
		var operation = Operation(screen);
		var toolbar = operation?.Toolbar.Get();
		var targetToolbar = operation?.TargetToolbar.Get();
		if (operation is null || toolbar is null || operation.TargetPosition is null)
		{
			ClearOperation(screen);
			return;
		}

		var layout = FindLayout(screen, toolbar);
		if (layout is null)
		{
			ClearOperation(screen);
			return;
		}

		switch (operation.TargetPosition)
		{
			case ToolbarDropPosition.Left:
			case ToolbarDropPosition.Right:
				if (targetToolbar is null || layout != FindLayout(screen, targetToolbar) || toolbar == targetToolbar)
				{
					break;
				}

				layout.DetachChild(toolbar);
				var targetIndex = layout.Children.IndexOf(targetToolbar);
				toolbar.RowIndex = targetToolbar.RowIndex;
				layout.Children.Insert(operation.TargetPosition == ToolbarDropPosition.Left ? targetIndex : targetIndex + 1, toolbar);
				((IChildOf<ToolbarLayout>)toolbar).Parent = layout;
				break;
			case ToolbarDropPosition.Top:
			case ToolbarDropPosition.Bottom:
				if (operation.TargetRowIndex is not int targetRowIndex)
				{
					break;
				}

				var insertionRowIndex = operation.TargetPosition == ToolbarDropPosition.Top ? targetRowIndex : targetRowIndex + 1;
				var sourceRowIndex = toolbar.RowIndex;
				var sourceRowIsEmpty = layout.Children.Values.Count(existingToolbar => existingToolbar.RowIndex == sourceRowIndex) == 1;
				layout.DetachChild(toolbar);
				if (sourceRowIsEmpty && sourceRowIndex < insertionRowIndex)
				{
					insertionRowIndex--;
				}

				NormalizeRows(layout);
				InsertRow(layout, toolbar, insertionRowIndex);
				((IChildOf<ToolbarLayout>)toolbar).Parent = layout;
				break;
		}

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