namespace SiegeTower.Client.UX;

public interface IDockContent
{
	string Name { get; }

	Dock? Parent { get; set; }
}
