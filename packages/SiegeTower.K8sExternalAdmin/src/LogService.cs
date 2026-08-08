namespace SiegeTower.K8sExternalAdmin;

public static class LogService
{
	public static void Info(string message) => Write("INFO", message);

	public static void Error(string message) => Write("ERROR", message);

	private static void Write(string level, string message)
	{
		Console.Error.WriteLine($"{DateTimeOffset.Now:O} {level} {message}");
	}
}