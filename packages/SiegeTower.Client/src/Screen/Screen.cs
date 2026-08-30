namespace SiegeTower.Client;

public class Screen : EntityStorage
{
	public Session Session { get; set; }
	public string Title { get; set; }
	
	public Screen(Session session, string title)
	{
		Session = session;
		Title = title;

		var menu = this.NewEntity().AddComponent<Element>(e => new Element(e, "menu")).AddComponent<MenuComponent>();
		AddMenuItem(menu, "Home", "/", "fa-solid fa-house");
		AddMenuItem(menu, "Workspaces", "/workspace", "fa-solid fa-folder");
		AddMenuItem(menu, "Ollama", "/ollama", "fa-solid fa-microchip");
	}

	private void AddMenuItem(MenuComponent menu, string text, string uri, string icon)
	{
		var item = this.NewEntity().AddComponent<Element>(e => new Element(e, $"menu-{text.ToLowerInvariant()}"));
		item.Entity.AddComponent(e => new Hyperlink(e, uri, true));
		var menuItem = item.Entity.AddComponent(e => new MenuItemComponent(e, text, icon));
		ElementSystem.Attach(menu, menuItem);
	}
}