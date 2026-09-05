namespace SiegeTower.Client;

public static class DockingSystem
{
	public static void HandleEvent(Session session, SessionEvent sessionEvent)
	{
		ArgumentNullException.ThrowIfNull(session);
		ArgumentNullException.ThrowIfNull(sessionEvent);

		if (sessionEvent.IsCanceled)
		{
			return;
		}

		switch (sessionEvent)
		{
			case DockWindowActivated activated:
				Activate(session.ActiveScreen, activated.Window);
				break;
			case DockWindowDragStarted started:
				Start(session.ActiveScreen, started.Window);
				break;
			case DockWindowDragTargetChanged changed:
				SetTarget(session.ActiveScreen, changed.TargetGroup, changed.Position);
				break;
			case DockWindowDragStopped stopped:
				SetTarget(session.ActiveScreen, stopped.TargetGroup, stopped.Position);
				Complete(session.ActiveScreen);
				break;
			case DockWindowDragCanceled:
				ClearOperation(session.ActiveScreen);
				break;
		}
	}

	static void Activate(Screen screen, DockWindow window)
	{
		var group = FindParentGroup(screen, window);
		if (group is not null)
		{
			group.ActiveWindow = window;
		}
	}

	static void Start(Screen screen, DockWindow window)
	{
		var sourceGroup = FindParentGroup(screen, window);
		if (sourceGroup is null)
		{
			return;
		}

		ClearOperation(screen);
		screen.NewEntity().AddComponent(entity => new DockWindowDragOperation(entity, window, sourceGroup));
	}

	static void SetTarget(Screen screen, DockWindowGroup targetGroup, DockWindowDropPosition position)
	{
		var operation = Operation(screen);
		if (operation is null || !screen.SelectComponents<DockWindowGroup>().Contains(targetGroup))
		{
			return;
		}

		operation.TargetGroup = targetGroup;
		operation.TargetPosition = position;
	}

	static void Complete(Screen screen)
	{
		var operation = Operation(screen);
		var window = operation?.Window.Get();
		var sourceGroup = operation?.SourceGroup.Get();
		var targetGroup = operation?.TargetGroup.Get();
		if (operation is null || window is null || sourceGroup is null || targetGroup is null || operation.TargetPosition is null)
		{
			ClearOperation(screen);
			return;
		}

		if (operation.TargetPosition == DockWindowDropPosition.Center)
		{
			if (sourceGroup != targetGroup)
			{
				MoveToGroup(window, sourceGroup, targetGroup);
			}
		}
		else
		{
			Split(screen, window, sourceGroup, targetGroup, operation.TargetPosition.Value);
		}

		Normalize(screen);
		ClearOperation(screen);
	}

	static void MoveToGroup(DockWindow window, DockWindowGroup sourceGroup, DockWindowGroup targetGroup)
	{
		sourceGroup.DetachChild(window);
		targetGroup.AttachChild(window);
		targetGroup.ActiveWindow = window;
		if (sourceGroup.ActiveWindow.Get() == window)
		{
			var nextActiveWindow = sourceGroup.Children.Values.LastOrDefault();
			if (nextActiveWindow is null)
			{
				ClearActiveWindow(sourceGroup);
			}
			else
			{
				sourceGroup.ActiveWindow = nextActiveWindow;
			}
		}
	}

	static void Split(Screen screen, DockWindow window, DockWindowGroup sourceGroup, DockWindowGroup targetGroup, DockWindowDropPosition position)
	{
		var targetParent = FindNodeParent(screen, targetGroup);
		var targetLayout = targetParent is null ? FindDockingLayoutParent(screen, targetGroup) : null;
		if (targetParent is null && targetLayout is null)
		{
			return;
		}

		var isHorizontal = position is DockWindowDropPosition.Left or DockWindowDropPosition.Right;
		DockContainer? split = isHorizontal
			? targetParent as DockWindowRow
			: targetParent as DockWindowStack;

		if (split is null)
		{
			split = isHorizontal
				? screen.NewEntity().AddComponent<DockWindowRow>()
				: screen.NewEntity().AddComponent<DockWindowStack>();
			if (targetParent is not null)
			{
				ReplaceNode(screen, targetParent, targetGroup, split);
			}
			else
			{
				ReplaceNode(screen, targetLayout!, targetGroup, split);
			}
			ParentingSystem.AttachParentChild<DockContainer, DockNode>(split, targetGroup);
		}

		var newGroup = screen.NewEntity().AddComponent<DockWindowGroup>();
		MoveToGroup(window, sourceGroup, newGroup);
		var targetIndex = split.Children.IndexOf(targetGroup);
		var insertionIndex = position is DockWindowDropPosition.Left or DockWindowDropPosition.Top
			? targetIndex
			: targetIndex + 1;
		split.Children.Insert(insertionIndex, newGroup);
		((IChildOf<DockContainer>)newGroup).Parent = split;
	}

	static void ReplaceNode(Screen screen, DockContainer parent, DockNode existing, DockContainer replacement)
	{
		var index = parent.Children.IndexOf(existing);
		parent.DetachChild(existing);
		parent.Children.Insert(index, replacement);
		((IChildOf<DockContainer>)replacement).Parent = parent;
	}

	static void ReplaceNode(Screen screen, DockingLayout parent, DockNode existing, DockContainer replacement)
	{
		var index = parent.Children.IndexOf(existing);
		ParentingSystem.DetachParentChild<DockingLayout, DockNode>(parent, existing);
		parent.Children.Insert(index, replacement);
		((IChildOf<DockingLayout>)replacement).Parent = parent;
	}

	static void Normalize(Screen screen)
	{
		var layoutChanged = true;
		while (layoutChanged)
		{
			layoutChanged = false;
			foreach (var group in screen.SelectComponents<DockWindowGroup>().ToArray())
			{
				layoutChanged |= NormalizeGroup(screen, group);
			}

			foreach (var container in DockContainers(screen).ToArray())
			{
				layoutChanged |= NormalizeContainer(screen, container);
			}
		}
	}

	static bool NormalizeGroup(Screen screen, DockWindowGroup group)
	{
		if (group.Children.Values.Count != 0)
		{
			var activeWindow = group.ActiveWindow.Get();
			if (activeWindow is null || !group.Children.Values.Contains(activeWindow))
			{
				group.ActiveWindow = group.Children.Values.Last();
			}
			return false;
		}

		var parent = FindNodeParent(screen, group);
		if (parent is null)
		{
			return false;
		}

		ParentingSystem.DetachParentChild<DockContainer, DockNode>(parent, group);
		ClearActiveWindow(group);
		group.Entity.TryDeleteEntity();
		return true;
	}

	static bool NormalizeContainer(Screen screen, DockContainer container)
	{
		if (container.Children.Values.Count == 0)
		{
			return RemoveContainer(screen, container);
		}

		if (container.Children.Values.Count != 1)
		{
			return false;
		}

		return ReplaceContainerWithChild(screen, container, container.Children.Values.Single());
	}

	static bool RemoveContainer(Screen screen, DockContainer container)
	{
		var parent = FindNodeParent(screen, container);
		if (parent is not null)
		{
			ParentingSystem.DetachParentChild<DockContainer, DockNode>(parent, container);
			container.Entity.TryDeleteEntity();
			return true;
		}

		var layout = FindDockingLayoutParent(screen, container);
		if (layout is null)
		{
			return false;
		}

		ParentingSystem.DetachParentChild<DockingLayout, DockNode>(layout, container);
		container.Entity.TryDeleteEntity();
		return true;
	}

	static bool ReplaceContainerWithChild(Screen screen, DockContainer container, DockNode child)
	{
		var parent = FindNodeParent(screen, container);
		if (parent is not null)
		{
			var index = parent.Children.IndexOf(container);
			ParentingSystem.DetachParentChild<DockContainer, DockNode>(parent, container);
			parent.Children.Insert(index, child);
			((IChildOf<DockContainer>)child).Parent = parent;
			container.Entity.TryDeleteEntity();
			return true;
		}

		var layout = FindDockingLayoutParent(screen, container);
		if (layout is null)
		{
			return false;
		}

		var layoutIndex = layout.Children.IndexOf(container);
		ParentingSystem.DetachParentChild<DockingLayout, DockNode>(layout, container);
		layout.Children.Insert(layoutIndex, child);
		((IChildOf<DockingLayout>)child).Parent = layout;
		container.Entity.TryDeleteEntity();
		return true;
	}

	static DockWindowGroup? FindParentGroup(Screen screen, DockWindow window)
		=> screen.SelectComponents<DockWindowGroup>().SingleOrDefault(group => group.Children.Values.Contains(window));

	static DockContainer? FindNodeParent(Screen screen, DockNode node)
		=> DockContainers(screen)
			.SingleOrDefault(container => container.Children.Values.Contains(node));

	static DockingLayout? FindDockingLayoutParent(Screen screen, DockNode node)
		=> screen.SelectComponents<DockingLayout>().SingleOrDefault(layout => layout.Children.Values.Contains(node));

	static IEnumerable<DockContainer> DockContainers(Screen screen)
		=> screen.SelectComponents<DockWindowRow>()
			.Cast<DockContainer>()
			.Concat(screen.SelectComponents<DockWindowStack>());

	static void ClearActiveWindow(DockWindowGroup group)
	{
		group.ActiveWindow.Clear();
	}

	static DockWindowDragOperation? Operation(Screen screen)
		=> screen.SelectComponents<DockWindowDragOperation>().SingleOrDefault();

	static void ClearOperation(Screen screen)
	{
		var operation = Operation(screen);
		if (operation is not null)
		{
			operation.Entity.TryDeleteEntity();
		}
	}
}