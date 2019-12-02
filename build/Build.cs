using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Force.Crc32;
using ICSharpCode.SharpZipLib.Zip;
using Nuke.Common;
using Nuke.Common.Execution;
using Nuke.Common.Git;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.GitVersion;
using Nuke.Common.Utilities.Collections;
using Octokit;
using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.IO.PathConstruction;
using static Nuke.Common.Tools.DotNet.DotNetTasks;
using FileMode = System.IO.FileMode;
using ZipFile = System.IO.Compression.ZipFile;

[assembly: InternalsVisibleTo("SpaBuildpackTests")]
[CheckBuildProjectConfigurations]
[UnsetVisualStudioEnvironmentVariables]
class Build : NukeBuild
{
    /// Support plugins are available for:
    ///   - JetBrains ReSharper        https://nuke.build/resharper
    ///   - JetBrains Rider            https://nuke.build/rider
    ///   - Microsoft VisualStudio     https://nuke.build/visualstudio
    ///   - Microsoft VSCode           https://nuke.build/vscode

    public enum StackType
    {
        Windows,
        Linux
    }
    public static int Main () => Execute<Build>(x => x.Publish);
    const string BuildpackProjectName = "SpaBuildpack";
    string PackageZipName => $"{BuildpackProjectName}-{Runtime}-{GitVersion.MajorMinorPatch}.zip";

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;
    
    [Parameter("Target CF stack type - 'windows' or 'linux'. Determines buildpack runtime (Framework or Core). Default is 'windows'")]
    readonly StackType Stack = StackType.Linux;
    
    [Parameter("GitHub personal access token with access to the repo")]
    string GitHubToken;

    [Parameter("Application directory against which buildpack will be applied")]
    readonly string ApplicationDirectory;

    string Runtime => Stack == StackType.Windows ? "win-x64" : "linux-x64";  
    string Framework => Stack == StackType.Windows ? "net472" : "netcoreapp3.0";


    [Solution] readonly Solution Solution;
    [GitRepository] readonly GitRepository GitRepository;
    [GitVersion] readonly GitVersion GitVersion;

    AbsolutePath SourceDirectory => RootDirectory / "src";
    AbsolutePath TestsDirectory => RootDirectory / "tests";
    AbsolutePath ArtifactsDirectory => RootDirectory / "artifacts";
    
    string[] LifecycleHooks = {"detect", "supply", "release", "finalize"};

    Target Clean => _ => _
        .Description("Cleans up **/bin and **/obj folders")
        .Before(Restore)
        .Executes(() =>
        {
            DoClean();
            //EnsureCleanDirectory(ArtifactsDirectory);
        });

    void DoClean()
    {
        SourceDirectory.GlobDirectories("**/bin", "**/obj").ForEach(DeleteDirectory);
        TestsDirectory.GlobDirectories("**/bin", "**/obj").ForEach(DeleteDirectory);
    }

    Target Restore => _ => _
        .Description("Restores NuGet dependencies for the buildpack")
        .Executes(() =>
        {
            DotNetRestore(s => s
                .SetProjectFile(Solution)
                .SetRuntime(Runtime));
        });

    Target Compile => _ => _
        .Description("Compiles the buildpack")
        .DependsOn(Restore)
        .Executes(() =>
        {
            
            Logger.Info(Stack);
            DotNetBuild(s => s
                .SetProjectFile(Solution)
                .SetConfiguration(Configuration)
                .SetFramework(Framework)
                .SetRuntime(Runtime)
                .SetAssemblyVersion(GitVersion.AssemblySemVer)
                .SetFileVersion(GitVersion.AssemblySemFileVer)
                .SetInformationalVersion(GitVersion.InformationalVersion)
                .EnableNoRestore());
        });
    
    Target Publish => _ => _
        .Description("Packages buildpack in Cloud Foundry expected format into /artifacts directory")
        .DependsOn(Restore)
        .Executes(() =>
        {
            var workDirectory = TemporaryDirectory / "pack";
            EnsureCleanDirectory(TemporaryDirectory);
            var buildpackProject = Solution.GetProject(BuildpackProjectName);
            var launcherProject = Solution.GetProject("SpaLauncher");
            var buildpackPublishDir = buildpackProject.Directory / "bin" / Configuration / Framework / Runtime / "publish";
            var launcherPublishDir = launcherProject.Directory / "bin" / Configuration / Framework / Runtime / "publish";
            var workBinDirectory = workDirectory / "bin";
            var launcherDirectory = workDirectory / "launcher";
            
            
            DotNetPublish(s => s
                .SetProject(Solution)
                .SetConfiguration(Configuration)
                .SetFramework(Framework)
                .SetRuntime(Runtime)
                .SetAssemblyVersion(GitVersion.AssemblySemVer)
                .SetFileVersion(GitVersion.AssemblySemFileVer)
                .SetInformationalVersion(GitVersion.InformationalVersion)
                //.SetProperties(new Dictionary<string,object>{{"TrimUnusedDependencies","true"}})
                .EnableNoRestore());



            var lifecycleBinaries = Solution.GetProjects("Lifecycle*")
                .Select(x => x.Directory / "bin" / Configuration / Framework / Runtime / "publish")
                .SelectMany(x => Directory.GetFiles(x).Where(path => LifecycleHooks.Any(hook => Path.GetFileName(path).StartsWith(hook))));

            foreach (var lifecycleBinary in lifecycleBinaries)
            {
                CopyFileToDirectory(lifecycleBinary, workBinDirectory, FileExistsPolicy.OverwriteIfNewer);
            }
            
            CopyDirectoryRecursively(buildpackPublishDir, workBinDirectory, DirectoryExistsPolicy.Merge);
            CopyDirectoryRecursively(launcherPublishDir, launcherDirectory, DirectoryExistsPolicy.Merge);
            var commonFiles = GetCommonFiles(buildpackPublishDir, launcherDirectory);
            var commonFilesManifest = workDirectory / "commonFiles.txt";
            using (var fileStream = File.Create(commonFilesManifest))
            {
                using var fsw = new StreamWriter(fileStream);
                foreach (var file in commonFiles)
                {
                    fsw.WriteLine(Path.GetFileName(file.Item2));
                    File.Delete(file.Item2);
                }
            }

            //            CopyDirectoryRecursively(scriptsDirectory, workBinDirectory, DirectoryExistsPolicy.Merge);
            var tempZipFile = TemporaryDirectory / PackageZipName;
            
            ZipFile.CreateFromDirectory(workDirectory, tempZipFile, CompressionLevel.NoCompression, false);
            MakeFilesInZipUnixExecutable(tempZipFile);
            CopyFileToDirectory(tempZipFile, ArtifactsDirectory, FileExistsPolicy.Overwrite);
            Logger.Block(ArtifactsDirectory / PackageZipName);

        });
    

    Target Release => _ => _
        .Description("Creates a GitHub release (or ammends existing) and uploads buildpack artifact")
        .DependsOn(Publish)
        .Requires(() => GitHubToken)
        .Executes(async () =>
        {
            if (!GitRepository.IsGitHubRepository())
                throw new Exception("Only supported when git repo remote is github");
            
            var client = new GitHubClient(new ProductHeaderValue(BuildpackProjectName))
            {
                Credentials = new Credentials(GitHubToken, AuthenticationType.Bearer)
            };
            var gitIdParts = GitRepository.Identifier.Split("/");
            var owner = gitIdParts[0];
            var repoName = gitIdParts[1];
            
            var releaseName = $"v{GitVersion.MajorMinorPatch}";
            Release release;
            try
            {
                release = await client.Repository.Release.Get(owner, repoName, releaseName);
            }
            catch (NotFoundException)
            {
                var newRelease = new NewRelease(releaseName)
                {
                    Name = releaseName, 
                    Draft = false, 
                    Prerelease = false
                };
                release = await client.Repository.Release.Create(owner, repoName, newRelease);
            }

            var existingAsset = release.Assets.FirstOrDefault(x => x.Name == PackageZipName);
            if (existingAsset != null)
            {
                await client.Repository.Release.DeleteAsset(owner, repoName, existingAsset.Id);
            }
            
            var zipPackageLocation = ArtifactsDirectory / PackageZipName;
            var stream = File.OpenRead(zipPackageLocation);
            var releaseAssetUpload = new ReleaseAssetUpload(PackageZipName, "application/zip", stream, TimeSpan.FromHours(1));
            var releaseAsset = await client.Repository.Release.UploadAsset(release, releaseAssetUpload);
            
            Logger.Block(releaseAsset.BrowserDownloadUrl);
        });

    Target Detect => _ => _
        .Description("Invokes buildpack 'detect' lifecycle event")
        .Requires(() => ApplicationDirectory)
        .Executes(async () =>
        {
            try
            {
                DotNetRun(s => s
                    .SetProjectFile(Solution.GetProject("Lifecycle.Detect").Path)
                    .SetApplicationArguments(ApplicationDirectory)
                    .SetConfiguration(Configuration)
                    .SetFramework(Framework)
                );
                Logger.Block($"Detect returned 'true'");
            }
            catch (ProcessException)
            {
                Logger.Block($"Detect returned 'false'");
            }

        });

    Target Supply => _ => _
        .Description("Invokes buildpack 'supply' lifecycle event")
        .Requires(() => ApplicationDirectory)
        .Executes(async () =>
        {
            
            var home = (AbsolutePath)Path.GetTempPath() / Guid.NewGuid().ToString();
            var app = home / "app";
            var deps = home / "deps";
            var index = 0;
            var cache = home / "cache";
            CopyDirectoryRecursively(ApplicationDirectory, app);
            
            DotNetRun(s => s
                .SetProjectFile(Solution.GetProject("Lifecycle.Supply").Path)
                .SetApplicationArguments($"{app} {cache} {app} {deps} {index}")
                .SetConfiguration(Configuration)
                .SetFramework(Framework)
            );
            Logger.Block($"Buildpack applied. Droplet is available in {home}");

        });

    public void MakeFilesInZipUnixExecutable(AbsolutePath zipFile)
    {
        var tmpFileName = zipFile + ".tmp";
        using (var input = new ZipInputStream(File.Open(zipFile, FileMode.Open)))
        using (var output = new ZipOutputStream(File.Open(tmpFileName, FileMode.Create)))
        {
            output.SetLevel(9);
            ZipEntry entry;
		
            while ((entry = input.GetNextEntry()) != null)
            {
                var outEntry = new ZipEntry(entry.Name);
                outEntry.HostSystem = (int)HostSystemID.Unix;
                var entryAttributes =  
                    ZipEntryAttributes.ReadOwner | 
                    ZipEntryAttributes.ReadOther | 
                    ZipEntryAttributes.ReadGroup |
                    ZipEntryAttributes.ExecuteOwner | 
                    ZipEntryAttributes.ExecuteOther | 
                    ZipEntryAttributes.ExecuteGroup;
                entryAttributes = entryAttributes | (entry.IsDirectory ? ZipEntryAttributes.Directory : ZipEntryAttributes.Regular);
                outEntry.ExternalFileAttributes = (int) (entryAttributes) << 16; // https://unix.stackexchange.com/questions/14705/the-zip-formats-external-file-attribute
                output.PutNextEntry(outEntry);
                input.CopyTo(output);
            }
            output.Finish();
            output.Flush();
        }

        DeleteFile(zipFile);
        RenameFile(tmpFileName,zipFile, FileExistsPolicy.Overwrite);
    }

    public void ExtractCommonFiles(string dir1, string dir2, string commonDir)
    {
        var files = GetCommonFiles(dir1, dir2);
        Directory.CreateDirectory(commonDir);
        foreach (var file in files)
        {
            var fileName = Path.GetFileName(file.Item1);
            MoveFile(file.Item1, Path.Combine(commonDir, fileName));
            DeleteFile(file.Item2);
        }
    }
    public List<Tuple<string, string>> GetCommonFiles(string dir1, string dir2)
    {
        var common = new List<Tuple<string,string>>();

        var dir1Info = new DirectoryInfo(dir1);
        var dir2Info = new DirectoryInfo(dir2);
        foreach(var file in  dir1Info.GetFiles())
        {
            var otherFile = new FileInfo(Path.Combine(dir2Info.FullName, file.Name));
		
            if(File.Exists(otherFile.FullName) && file.Length == otherFile.Length && Crc32Algorithm.Compute(File.ReadAllBytes(file.FullName)) == Crc32Algorithm.Compute(File.ReadAllBytes(otherFile.FullName)))
            {
                common.Add(Tuple.Create(file.FullName, otherFile.FullName));
            }
        }

        return common;
    }
    
    [Flags]
    enum ZipEntryAttributes
    {
        ExecuteOther = 1,
        WriteOther = 2,
        ReadOther = 4,
	
        ExecuteGroup = 8,
        WriteGroup = 16,
        ReadGroup = 32,

        ExecuteOwner = 64,
        WriteOwner = 128,
        ReadOwner = 256,

        Sticky = 512, // S_ISVTX
        SetGroupIdOnExecution = 1024,
        SetUserIdOnExecution = 2048,

        //This is the file type constant of a block-oriented device file.
        NamedPipe = 4096,
        CharacterSpecial = 8192,
        Directory = 16384,
        Block = 24576,
        Regular = 32768,
        SymbolicLink = 40960,
        Socket = 49152,
	
    }
}
