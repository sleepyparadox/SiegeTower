namespace SiegeTower.Client.UX;

public static class DockService
{
	public static void Attach(Dock dock, IDockContent dockContent)
	{
		ArgumentNullException.ThrowIfNull(dock);
		ArgumentNullException.ThrowIfNull(dockContent);

		if (dockContent.Parent == dock)
		{
			if (!dock.MutableContents.Contains(dockContent))
			{
				dock.MutableContents.Add(dockContent);
			}

			return;
		}

		if (dockContent.Parent is not null)
		{
			Detach(dockContent.Parent, dockContent);
		}

		if (!dock.MutableContents.Contains(dockContent))
		{
			dock.MutableContents.Add(dockContent);
		}

		dockContent.Parent = dock;
	}

	public static void Detach(Dock dock, IDockContent dockContent)
	{
		ArgumentNullException.ThrowIfNull(dock);
		ArgumentNullException.ThrowIfNull(dockContent);

		dock.MutableContents.Remove(dockContent);
		if (dockContent.Parent == dock)
		{
			dockContent.Parent = null;
		}

		if (dock.ActiveContent == dockContent)
		{
			dock.ActiveContent = dock.MutableContents.FirstOrDefault();
		}
	}
}