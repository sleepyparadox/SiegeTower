namespace SiegeTower.Client;

public sealed class Screen : EntityStorage
{
	public Session Session { get; set; }
	public string Title { get; set; }
	public bool LegacyScreenRenderer { get; set; }
	
	public Screen(Session session, string title, bool legacyScreenRenderer = false)
	{
		Session = session;
		Title = title;
		LegacyScreenRenderer = legacyScreenRenderer;

		var menu = this.NewEntity().AddComponent<Element>(e => new Element(e, "menu")).AddComponent<MenuComponent>();
		AddMenuItem(menu, "Home", "/", "fa-solid fa-house");
		AddMenuItem(menu, "Workspaces", "/workspace", "fa-solid fa-folder");
		AddMenuItem(menu, "Ollama", "/ollama", "fa-solid fa-microchip");
		var example = AddMenuItem(menu, "Example", "/example", "fa-solid fa-flask");
		AddMenuItem(example, "Files", "/example/files", "fa-solid fa-file");
		AddMenuItem(example, "SQL", "/example/sql", "fa-solid fa-database");
		var legacyExample = AddMenuItem(menu, "Example (Old)", "/example-old", "fa-solid fa-flask");
		AddMenuItem(legacyExample, "Files", "/example-old/files", "fa-solid fa-file");
		AddMenuItem(legacyExample, "SQL", "/example-old/sql", "fa-solid fa-database");
	}

	private MenuItemComponent AddMenuItem(MenuComponent menu, string text, string uri, string icon)
	{
		var item = this.NewEntity().AddComponent<Element>(e => new Element(e, $"menu-{text.ToLowerInvariant()}"));
		item.Entity.AddComponent(e => new Hyperlink(e, uri, true));
		var menuItem = item.Entity.AddComponent(e => new MenuItemComponent(e, text, icon));
		menu.WithChildren(menuItem);
		return menuItem;
	}

	private MenuItemComponent AddMenuItem(MenuItemComponent parent, string text, string uri, string icon)
	{
		var item = this.NewEntity().AddComponent<Element>(e => new Element(e, $"menu-{text.ToLowerInvariant()}"));
		item.Entity.AddComponent(e => new Hyperlink(e, uri, true));
		var menuItem = item.Entity.AddComponent(e => new MenuItemComponent(e, text, icon));
		parent.WithChildren(menuItem);
		return menuItem;
	}
}