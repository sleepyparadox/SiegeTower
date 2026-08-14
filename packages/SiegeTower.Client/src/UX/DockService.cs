namespace SiegeTower.Client.UX;

public static class DockService
{
	public static void Attach(Dock dock, IDockContent dockContent, bool makeActive = true)
	{
		ArgumentNullException.ThrowIfNull(dock);
		ArgumentNullException.ThrowIfNull(dockContent);

		if (dockContent.Parent == dock)
		{
			if (!dock.Contents.Contains(dockContent))
			{
				dock.Contents.Add(dockContent);
			}

			return;
		}

		if (dockContent.Parent is not null)
		{
			Detach(dockContent.Parent, dockContent);
		}

		if (!dock.Contents.Contains(dockContent))
		{
			dock.Contents.Add(dockContent);
		}

		dockContent.Parent = dock;
		if (makeActive || dock.Contents.Count == 1)
		{
			dock.ActiveContent = dockContent;
		}
	}

	public static void Detach(Dock dock, IDockContent dockContent)
	{
		ArgumentNullException.ThrowIfNull(dock);
		ArgumentNullException.ThrowIfNull(dockContent);

		dock.Contents.Remove(dockContent);
		if (dockContent.Parent == dock)
		{
			dockContent.Parent = null;
		}

		if (dock.ActiveContent == dockContent)
		{
			dock.ActiveContent = dock.Contents.FirstOrDefault();
		}
	}
}