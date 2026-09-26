namespace SiegeTower.Client;

public sealed class AsyncTask : Component
{
	public Task Task { get; }

	public AsyncTask(Entity entity, Func<Task> function) : base(entity)
	{
		ArgumentNullException.ThrowIfNull(function);

		Task = RunAsync(function);
	}

	static async Task RunAsync(Func<Task> function)
	{
		await function();
	}
}