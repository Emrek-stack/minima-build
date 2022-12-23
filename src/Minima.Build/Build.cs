using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.Build.Locator;
using Minima.Build.Utils;
using NGitLab;
using Nuke.Common;
using Nuke.Common.CI.GitHubActions;
using Nuke.Common.CI.TeamCity;
using Nuke.Common.Execution;
using Nuke.Common.Git;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.Git;
using Nuke.Common.Tools.GitVersion;
using Nuke.Common.Utilities;
using Nuke.Common.Utilities.Collections;
using Octokit;
using Serilog;
using static Nuke.Common.Tools.DotNet.DotNetTasks;
using Project = Nuke.Common.ProjectModel.Project;
using static Nuke.Common.IO.PathConstruction;

namespace Minima.Build;


[GitHubActions("continuous",
GitHubActionsImage.UbuntuLatest,
AutoGenerate = false,
FetchDepth = 0,
    OnPushBranches = new[]
    {
        "main",
        "dev",
        "releases/**"
    },
    OnPullRequestBranches = new[]
    {
        "releases/**"
    },
    InvokedTargets = new[]
    {
        nameof(Pack),
    },
    EnableGitHubToken = true,
    ImportSecrets = new[]
    {
        nameof(NuGetApiKey)
    }
)]


[CheckBuildProjectConfigurations]
[UnsetVisualStudioEnvironmentVariables]
internal partial class Build : NukeBuild
{

    TeamCity TeamCity => TeamCity.Instance;
    private static bool ClearTempBeforeExit { get; set; }
    private static int? _exitCode;

    [Parameter("Nuget Feed Url for Public Access of Pre Releases")]
    readonly string NugetFeed;
    [Parameter("Nuget Api Key"), Secret]
    readonly string NuGetApiKey;

    [GitVersion(NoFetch = true)]
    readonly GitVersion GitVersion;

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    public static Configuration Configuration { get; set; } =
    IsLocalBuild ? Configuration.Debug : Configuration.Release;


    [Parameter("GitHub user for release creation")]
    public static string GitHubUser { get; set; }

    [Parameter("GitHub user security token for release creation")]
    public static string GitHubToken { get; set; }

    [Parameter("Path to NuGet config")]
    public static AbsolutePath NugetConfig { get; set; }

    [Parameter("Defauld (start) project name")]
    public static string DefaultProject { get; set; } = "";

    [Parameter]
    public static string TestsFilter { get; set; } = "Category!=IntegrationTest";

    [Parameter]
    public static AbsolutePath CoverageReportPath { get; set; } = RootDirectory / ".tmp" / "coverage.xml";

    [Parameter("VersionTag for  Directory.Build.props")]
    public static string CustomVersionPrefix { get; set; }

    [Parameter("VersionSuffix for Directory.Build.props")]
    public static string CustomVersionSuffix { get; set; }

    [Parameter("Path to Artifacts Directory")]
    public static AbsolutePath ArtifactsDirectory { get; set; } = RootDirectory / "artifacts";

    [Parameter("Release branch")]
    public static string ReleaseBranch { get; set; }

    [Parameter("Force parameter for git checkout")]
    public static bool Force { get; set; }

    [Parameter("Http tasks timeout in seconds")]
    public static int HttpTimeout { get; set; } = 15;

    [Parameter("Path to Release Notes File")]
    public static AbsolutePath ReleaseNotes { get; set; }

    [Parameter("True - prerelease, False - release")]
    public static bool PreRelease { get; set; }

    [Parameter("Main branch")]
    public static string MainBranch { get; set; } = "master";

    [Parameter("Artifacts Type")]
    readonly string ArtifactsType;

    [Parameter("Excluded Artifacts Type")]
    readonly string ExcludedArtifactsType;


    static GitHubActions GitHubActions => GitHubActions.Instance;

    static readonly string PackageContentType = "application/octet-stream";
    static string ChangeLogFile => RootDirectory / "CHANGELOG.md";

    string GithubNugetFeed => GitHubActions != null
         ? $"https://nuget.pkg.github.com/{GitHubActions.RepositoryOwner}/index.json"
         : null;


    public static Solution Solution
    {
        get
        {
            var solutions = Directory.GetFiles(Directory.GetCurrentDirectory(), "*.sln", SearchOption.TopDirectoryOnly);
            if (solutions.Any())
            {
                return ProjectModelTasks.ParseSolution(solutions.First());
            }

            Log.Warning("No solution files found in the current directory");
            return new Solution();
        }
    }


    protected static Project WebProject => Solution?.AllProjects.FirstOrDefault(); //.FirstOrDefault(x =>x.Name.EndsWith(DefaultProject));

    protected GitRepository GitRepository => GitRepository.FromLocalDirectory(RootDirectory / ".git");
    protected static Microsoft.Build.Evaluation.Project MSBuildProject => WebProject?.GetMSBuildProject();

    protected string VersionSuffix => MSBuildProject?.GetProperty("VersionSuffix")?.EvaluatedValue;

    protected string ReleaseVersion => MSBuildProject?.GetProperty("PackageVersion")?.EvaluatedValue ??
                                   WebProject.GetProperty("Version");

    protected string VersionPrefix => MSBuildProject.GetProperty("VersionPrefix")?.EvaluatedValue;

    protected string GitRepositoryName => GitRepository.Identifier.Split('/')[1];
    protected string ZipFileName => $"{WebProject.Solution.Name}.{ReleaseVersion}.zip";


    protected static AbsolutePath SourceDirectory => RootDirectory / "src";
    protected static AbsolutePath TestsDirectory => RootDirectory / "tests";
    protected static AbsolutePath SamplesDirectory => RootDirectory / "samples";
    protected string ZipFilePath => ArtifactsDirectory / ZipFileName;
    protected AbsolutePath DirectoryBuildPropsPath => Solution.Directory / "Directory.Build.props";





    public Target Clean => _ => _
       .Before(Restore)
       .Executes(() =>
       {
           var searchPattern = new[] { "**/bin", "**/obj" };
           if (SourceDirectory.DirectoryExists())
           {
               SourceDirectory.GlobDirectories(searchPattern).ForEach(FileSystemTasks.DeleteDirectory);

               if (TestsDirectory.DirectoryExists())
               {
                   TestsDirectory.GlobDirectories(searchPattern).ForEach(FileSystemTasks.DeleteDirectory);
               }
           }
           else
           {
               RootDirectory.GlobDirectories(searchPattern).ForEach(FileSystemTasks.DeleteDirectory);
           }

           FileSystemTasks.EnsureCleanDirectory(ArtifactsDirectory);
       });

    public Target Restore => _ => _
    .Executes(() =>
    {
        DotNetRestore(settings => settings
            .SetProjectFile(Solution)
            .When(NugetConfig != null, c => c
                .SetConfigFile(NugetConfig))
        );
    });


    public Target Compile => _ => _
        .DependsOn(Restore)
        .Executes(() =>
        {
            DotNetBuild(settings => settings
                .SetProjectFile(Solution)
                .SetConfiguration(Configuration)
                .EnableNoRestore());
        });

    public Target Test => _ => _
    .DependsOn(Compile)
    .Executes(() =>
    {
        var testProjects = Solution.GetProjects("*.Test|*.Tests|*.Testing");
        var outPath = RootDirectory / ".tmp";

        foreach (var testProject in testProjects)
        {
            DotNet($"add {testProject.Path} package coverlet.collector");

            var testSetting = new DotNetTestSettings()
                .SetProjectFile(testProject.Path)
                .SetConfiguration(Configuration)
                .SetFilter(TestsFilter)
                .SetNoBuild(true)
                .SetProcessLogOutput(true)
                .SetResultsDirectory(outPath)
                .SetDataCollector("XPlat Code Coverage");

            DotNetTest(testSetting);
        }

        var coberturaReports = outPath.GlobFiles("**/coverage.cobertura.xml");

        if (coberturaReports.Count > 0)
        {
            var reportGenerator = ToolResolver.GetPackageTool("dotnet-reportgenerator-globaltool",
                "ReportGenerator.dll", "4.8.8", "netcoreapp3.0");
            reportGenerator.Invoke(
                $"-reports:{outPath / "**/coverage.cobertura.xml"} -targetdir:{outPath} -reporttypes:SonarQube");
            var sonarCoverageReportPath = outPath.GlobFiles("SonarQube.xml").FirstOrDefault();

            if (sonarCoverageReportPath == null)
            {
                Assert.Fail("No Coverage Report found");
            }

            FileSystemTasks.MoveFile(sonarCoverageReportPath, CoverageReportPath, FileExistsPolicy.Overwrite);
        }
        else
        {
            Log.Warning("No Coverage Reports found");
        }
    });

    public Target Pack => _ => _
    .DependsOn(Test)
    .Executes(() =>
    {
        //For platform take nuget package description from Directory.Build.props
        var settings = new DotNetPackSettings()
            .SetProject(Solution)
            .EnableNoBuild()
            .SetConfiguration(Configuration)
            .EnableIncludeSymbols()
            .SetSymbolPackageFormat(DotNetSymbolPackageFormat.snupkg)
            .SetOutputDirectory(ArtifactsDirectory)
            .SetVersion(ReleaseVersion);
        DotNetPack(settings);
    });


    public Target Publish => _ => _
    .DependsOn(Compile)
    .After(Test)
    .Executes(() =>
    {
        DotNetPublish(settings => settings
            .SetProcessWorkingDirectory(WebProject.Directory)
            .EnableNoRestore()
            .SetOutput(ArtifactsDirectory / "publish")
            .SetConfiguration(Configuration));
    });

    public Target Compress => _ => _
       .DependsOn(Clean, Test, Publish)
       .Executes(() =>
       {

               FileSystemTasks.DeleteFile(ZipFilePath);
               CompressionTasks.CompressZip(ArtifactsDirectory / "publish", ZipFilePath);

       });



    #region github


    Target PublishToGithub => _ => _
     .Description($"Publishing to Github for Development only.")
     .Triggers(StartRelease)
     .Requires(() => Configuration.Equals(Configuration.Release))
     .OnlyWhenStatic(() => GitRepository.IsOnDevelopBranch() || GitHubActions.IsPullRequest)
     .Executes(() =>
     {
         GlobFiles(ArtifactsDirectory, ArtifactsType)
             .Where(x => !x.EndsWith(ExcludedArtifactsType))
             .ForEach(x =>
             {
                 DotNetNuGetPush(s => s
                     .SetTargetPath(x)
                     .SetSource(GithubNugetFeed)
                     .SetApiKey(GitHubActions.Token)
                     .EnableSkipDuplicate()
                 );
             });
     });

    #endregion

    #region nuget
    Target PublishToNuGet => _ => _
      .Description($"Publishing to NuGet with the version.")
      .Triggers(StartRelease)
      .Requires(() => Configuration.Equals(Configuration.Release))
      .OnlyWhenStatic(() => GitRepository.IsOnMainOrMasterBranch())
      .Executes(() =>
      {
          GlobFiles(ArtifactsDirectory, ArtifactsType)
              .Where(x => !x.EndsWith(ExcludedArtifactsType))
              .ForEach(x =>
              {
                  DotNetNuGetPush(s => s
                      .SetTargetPath(x)
                      .SetSource(NugetFeed)
                      .SetApiKey(NuGetApiKey)
                      .EnableSkipDuplicate()
                  );
              });
      });

    #endregion

    #region versioning
    public Target ChangeVersion => _ => _
    .Executes(() =>
    {
        if ((string.IsNullOrEmpty(VersionSuffix) && !CustomVersionSuffix.IsNullOrEmpty()) ||
            !CustomVersionPrefix.IsNullOrEmpty())
        {
            ChangeProjectVersion(CustomVersionPrefix, CustomVersionSuffix);
        }
    });

    public Target IncrementMinor => _ => _
    .Triggers(ChangeVersion)
    .Executes(IncrementVersionMinor);

    public Target IncrementPatch => _ => _
        .Triggers(ChangeVersion)
        .Executes(IncrementVersionPatch);
    #endregion

    #region release

    public Target Release => _ => _
        .DependsOn(Clean, Compress)
        .Requires(() => GitHubUser, () => GitHubToken)
        .Executes(async () =>
        {
            var tag = ReleaseVersion;
            var description = File.Exists(ReleaseNotes) ? File.ReadAllText(ReleaseNotes) : "";

            try
            {
                await PublishRelease(GitHubUser, GitRepositoryName, GitHubToken, tag, description, ZipFilePath,
                    PreRelease);
            }
            catch (AggregateException ex)
            {
                foreach (var innerException in ex.Flatten().InnerExceptions)
                {
                    if (innerException is ApiValidationException exception)
                    {
                        var responseString = exception?.HttpResponse?.Body.ToString() ?? string.Empty;
                        var responseDocument = JsonDocument.Parse(responseString);
                        var alreadyExistsError = false;

                        if (responseDocument.RootElement.TryGetProperty("errors", out var errors))
                        {
                            var errorCount = errors.GetArrayLength();

                            if (errorCount > 0)
                            {
                                alreadyExistsError = errors.EnumerateArray().Any(e =>
                                    e.GetProperty("code").GetString() == "already_exists");
                            }
                        }

                        if (alreadyExistsError)
                        {
                            _exitCode = 422;
                        }

                        Log.Error($"Api Validation Error: {responseString}");
                    }
                    else
                    {
                        Log.Error(innerException, "Error");
                    }
                }

                Assert.Fail("Publish Release Failed");
            }
        });

    public Target StartRelease => _ => _
       .Executes(() =>
       {
           GitTasks.GitLogger = GitLogger;
           var disableApproval = Environment.GetEnvironmentVariable("VCBUILD_DISABLE_RELEASE_APPROVAL");

           if (disableApproval.IsNullOrEmpty() && !Force)
           {
               Console.Write($"Are you sure you want to release {GitRepository.Identifier}? (y/N): ");
               var response = Console.ReadLine();

               if (string.Compare(response, "y", true, CultureInfo.InvariantCulture) != 0)
               {
                   Assert.Fail("Aborted");
               }
           }

           var checkoutCommand = new StringBuilder("checkout dev");

           if (Force)
           {
               checkoutCommand.Append(" --force");
           }

           GitTasks.Git(checkoutCommand.ToString());
           GitTasks.Git("pull");

           var version = ReleaseVersion;

           var releaseBranchName = $"release/{version}";
           Log.Information(Directory.GetCurrentDirectory());
           GitTasks.Git($"checkout -B {releaseBranchName}");
           GitTasks.Git($"push -u origin {releaseBranchName}");
       });

    public Target CompleteRelease => _ => _
    .After(StartRelease)
    .Executes(() =>
    {
        var currentBranch = GitTasks.GitCurrentBranch();
        //Master
        GitTasks.Git($"checkout {MainBranch}");
        GitTasks.Git("pull");
        GitTasks.Git($"merge {currentBranch}");
        GitTasks.Git($"push origin {MainBranch}");
        //Dev
        GitTasks.Git("checkout dev");
        GitTasks.Git($"merge {currentBranch}");
        IncrementVersionMinor();
        ChangeProjectVersion(CustomVersionPrefix);
        string filesToAdd;



       var manifestPath = "";
       filesToAdd = $"Directory.Build.props {manifestPath}";


        GitTasks.Git($"add {filesToAdd}");
        GitTasks.Git($"commit -m \"{CustomVersionPrefix}\"");
        GitTasks.Git("push origin dev");
        //remove release branch
        GitTasks.Git($"branch -d {currentBranch}");
        GitTasks.Git($"push origin --delete {currentBranch}");
    });

    public Target QuickRelease => _ => _
    .DependsOn(StartRelease, CompleteRelease);

    public Target StartHotfix => _ => _
        .Executes(() =>
        {
            GitTasks.Git($"checkout {MainBranch}");
            GitTasks.Git("pull");
            IncrementVersionPatch();
            var hotfixBranchName = $"hotfix/{CustomVersionPrefix}";
            Log.Information(Directory.GetCurrentDirectory());
            GitTasks.Git($"checkout -b {hotfixBranchName}");
            ChangeProjectVersion(CustomVersionPrefix);
            var manifestPath = "";
            GitTasks.Git($"add Directory.Build.props {manifestPath}");
            GitTasks.Git($"commit -m \"{CustomVersionPrefix}\"");
            GitTasks.Git($"push -u origin {hotfixBranchName}");
        });

    public Target CompleteHotfix => _ => _
     .After(StartHotfix)
     .Executes(() =>
     {
         //workaround for run from sources
         var currentBranch = GitTasks.GitCurrentBranch();
         //Master
         GitTasks.Git($"checkout {MainBranch}");
         GitTasks.Git($"merge {currentBranch}");
         GitTasks.Git($"tag {VersionPrefix}");
         GitTasks.Git($"push origin {MainBranch}");
         //remove hotfix branch
         GitTasks.Git($"branch -d {currentBranch}");
         GitTasks.Git($"push origin --delete {currentBranch}");
     });

    #endregion

    #region private methods

    private static void GitLogger(OutputType type, string text)
    {
        if (text.Contains("github returned 422 Unprocessable Entity") && text.Contains("already_exists"))
        {
            _exitCode = 422;
        }

        if (text.Contains("nothing to commit, working tree clean"))
        {
            _exitCode = 423;
        }

        switch (type)
        {
            case OutputType.Err:
                Log.Error(text);
                break;

            case OutputType.Std:
                Log.Information(text);
                break;
        }
    }

    private async Task PublishRelease(string owner, string repo, string token, string tag, string description,
string artifactPath, bool prerelease)
    {
        var tokenAuth = new Credentials(token);


        var gitlabClient = new GitLabClient("http://gitstage.dsans.int", token);

        //var githubClient = new GitHubClient(new ProductHeaderValue("nsn-build")) { Credentials = tokenAuth };

        var newRelease = new NewRelease(tag)
        {
            Name = tag,
            Prerelease = prerelease,
            Draft = false,
            Body = description,
            TargetCommitish = GitTasks.GitCurrentBranch()
        };

        var newReleaseData = new NGitLab.Models.ReleaseCreate()
        {
            Name = tag,
            TagName = tag,
            Ref = GitTasks.GitCurrentBranch(),
            Description = description,
            ReleasedAt= DateTime.UtcNow,
        };


        var aa = gitlabClient.GetRepository(44);

        var rr = gitlabClient.GetReleases(44);
        var data = await rr.CreateAsync(newReleaseData);

        //var release = await githubClient.Repository.Release.Create(owner, repo, newRelease);

        using (var artifactStream = File.OpenRead(artifactPath))
        {
            var assetUpload = new ReleaseAssetUpload
            {
                FileName = Path.GetFileName(artifactPath),
                ContentType = "application/zip",
                RawData = artifactStream
            };

            //newReleaseData.Assets = new NGitLab.Models.ReleaseAssetsInfo();
            //newReleaseData.Assets.Sources

            //await githubClient.Repository.Release.UploadAsset(release, assetUpload);
            //await githubClient.Repository.Release.UploadAsset(release, assetUpload);
        }
    }
    #endregion

    public static int Main(string[] args)
    {
        if (!MSBuildLocator.IsRegistered)
        {
            MSBuildLocator.RegisterDefaults();
        }

        Environment.SetEnvironmentVariable("NUKE_TELEMETRY_OPTOUT", "1");
        if (args.ElementAtOrDefault(0)?.ToLowerInvariant() == "help" || args.Length == 0)
        {
            if (args.Length >= 2)
            {
                 var help = Nesine.Build.HelpProvider.HelpProvider.GetHelpForTarget(args[1]);
                 Console.WriteLine(help);
            }
            else if (args.Length <= 1)
            {
                var targets = Nesine.Build.HelpProvider.HelpProvider.GetTargets();
                var stringBuilder = new StringBuilder("There is a help for targets:" + Environment.NewLine);
                targets.ForEach(target => stringBuilder = stringBuilder.AppendLine($"- {target}"));
                Console.WriteLine(stringBuilder.ToString());
            }

            Environment.Exit(0);
        }

        var currentDirectory = Directory.GetCurrentDirectory();

        var nukeFiles = Directory.GetFiles(currentDirectory, ".nuke");

        if (!nukeFiles.Any() && !Directory.Exists(Path.Join(currentDirectory, ".nuke")))
        {
            Console.WriteLine("No .nuke file found!");
            var solutions = Directory.GetFiles(currentDirectory, "*.sln");

            if (solutions.Length == 1)
            {
                var solutionFileName = Path.GetFileName(solutions.First());
                Console.WriteLine($"Solution found: {solutionFileName}");
                CreateDotNuke(currentDirectory, solutionFileName);
            }
            else if (solutions.Length < 1)
            {
                CreateDotNuke(currentDirectory);
            }
        }
        else if (nukeFiles.Any())
        {
            var nukeFile = nukeFiles.First();
            ConvertDotNukeFile(nukeFile);
        }

        if (ClearTempBeforeExit)
        {
            FileSystemTasks.DeleteDirectory(TemporaryDirectory);
        }

        var exitCode = Execute<Build>(x => x.Compile);
        return _exitCode ?? exitCode;
    }

    public void ChangeProjectVersion(string versionPrefix = null, string versionSuffix = null)
    {
            //Directory.Build.props
            var xmlDocument = new XmlDocument { PreserveWhitespace = true };

            xmlDocument.LoadXml(File.ReadAllText(DirectoryBuildPropsPath));

            if (!string.IsNullOrEmpty(versionPrefix))
            {
                var prefixNode = xmlDocument.GetElementsByTagName("VersionPrefix")[0];

                if (prefixNode != null)
                {
                    prefixNode.InnerText = versionPrefix;
                }
            }

            if (string.IsNullOrEmpty(VersionSuffix) && !string.IsNullOrEmpty(versionSuffix))
            {
                var suffixNode = xmlDocument.GetElementsByTagName("VersionSuffix")[0];

                if (suffixNode != null)
                {
                    suffixNode.InnerText = versionSuffix;
                }
            }

            using (var writer = new Utf8StringWriter())
            {
                xmlDocument.Save(writer);
                File.WriteAllText(DirectoryBuildPropsPath, writer.ToString());
            }

    }

    public void IncrementVersionMinor()
    {
        Log.Information(WebProject.Directory);
        var version = new Version(VersionPrefix);
        var newPrefix = $"{version.Major}.{version.Minor + 1}.{version.Build}";
        CustomVersionPrefix = newPrefix;
    }

    public void IncrementVersionPatch()
    {
        var version = new Version(VersionPrefix);
        var newPrefix = $"{version.Major}.{version.Minor}.{version.Build + 1}";
        CustomVersionPrefix = newPrefix;
    }

    protected override void OnBuildCreated()
    {
        HttpTasks.DefaultTimeout = TimeSpan.FromSeconds(HttpTimeout);
        base.OnBuildCreated();
    }

    private static void ConvertDotNukeFile(string path)
    {
        var directory = Path.GetDirectoryName(path);
        var solutionPath = File.ReadLines(path).FirstOrDefault();
        FileSystemTasks.DeleteFile(path);
        CreateDotNuke(directory, solutionPath);
    }

    private static void CreateDotNuke(string path, string solutionPath = "")
    {
        var dotnukeDir = Path.Join(path, ".nuke");
        var paramsFilePath = Path.Join(dotnukeDir, "parameters.json");
        FileSystemTasks.EnsureExistingDirectory(dotnukeDir);
        var parameters = new NukeParameters { Solution = solutionPath };
        SerializationTasks.JsonSerializeToFile(parameters, paramsFilePath);
    }

}
public class Utf8StringWriter : StringWriter
{
    // Use UTF8 encoding but write no BOM to the wire
    public override Encoding Encoding => new UTF8Encoding(false);
}
