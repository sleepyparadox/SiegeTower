using SiegeTower.Data;

namespace SiegeTower.Client;

public static class APISystem
{
	public static Task<WorkspaceRow[]?> WorkspaceGet(Session session)
		=> HttpSystem.Get<WorkspaceRow[]>(session, "/api/workspace");

	public static Task<WorkspaceRow?> WorkspaceCreate(Session session, string name)
		=> HttpSystem.Post<object, WorkspaceRow>(session, "/api/workspace", new { Name = name });

	public static Task<bool> WorkspaceDelete(Session session, string name)
		=> HttpSystem.Delete(session, $"/api/workspace/{Uri.EscapeDataString(name)}");
}