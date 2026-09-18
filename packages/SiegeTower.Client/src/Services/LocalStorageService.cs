using Microsoft.JSInterop;

namespace SiegeTower.Client;

public static class LocalStorageService
{
	public static async Task<string?> Get(Session session, string key)
	{
		ArgumentNullException.ThrowIfNull(session);
		ArgumentException.ThrowIfNullOrEmpty(key);

		return await session.JSRuntime.InvokeAsync<string?>("localStorage.getItem", [key]);
	}

	public static async Task Set(Session session, string key, string value)
	{
		ArgumentNullException.ThrowIfNull(session);
		ArgumentException.ThrowIfNullOrEmpty(key);
		ArgumentNullException.ThrowIfNull(value);

		await session.JSRuntime.InvokeAsync<object?>("localStorage.setItem", [key, value]);
	}
}