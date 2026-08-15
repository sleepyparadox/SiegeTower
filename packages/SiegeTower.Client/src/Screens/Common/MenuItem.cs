namespace SiegeTower.Client.Screens.Common;

public sealed class MenuItem
{
	public MenuItem(string name, Action action)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(name);
		ArgumentNullException.ThrowIfNull(action);
		Name = name;
		Action = action;
	}

	public string Name { get; }

	public Action Action { get; }
}
