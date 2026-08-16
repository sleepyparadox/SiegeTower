# SiegeTower Agent Instructions

Follow the repository standards in `.github/prompts/standards.prompt.md` for all code and project changes.

Follow the repository UI standards in `.github/prompts/style.prompt.md` for all client and UX changes.

Consult `docs/Routes.md` before changing public routes, client navigation, load-balancer configuration, API endpoints, or workspace-harness endpoints. Keep that document updated when the route contract changes.

Use the existing package structure, naming conventions, and abstractions. Keep changes focused and do not modify unrelated user changes.

Use SiegeTrain from the repository root for builds, dependencies, and releases. Do not commit changes unless explicitly requested.
