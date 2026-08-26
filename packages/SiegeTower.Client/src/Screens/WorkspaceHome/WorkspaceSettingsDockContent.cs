using SiegeTower.Client.Services.Workspace;
using SiegeTower.Client.UX;
using SiegeTower.Data;

namespace SiegeTower.Client.Screens.WorkspaceHome;

public sealed class WorkspaceSettingsDockContent : IDockContent
{
	private readonly WorkspaceHomeScreenData data;

	public WorkspaceSettingsDockContent(WorkspaceHomeScreenData data)
	{
		this.data = data ?? throw new ArgumentNullException(nameof(data));
	}

	public string Name => "Settings";

	public Dock? Parent { get; set; }

	public WorkspaceSettings Settings { get; set; } = new();

	public bool IsSaving { get; private set; }

	public Exception? Error { get; private set; }

	public void SetSettings(WorkspaceSettings settings)
	{
		ArgumentNullException.ThrowIfNull(settings);
		Settings = settings;
	}

	public async Task SaveAsync()
	{
		if (IsSaving)
		{
			return;
		}

		IsSaving = true;
		Error = null;
		try
		{
			Settings.GitAccessToken = Settings.GitAccessToken?.Trim();
			Settings.GitBranchName = Settings.GitBranchName?.Trim();
			Settings.GitPR = Settings.GitPR?.Trim();
			Settings = await WorkspaceSettingsService.SaveAsync(data.Session.Context, data.Session.Services.HttpClient, Settings);
		}
		catch (Exception exception)
		{
			Error = exception;
		}
		finally
		{
			IsSaving = false;
			data.Session.RequestRedraw();
		}
	}
}
