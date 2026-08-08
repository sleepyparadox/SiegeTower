# Use SiegeTrain to manage dependencies and releases

Use SiegeTrain to manage SiegeTrain CLI packages within this repo
SiegeTrain is a global CLI tool but should be run from repo root (the directory containing siegetrain.packages.json and siegetrain.releases.json)

```bash
SiegeTrain
Usage: SiegeTrain <package|release> <action> [--option value] [--root-dir path]
	SiegeTrain build [--id package-id] [--root-dir path]
	SiegeTrain cmd [command-name] [--root-dir path]
	SiegeTrain release add-candidate {major-version} [--author email] [--description text] [--repair-releases versions] [--duplicated-releases versions] [--root-dir path]
	SiegeTrain release list [--root-dir path]
	SiegeTrain release add-major {major-version} [--author email] [--description text] [--repair-releases versions] [--duplicated-releases versions] [--root-dir path]
	SiegeTrain release promote --minor|--patch [--root-dir path]
	SiegeTrain release write {src_path}/Releases.cs [--namespace name] [--classname name] [--latest-only true|false] [--root-dir path]
	SiegeTrain release write-all [--root-dir path]
	SiegeTrain web [port] [--local|--public] [--root-dir path]
Package actions: add, delete, get, list
Release commands: add-candidate, list, promote, add-major
```

Use `SiegeTrain` to build all dependencies
Use `SiegeTrain cmd run` to run the main package in the repo

When we are not in master we should use `SiegeTrain add-candidate` to create a new release candidate for this branch in latest major release.
When we are ready to merge we promote it to a minor version with `SiegeTrain release promote --minor`


# Use Package structure

Each package has a 
`packages/{package-name}/src`
`packages/{package-name}/dist`
`packages/{package-name}/bin`
`packages/{package-name}/obj`

And includes a `packages/{package}/.gitignore` with contents
```txt
bin
dist
```

It it each package repsonsibility to push their dlls and/or content to dist
And each parent package is responsible for copying and consuming dist of their dependencies
This is so that the dist folder can be used as a detirministic output and cached to speed up builds

# Dotnet setup

dotnet projects should use the following settings

File: Directory.Build.props
```xml
<Project>
	<PropertyGroup>
		<BaseIntermediateOutputPath>../obj/</BaseIntermediateOutputPath>
	</PropertyGroup>
</Project>
```

File: *.csproj
```xml
	<PropertyGroup>
		<TargetFramework>net10.0</TargetFramework>
		<Nullable>enable</Nullable>
		<ImplicitUsings>enable</ImplicitUsings>
		<BaseOutputPath>../bin/</BaseOutputPath>
	</PropertyGroup>

	<Target Name="CopyBuildOutputToDist" AfterTargets="Build">
		<ItemGroup>
			<BuildArtifacts Include="$(TargetDir)**/*" />
		</ItemGroup>
		<Copy SourceFiles="@(BuildArtifacts)"
			DestinationFiles="@(BuildArtifacts->'../dist/%(RecursiveDir)%(Filename)%(Extension)')"
			SkipUnchangedFiles="true" />
	</Target>
```


# Coding Standards

We follow Entity Component System standards where possible.
This means Component classes are data only, and Service classes are logic only.
Components should be Records or Readonly Records without nullable properties (unless it can genuinely empty.)
If we need to parse a Component in an unsafe way with nullables.

Use this pattern
```cs
public class FooUnsave
{
	public int? SomeInt { get; set; }
}

public record Foo
{
	public int SomeInt { get; set; }
}

```


We test with our actual Components and Services not mocks.
The only exception is external systems where we can't control the outcome like external apis.
For these we use as services pattern.

Like this:
```cs
// Services.cs

public static class Services
{
	public static IGit Git { get; private set; } = new GitInstance();
	public static IDotNet DotNet { get; private set; } = new DotNetInstances();
	public static IPackageBuilderService PackageBuilder { get; private set; } = new PackageBuilderInstance();

	public static ReplaceGitService(IGit git) => Git = git;
}
```

```cs
// GitService.cs

public interface IGit
{
	LocalRepo? GetLocalRepo(string path);
	string GetUserEmail(string path);
	string Clone(string repository, string branch, string path);
	string GetBranch(string path);
	string SwitchBranch(string path, string branch);
	string NewBranch(string path, string name, bool @switch = true);
}

public sealed class GitInstance : IGit
{
	public LocalRepo? GetLocalRepo(string path) => GitService.GetLocalRepo(path);
	public string GetUserEmail(string path) => GitService.GetUserEmail(path);
	public string Clone(string repository, string branch, string path) => GitService.Clone(repository, branch, path);
	public string GetBranch(string path) => GitService.GetBranch(path);
	public string SwitchBranch(string path, string branch) => GitService.SwitchBranch(path, branch);
	public string NewBranch(string path, string name, bool @switch = true) => GitService.NewBranch(path, name, @switch);
}

public static class GitService
{

}
```

# Coding Style

Use tabs not spaces

Use pascal naming for classes, methods, and properties
Use "_" prefix for private fields
Do not use the "private" keyword assume private is default
Do not use the "internal" keyword just make it public if required or leave as private (without "private" keyword)

Example

```cs
public ClassFoo
{
	public int SomeInt { get; set; }
	int _someInt;
}
```
