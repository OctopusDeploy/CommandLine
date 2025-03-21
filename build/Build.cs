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
using static Nuke.Common.Tools.DotNet.DotNetTasks;
using static Nuke.Common.Tools.SignTool.SignToolTasks;

[UnsetVisualStudioEnvironmentVariables]

[VerbosityMapping(typeof(DotNetVerbosity),
    Verbose = nameof(DotNetVerbosity.normal))]
class Build : NukeBuild
{
    /// Support plugins are available for:
    /// - JetBrains ReSharper        https://nuke.build/resharper
    /// - JetBrains Rider            https://nuke.build/rider
    /// - Microsoft VisualStudio     https://nuke.build/visualstudio
    /// - Microsoft VSCode           https://nuke.build/vscode
    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

    [Solution(GenerateProjects = true)] readonly Solution Solution;

    [Parameter(
        "Whether to auto-detect the branch name - this is okay for a local build, but should not be used under CI.")]
    readonly bool AutoDetectBranch = IsLocalBuild;

    [Parameter(
        "Branch name for OctoVersion to use to calculate the version number. Can be set via the environment variable OCTOVERSION_CurrentBranch.",
        Name = "OCTOVERSION_CurrentBranch")]
    readonly string BranchName;

    [OctoVersion(UpdateBuildNumber = true, Branch = nameof(BranchName),
        AutoDetectBranch = true, Framework = "net8.0")]
    readonly OctoVersionInfo OctoVersionInfo;

    AbsolutePath SourceDirectory => RootDirectory / "source";
    AbsolutePath ArtifactsDirectory => RootDirectory / "artifacts";
    AbsolutePath LocalPackagesDirectory => RootDirectory / ".." / "LocalPackages";
    AbsolutePath OctopusCommandLineFolder => SourceDirectory / "CommandLine";

    Target Clean => _ => _
        .Executes(() =>
        {
            SourceDirectory.GlobDirectories("**/bin", "**/obj").ForEach(x => x.CreateOrCleanDirectory());
            ArtifactsDirectory.CreateOrCleanDirectory();
        });

    Target Restore => _ => _
        .DependsOn(Clean)
        .Executes(() =>
        {
            DotNetRestore(_ => _
                .SetProjectFile(Solution));
        });

    Target Compile => _ => _
        .DependsOn(Restore)
        .Executes(() =>
        {
            DotNetBuild(_ => _
                .SetProjectFile(Solution)
                .SetConfiguration(Configuration)
                .SetVersion(OctoVersionInfo.FullSemVer)
                .SetInformationalVersion(OctoVersionInfo.InformationalVersion)
                .EnableNoRestore());
        });

    [PublicAPI]
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
        .DependsOn(Compile)
        .Executes(() =>
        {
            var targets = Solution.CommandLine.GetTargetFrameworks();

            foreach (var target in targets)
            {
                var inputFolder = OctopusCommandLineFolder / "bin" / Configuration / target;
                var outputFolder = OctopusCommandLineFolder / "bin" / Configuration / $"{target}-Merged";
                outputFolder.CreateOrCleanDirectory();

                // The call to ILRepack with .EnableInternalize() requires the Octopus.CommandLine.dll assembly to be first in the list.
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

                inputFolder.DeleteDirectory();
                outputFolder.MoveToDirectory(inputFolder);
            }
        });

    Target Pack => _ => _
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
                    .SetProcessAdditionalArguments("/p:NuspecFile=" + nuspec)
                    .SetVersion(OctoVersionInfo.FullSemVer)
                    .SetConfiguration(Configuration)
                    .SetOutputDirectory(ArtifactsDirectory)
                    .EnableNoBuild()
                    .DisableIncludeSymbols()
                    .SetVerbosity(DotNetVerbosity.normal)
                );
            }
            finally
            {
                ReplaceTextInFiles(nuspecPath, $"<version>{OctoVersionInfo.FullSemVer}</version>", "<version>$version$</version>");
                ReplaceTextInFiles(nuspecPath, $"\\{Configuration.ToString()}\\", "\\$configuration$\\");
            }

            static void ReplaceTextInFiles(AbsolutePath path, string oldValue, string newValue)
            {
                var fileText = File.ReadAllText(path);
                fileText = fileText.Replace(oldValue, newValue);
                File.WriteAllText(path, fileText);
            }
        });

    Target CopyToLocalPackages => _ => _
        .OnlyWhenStatic(() => IsLocalBuild)
        .TriggeredBy(Pack)
        .Executes(() =>
        {
            //EnsureExistingDirectory(LocalPackagesDirectory.exist);
            if (!LocalPackagesDirectory.Exists()) throw new DirectoryNotFoundException();
            
            ArtifactsDirectory.GlobFiles("*.nupkg")
                .ForEach(package => package.CopyToDirectory(LocalPackagesDirectory, ExistsPolicy.FileOverwrite));
        });

    Target Default => _ => _
        .DependsOn(Pack)
        .DependsOn(CopyToLocalPackages);

    public static int Main() => Execute<Build>(x => x.Default);
}