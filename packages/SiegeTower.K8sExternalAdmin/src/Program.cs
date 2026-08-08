if (args is ["push"])
{
	Console.WriteLine("ok");
	return;
}

Console.Error.WriteLine("Usage: SiegeTower.K8sExternalAdmin push");
Environment.ExitCode = 1;
