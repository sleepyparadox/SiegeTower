using SiegeTower.Data.ECSPattern;

namespace SiegeTower.Client.Screens.Common;

public interface IScreenData : IDataComponent
{
	string Title { get; }
	bool IsLoadedOnce { get; }
}