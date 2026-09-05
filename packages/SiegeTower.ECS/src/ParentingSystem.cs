public static class ParentingSystem
{
	public static int GetIndex<TParent, TChild>(this TChild child)
		where TParent : Component, IParentOf<TChild>
		where TChild : Component, IChildOf<TParent>
	{
		ArgumentNullException.ThrowIfNull(child);

		var parent = child.Parent.Get();
		return parent is null ? -1 : parent.Children.IndexOf(child);
	}

	public static void AttachParentChild<TParent, TChild>(TParent parent, TChild child)
		where TParent : Component, IParentOf<TChild>
		where TChild : Component, IChildOf<TParent>
	{
		ArgumentNullException.ThrowIfNull(parent);
		ArgumentNullException.ThrowIfNull(child);

		if (child.Parent.HasValue)
		{
			var currentParent = child.Parent.Get();
			if (currentParent is not null)
			{
				DetachParentChild(currentParent, child);
			}
		}

		var children = parent.Children;
		if (!children.Values.Contains(child))
		{
			children.Add(child);
			parent.Children = children;
		}
		child.Parent = parent;
	}

	public static void DetachParentChild<TParent, TChild>(TParent parent, TChild child)
		where TParent : Component, IParentOf<TChild>
		where TChild : Component, IChildOf<TParent>
	{
		ArgumentNullException.ThrowIfNull(parent);
		ArgumentNullException.ThrowIfNull(child);

		var children = parent.Children;
		children.Remove(child);
		parent.Children = children;

		if (child.Parent.Get() == parent)
		{
			child.Parent.Clear();
		}
	}

	public static TChild AttachTo<TChild, TParent>(this TChild child, TParent parent)
		where TParent : Component, IParentOf<TChild>
		where TChild : Component, IChildOf<TParent>
	{
		AttachParentChild(parent, child);
		return child;
	}

	public static TChild DetachFrom<TChild, TParent>(this TChild child, TParent parent)
		where TParent : Component, IParentOf<TChild>
		where TChild : Component, IChildOf<TParent>
	{
		DetachParentChild(parent, child);
		return child;
	}

	public static TParent AttachChild<TParent, TChild>(this TParent parent, TChild child)
		where TParent : Component, IParentOf<TChild>
		where TChild : Component, IChildOf<TParent>
	{
		AttachParentChild(parent, child);
		return parent;
	}

	public static TParent DetachChild<TParent, TChild>(this TParent parent, TChild child)
		where TParent : Component, IParentOf<TChild>
		where TChild : Component, IChildOf<TParent>
	{
		DetachParentChild(parent, child);
		return parent;
	}
}