public static class ElementSystem
{
	public static void Attach(IRequires<Element> parent, IRequires<Element> child)
		=> Attach(parent.GetComponent(), child.GetComponent());

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

		if (child.Parent.Get() == parent)
		{
			child.Parent = new ComponentRef<Element>();
		}
	}

	private static Element? FindElement(ComponentRef<Element> elementReference)
		=> elementReference.Get();
}