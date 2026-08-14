using SiegeTower.Data;
using SiegeTower.Client.UX;

namespace SiegeTower.Client.Screens;

public sealed class PodListDockContent : IDockContent
{
	public string Name { get; init; } = "Pods";

	public IReadOnlyList<Pod> Pods { get; set; } = [];
}
