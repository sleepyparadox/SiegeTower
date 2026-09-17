using SiegeTower.Data;

namespace SiegeTower.Client;

public static class APISystem
{
	public static Task<WorkspaceRow[]?> WorkspaceGet(Session session)
		=> HttpSystem.Get<WorkspaceRow[]>(session, "/api/workspace");
}