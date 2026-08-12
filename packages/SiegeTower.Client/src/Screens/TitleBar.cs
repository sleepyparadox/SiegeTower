namespace SiegeTower.Client.Screens;

public sealed class TitleBar
{
	public string Title { get; init; } = string.Empty;

	public string[] Breadcrumbs { get; init; } = [];
}