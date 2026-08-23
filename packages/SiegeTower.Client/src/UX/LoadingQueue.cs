namespace SiegeTower.Client.UX;

public sealed class LoadingQueue
{
	private int pendingTasks;

	public bool IsLoading => Volatile.Read(ref pendingTasks) > 0;

	public event EventHandler? Changed;

	public void Append(Task task)
	{
		ArgumentNullException.ThrowIfNull(task);
		Interlocked.Increment(ref pendingTasks);
		Changed?.Invoke(this, EventArgs.Empty);
		_ = CompleteAsync(task);
	}

	private async Task CompleteAsync(Task task)
	{
		try
		{
			await task;
		}
		finally
		{
			Interlocked.Decrement(ref pendingTasks);
			Changed?.Invoke(this, EventArgs.Empty);
		}
	}
}