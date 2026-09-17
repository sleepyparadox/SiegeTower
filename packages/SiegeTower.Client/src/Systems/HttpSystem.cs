using System.Net.Http.Json;
using System.Text.Json;

namespace SiegeTower.Client;

public static class HttpSystem
{
	public static async Task<T?> Get<T>(Session session, string url)
	{
		ArgumentNullException.ThrowIfNull(session);
		ArgumentException.ThrowIfNullOrEmpty(url);

		try
		{
			using var response = await session.HttpClient.GetAsync(url);
			if (!response.IsSuccessStatusCode)
			{
				return default;
			}

			return await response.Content.ReadFromJsonAsync<T>();
		}
		catch (HttpRequestException)
		{
			return default;
		}
		catch (JsonException)
		{
			return default;
		}
	}

	public static async Task<TResponse?> Post<TRequest, TResponse>(Session session, string url, TRequest request)
	{
		ArgumentNullException.ThrowIfNull(session);
		ArgumentException.ThrowIfNullOrEmpty(url);

		try
		{
			using var response = await session.HttpClient.PostAsJsonAsync(url, request);
			if (!response.IsSuccessStatusCode)
			{
				return default;
			}

			return await response.Content.ReadFromJsonAsync<TResponse>();
		}
		catch (HttpRequestException)
		{
			return default;
		}
		catch (JsonException)
		{
			return default;
		}
	}
}