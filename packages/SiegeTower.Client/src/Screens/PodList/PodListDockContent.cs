using SiegeTower.Data;
using SiegeTower.Client.UX;

namespace SiegeTower.Client.Screens.PodList;

public sealed class PodListDockContent : IDockContent
{
	#region  IDockContent

	string IDockContent.Name { get => "Pods"; }

	Dock? IDockContent.Parent { get; set; }

	#endregion

	public IReadOnlyList<Pod> Pods { get; set; } = [];
}
