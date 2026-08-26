using SiegeTower.Data.ECSPattern;
using SiegeTower.Client.UX;

namespace SiegeTower.Client.Screens.Common;

public interface IScreenData : IDataComponent
{
	string Title { get; }
	LoadingQueue LoadingQueue { get; }
}