using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;

namespace SiegeTower.Client;

public sealed class Session // 1 session per tab
{	
	public Screen ActiveScreen { get; set; } 
	public Session(NavigationManager injectedNavigationManager, HttpClient injectedHttpClient)
	{
		ActiveScreen = new Screen(this, "Home");
		ActiveScreen.AddNewBreadCrumbEntity("Home", injectedNavigationManager.BaseUri, true, 0);
	}
}