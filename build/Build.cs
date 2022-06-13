// ReSharper disable RedundantUsingDirective

using System;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using Nuke.Common;
using Nuke.Common.Execution;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.ILRepack;
using Nuke.Common.Tools.OctoVersion;
using Nuke.Common.Utilities.Collections;
using Serilog;
using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

[CheckBuildProjectConfigurations]
[UnsetVisualStudioEnvironmentVariables]
class Build : NukeBuild
{
    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")] readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

    [Solution(GenerateProjects = true)] readonly Solution Solution;

    [Parameter(
        "Whether to auto-detect the branch name - this is okay for a local build, but should not be used under CI.")]
    readonly bool AutoDetectBranch = IsLocalBuild;

    [Parameter(
        "Branch name for OctoVersion to use to calculate the version number. Can be set via the environment variable OCTOVERSION_CurrentBranch.",
        Name = "OCTOVERSION_CurrentBranch")]
    readonly string BranchName;

    [OctoVersion(UpdateBuildNumber = true, BranchParameter = nameof(BranchName),
        AutoDetectBranchParameter = nameof(AutoDetectBranch), Framework = "net6.0")]
    readonly OctoVersionInfo OctoVersionInfo;

    AbsolutePath SourceDirectory => RootDirectory / "source";
    AbsolutePath ArtifactsDirectory => RootDirectory / "artifacts";
    AbsolutePath LocalPackagesDirectory => RootDirectory / ".." / "LocalPackages";
    AbsolutePath OctopusCommandLineFolder => SourceDirectory / "CommandLine";

    Target Clean => _ => _
        .Before(Restore)
        .Executes(() =>
        {
            SourceDirectory.GlobDirectories("**/bin", "**/obj", "**/TestResults").ForEach(DeleteDirectory);
            EnsureCleanDirectory(ArtifactsDirectory);
        });

    [PublicAPI]
    Target CalculateVersion => _ => _
        .Executes(() =>
        {
            //all the magic happens inside `[OctoVersion]` above. we just need a target for TeamCity to call
        });

    Target Restore => _ => _
        .DependsOn(Clean)
        .Executes(() =>
        {
            DotNetRestore(_ => _
                .SetProjectFile(Solution));
        });

    Target Compile => _ => _
        .DependsOn(Clean)
        .DependsOn(Restore)
        .Executes(() =>
        {
            Log.Information("Building {0} v{1}", Solution.Name, OctoVersionInfo.FullSemVer);

            DotNetBuild(_ => _
                .SetProjectFile(Solution)
                .SetConfiguration(Configuration)
                .SetVersion(OctoVersionInfo.FullSemVer)
                .EnableNoRestore());
        });

    Target Test => _ => _
        .DependsOn(Compile)
        .Executes(() =>
        {
            DotNetTest(_ => _
                .SetProjectFile(Solution)
                .SetConfiguration(Configuration)
                .EnableNoBuild()
                .EnableNoRestore());
        });

    Target Merge => _ => _
        .DependsOn(Test)
        .Executes(() =>
        {
            var targets = Solution.CommandLine.GetTargetFrameworks();

            foreach (var target in targets)
            {
                var inputFolder = OctopusCommandLineFolder / "bin" / Configuration / target;
                var outputFolder = OctopusCommandLineFolder / "bin" / Configuration / $"{target}-Merged";
                EnsureExistingDirectory(outputFolder);

                // The call to ILRepack with .EnableInternalize() requires the Octopus.Server.Client.dll assembly to be first in the list.
                var inputAssemblies = inputFolder.GlobFiles("NewtonSoft.Json.dll", "Octopus.*.dll")
                    .Select(x => x.ToString())
                    .OrderByDescending(x => x.Contains("Octopus.CommandLine.dll"))
                    .ThenBy(x => x)
                    .ToArray();

                //note: ilmerge requires CopyLocalLockFileAssemblies set to true in the csproj
                ILRepackTasks.ILRepack(_ => _
                    .SetAssemblies(inputAssemblies)
                    .SetOutput(outputFolder / "Octopus.CommandLine.dll")
                    .EnableInternalize()
                    .DisableParallel()
                    .EnableXmldocs()
                    .SetLib(inputFolder)
                );

                DeleteDirectory(inputFolder);
                MoveDirectory(outputFolder, inputFolder);
            }
        });

    Target Pack => _ => _
        .DependsOn(Compile)
        .DependsOn(Test)
        .DependsOn(Merge)
        .Executes(() =>
        {
            var nuspec = "Octopus.CommandLine.nuspec";
            var nuspecPath = OctopusCommandLineFolder / nuspec;
            try
            {
                ReplaceTextInFiles(nuspecPath, "<version>$version$</version>", $"<version>{OctoVersionInfo.FullSemVer}</version>");
                ReplaceTextInFiles(nuspecPath, "\\$configuration$\\", $"\\{Configuration.ToString()}\\");

                DotNetPack(_ => _
                    .SetProject(Solution)
                    .SetProcessArgumentConfigurator(args =>
                    {
                        args.Add("/p:NuspecFile=" + nuspec);
                        return args;
                    })
                    .SetVersion(OctoVersionInfo.FullSemVer)
                    .SetConfiguration(Configuration)
                    .SetOutputDirectory(ArtifactsDirectory)
                    .EnableNoBuild());
            }
            finally
            {
                ReplaceTextInFiles(nuspecPath, $"<version>{OctoVersionInfo.FullSemVer}</version>", "<version>$version$</version>");
                ReplaceTextInFiles(nuspecPath, $"\\{Configuration.ToString()}\\", "\\$configuration$\\");
            }
        });

    [UsedImplicitly]
    Target CopyToLocalPackages => _ => _
        .OnlyWhenStatic(() => IsLocalBuild)
        .TriggeredBy(Pack)
        .Executes(() =>
        {
            EnsureExistingDirectory(LocalPackagesDirectory);
            CopyFileToDirectory(ArtifactsDirectory / $"{Solution.Name}.{OctoVersionInfo.FullSemVer}.nupkg", LocalPackagesDirectory, FileExistsPolicy.Overwrite);
        });

    void ReplaceTextInFiles(AbsolutePath path, string oldValue, string newValue)
    {
        var fileText = File.ReadAllText(path);
        fileText = fileText.Replace(oldValue, newValue);
        File.WriteAllText(path, fileText);
    }

    /// Support plugins are available for:
    /// - JetBrains ReSharper        https://nuke.build/resharper
    /// - JetBrains Rider            https://nuke.build/rider
    /// - Microsoft VisualStudio     https://nuke.build/visualstudio
    /// - Microsoft VSCode           https://nuke.build/vscode
    public static int Main() => Execute<Build>(x => x.Pack);
}