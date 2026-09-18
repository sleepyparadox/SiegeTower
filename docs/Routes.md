# Routes

The load balancer is the public entry point for SiegeTower. Keep public route names, client navigation, and upstream paths aligned with this contract.

## Public Routes

| Public route | Owner | Upstream or behavior |
| --- | --- | --- |
| `/` | `SiegeTower.Client` / `HomeScreen` | Serve the client application. |
| `/workspace` | `SiegeTower.Client` / `WorkspaceListScreen` | Serve the client application. |
| `/workspace/{id}` | `SiegeTower.Client` / `WorkspaceHomeScreen` | Serve the workspace home screen. `{id}` is the workspace ID, without the `st-workspace-` prefix. |
| `/workspace/{id}/files` | `SiegeTower.Client` / `WorkspaceFilesScreen` | Serve the workspace files screen. |
| `/workspace/{id}/api/*` | `SiegeTower.WorkspaceHarness` | Proxy to `st-workspace-{id}:80/api/*`. |
| `/workspace/{id}/api/file` | `SiegeTower.WorkspaceHarness` | Proxy `GET` to `st-workspace-{id}:80/api/file`. |
| `/api/github-access-token` | `SiegeTower.Api` | Generate a GitHub App installation token. |
| `/api/*` | `SiegeTower.Api` | Proxy to `st-api:80/api/*`. |
| `/ollama` | `SiegeTower.Client` / `OllamaScreen` | Serve the client application. |
| `/ollama/api/*` | Ollama | Proxy to `st-ollama:11434/api/*`. |
| `/example` | `SiegeTower.Client` / `ExampleScreen` | Serve the client application. |

The client serves the remaining non-API routes from its static files and falls back to `index.html` for client-side navigation.

## Naming Rules

- Use the singular `/workspace` route for both the workspace list and an individual workspace.
- Use `/workspace/{id}/api/*` for workspace-harness requests. The workspace ID is captured from the URL and used to construct the upstream service name.
- Keep `/api/*` reserved for `SiegeTower.Api`.
- Keep `/ollama/api/*` reserved for Ollama requests made by `OllamaScreen`.
- UI labels such as `Workspaces` may remain plural when they describe a list; URL segments should follow the routes above.

## Implementation Locations

- Load-balancer routing: `packages/SiegeTower.K8sExternalAdmin/src/Images/LoadBalancer.cs`
- Client navigation: `packages/SiegeTower.Client/src/Session/Session.cs`
- Workspace file requests: `packages/SiegeTower.Client/src/Services/Workspace/WorkspaceFileService.cs`
- GitHub access token requests: `packages/SiegeTower.Client/src/Services/API/APIService.cs` and `packages/SiegeTower.Api/src/Program.cs`
- SiegeTower API endpoints: `packages/SiegeTower.Api/src/Program.cs`
- Workspace-harness endpoints: `packages/SiegeTower.WorkspaceHarness/src/Program.cs`
