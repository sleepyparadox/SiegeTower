using SiegeTower.Client.Pattern;
using SiegeTower.Client.UX;

namespace SiegeTower.Client.Screens.Common;

public interface IScreenData : IDataComponent
{
	string Title { get; }
	LoadingQueue LoadingQueue { get; }
}