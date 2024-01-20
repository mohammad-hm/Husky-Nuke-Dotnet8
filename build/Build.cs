using Nuke.Common;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.GitVersion;
using Nuke.Common.Tools.SonarScanner;
using Nuke.Common.Utilities;

class Build : NukeBuild
{
	/// Support plugins are available for:
	///   - JetBrains ReSharper        https://nuke.build/resharper
	///   - JetBrains Rider            https://nuke.build/rider
	///   - Microsoft VisualStudio     https://nuke.build/visualstudio
	///   - Microsoft VSCode           https://nuke.build/vscode

	public static int Main() => Execute<Build>(x => x.Publish);

	[Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
	readonly Configuration configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;
	[Parameter] readonly string sonarUrl;
	[Parameter] readonly string nugetUrl;
	[Parameter] readonly string sonarLogin;
	[Parameter] readonly string nugetApi;
	[Solution]
	readonly Solution solution;
	[GitVersion]
	readonly GitVersion gitVersion;
	Target Clean => t => t
		.Before(Restore)
		.Executes(() =>
		{
			_ = DotNetTasks.DotNetClean(s => s
				.SetProject(solution)
				.SetConfiguration(configuration)
			);
		});

	Target Restore => t => t
		.DependsOn(Clean)
		.Executes(() =>
		{
			_ = DotNetTasks.DotNetRestore();
		});
	Target Compile => t => t
		.DependsOn(Restore)
		.Executes(() =>
		{
			_ = SonarScannerTasks.SonarScannerBegin(s => s
				.SetServer(sonarUrl)
				.SetProjectKey("Vishkadeh.Engine.ScannerAvProxy")
				.SetVersion(Version)
				.SetLogin(sonarLogin));
			_ = DotNetTasks.DotNetBuild(s => s
				.SetProjectFile(solution)
				.SetConfiguration(Configuration.Debug)
				.SetNoRestore(InvokedTargets.Contains(Restore))
			);
			_ = SonarScannerTasks.SonarScannerEnd(s => s.SetLogin(sonarLogin));
		});
	Target Test => t => t
		.DependsOn(Compile)
		.Executes(() =>
		{
			_ = DotNetTasks.DotNetTest(s => s
				.SetProjectFile(solution)
				.SetConfiguration(configuration)
				.SetNoBuild(InvokedTargets.Contains(Compile))
			);
		});
	Target Pack => t => t
		.DependsOn(Test)
		.Executes(() =>
		{
			_ = DotNetTasks.DotNetPack(s => s
				.SetProject(solution)
				.SetConfiguration(configuration)
				.SetOutputDirectory(RootDirectory / "artifacts")
				.SetAssemblyVersion(Version)
				.SetIncludeSymbols(true)
				.SetCopyright("Copyright (c) Amnpardaz 2024")
				.SetVersion(Version)
				.SetNoBuild(InvokedTargets.Contains(Compile))
			);
		});
	Target Publish => t => t
		.DependsOn(Pack)
		.Executes(() =>
		{
			// publish the nuget package to the nuget feed
			_ = DotNetTasks.DotNetNuGetPush(s => s
				.SetTargetPath((RootDirectory / "artifacts" / "Vishkadeh.Engine.ScannerAvProxy.") + Version + ".nupkg")
				.SetSource(nugetUrl)
				.SetApiKey(nugetApi)
			);
		});

	string Version
	{
		get
		{
			var patchVersion = gitVersion.Patch > 0 ? gitVersion.Patch - 1 : gitVersion.Patch;
			return gitVersion.CommitsSinceVersionSource.EqualsOrdinalIgnoreCase("0")
			 ? gitVersion.MajorMinorPatch :
			$"{gitVersion.Major}.{gitVersion.Minor}.{patchVersion}.{gitVersion.CommitsSinceVersionSource}";
		}
	}
}
