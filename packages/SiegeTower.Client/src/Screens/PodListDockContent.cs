using SiegeTower.Data;

namespace SiegeTower.Client.Screens;

public sealed class PodListDockContent
{
	public IReadOnlyList<Pod> Pods { get; set; } = [];
}
