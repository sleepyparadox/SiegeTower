namespace SiegeTower.Client;

public static class ElementHelper
{
	public static IRequires<Element> WithChildren(this IRequires<Element> parent, params IRequires<Element>[] children)
	{
		ArgumentNullException.ThrowIfNull(parent);
		ArgumentNullException.ThrowIfNull(children);

		foreach (var child in children)
		{
			ArgumentNullException.ThrowIfNull(child);
			ElementSystem.Attach(parent, child);
		}

		return parent;
	}

	public static IRequires<Element> WithChildren(this IRequires<Element> parent, Action<IRequires<Element>> configureChildren)
	{
		ArgumentNullException.ThrowIfNull(parent);
		ArgumentNullException.ThrowIfNull(configureChildren);

		configureChildren(parent);
		return parent;
	}
}