public static class ElementSystem
{
	public static void Attach(Element parent, Element child)
	{
		ArgumentNullException.ThrowIfNull(parent);
		ArgumentNullException.ThrowIfNull(child);

		if (child.Parent.HasValue)
		{
			var currentParent = FindElement(child.Parent);
			if (currentParent is not null)
			{
				Detach(currentParent, child);
			}
		}

		var children = parent.Children;
		children.Add(child);
		parent.Children = children;
		child.Parent = parent;
	}

	public static void Detach(Element parent, Element child)
	{
		ArgumentNullException.ThrowIfNull(parent);
		ArgumentNullException.ThrowIfNull(child);

		var children = parent.Children;
		children.Remove(child);
		parent.Children = children;

		if (child.Parent.RefID == parent.EntityID)
		{
			child.Parent = null;
		}
	}

	private static Element? FindElement(ComponentRef<Element> elementReference)
	{
		if (!elementReference.RefID.HasValue ||
			!elementReference.EntityStorage.Components.TryGetValue(typeof(Element), out var elements) ||
			!elements.TryGetValue(elementReference.RefID.Value, out var component))
		{
			return null;
		}

		return component as Element;
	}
}