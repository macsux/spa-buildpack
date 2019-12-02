using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace SpaBuildpack
{
    public class SpaBuildpack : FinalBuildpack 
    {
        protected override bool Detect(string buildPath)
        {
            return true;
        }

        protected override void Apply(string buildPath, string cachePath, string depsPath, int index)
        {
            Console.WriteLine($"===Applying {nameof(SpaBuildpack)}===");

            var wwwRoot = Path.Combine(buildPath, "wwwroot");
            var buildpackDir = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "..");
            var launcherDir = Path.Combine(buildpackDir, "launcher");
            var tmpFolder = Path.Combine(cachePath, "tmp");
            Directory.Move(buildPath,tmpFolder);
            Directory.CreateDirectory(buildPath);
            Directory.Move(tmpFolder, wwwRoot);
            Directory.CreateDirectory(cachePath);
            CopyAll(new DirectoryInfo(launcherDir),new DirectoryInfo(buildPath));
            var sharedFiles = Path.Combine(buildpackDir, "commonFiles.txt");
            using (var commonFiles = File.OpenText(sharedFiles))
            {
                while (true)
                {
                    var fileName = commonFiles.ReadLine();
                    if (fileName == null)
                        break;
                    var from = Path.Combine(buildpackDir, "bin", fileName);
                    var to = Path.Combine(buildPath, fileName);
                    File.Copy(from, to);
                }
            }
        }
        
        public override string GetStartupCommand(string buildPath)
        {
            return "./SpaLauncher";
        }
        
        
        public static void CopyAll(DirectoryInfo source, DirectoryInfo target)
        {
            if (source.FullName.ToLower() == target.FullName.ToLower())
            {
                return;
            }

            // Check if the target directory exists, if not, create it.
            if (Directory.Exists(target.FullName) == false)
            {
                Directory.CreateDirectory(target.FullName);
            }

            // Copy each file into it's new directory.
            foreach (var fi in source.GetFiles())
            {
                fi.CopyTo(Path.Combine(target.ToString(), fi.Name), true);
            }

            // Copy each subdirectory using recursion.
            foreach (var diSourceSubDir in source.GetDirectories())
            {
                var nextTargetSubDir =
                    target.CreateSubdirectory(diSourceSubDir.Name);
                CopyAll(diSourceSubDir, nextTargetSubDir);
            }
        }

    }
}
