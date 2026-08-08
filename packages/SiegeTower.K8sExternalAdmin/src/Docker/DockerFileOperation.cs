namespace SiegeTower.K8sExternalAdmin.Docker.DockerFileOperation;

public interface IDockerFileOperation
{
	string ToString();
}

public sealed class From(string image) : IDockerFileOperation
{
	public override string ToString() => $"FROM {image}";
}

public sealed class Run(string command) : IDockerFileOperation
{
	public override string ToString() => $"RUN {command}";
}

public sealed class Copy(string source, string destination) : IDockerFileOperation
{
	public override string ToString() => $"COPY {source} {destination}";
}

public sealed class Workdir(string path) : IDockerFileOperation
{
	public override string ToString() => $"WORKDIR {path}";
}

public sealed class Expose(params int[] ports) : IDockerFileOperation
{
	public override string ToString() => $"EXPOSE {string.Join(' ', ports)}";
}

public sealed class Cmd(string command) : IDockerFileOperation
{
	public override string ToString() => $"CMD {command}";
}