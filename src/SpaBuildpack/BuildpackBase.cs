using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace SpaBuildpack
{
    public abstract class BuildpackBase
    {
        /// <summary>
        /// Dictionary of environmental variables to be set at runtime before the app starts
        /// </summary>
        protected Dictionary<string,string> EnvironmentalVariables { get; } = new Dictionary<string, string>();
        public bool IsLinux => RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
        
        /// <summary>
        /// Determines if the buildpack is compatible and should be applied to the application being staged.
        /// </summary>
        /// <param name="buildPath">Directory path to the application</param>
        /// <returns>True if buildpack should be applied, otherwise false</returns>
        protected abstract bool Detect(string buildPath);
        /// <summary>
        /// Logic to apply when buildpack is ran.
        /// Note that for <see cref="SupplyBuildpack"/> this will correspond to "bin/supply" lifecycle event, while for <see cref="FinalBuildpack"/> it will be invoked on "bin/finalize"
        /// </summary>
        /// <param name="buildPath">Directory path to the application</param>
        /// <param name="cachePath">Location the buildpack can use to store assets during the build process</param>
        /// <param name="depsPath">Directory where dependencies provided by all buildpacks are installed. New dependencies introduced by current buildpack should be stored inside subfolder named with index argument ({depsPath}/{index})</param>
        /// <param name="index">Number that represents the ordinal position of the buildpack</param>
        protected abstract void Apply(string buildPath, string cachePath, string depsPath, int index);
        
        /// <summary>
        /// Code that will execute during the run stage before the app is started
        /// </summary>
        protected virtual void PreStartup(string buildPath, string depsPath, int index)
        {
        }

        /// <summary>
        /// Entry point into the buildpack. Should be called from Main method with args
        /// </summary>
        /// <param name="args">Args array passed into Main method</param>
        /// <returns>Status return code</returns>
        public int Run(string[] args)
        {
            return DoRun(args);
        }

        protected virtual int DoRun(string[] args)
        {
            var command = args[0];
            switch (command)
            {
                case "detect":
                    return Detect(args[1]) ? 2 : 1;
                case "prestartup":
                    PreStartup(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), Environment.GetEnvironmentVariable("DEPS_DIR"), int.Parse(args[1]));
                    break;
            }

            return 0;
        }
        protected void DoApply(string buildPath, string cachePath, string depsPath, int index)
        {
            Apply(buildPath, cachePath, depsPath, index);
            
            var isPreStartOverriden = GetType().GetMethod(nameof(PreStartup), BindingFlags.Instance | BindingFlags.NonPublic)?.DeclaringType != typeof(BuildpackBase);
            var buildpackDepsDir = Path.Combine(depsPath, index.ToString());
            Directory.CreateDirectory(buildpackDepsDir);
            var profiled = Path.Combine(buildPath, ".profile.d");
            Directory.CreateDirectory(profiled);
            
            if (isPreStartOverriden) 
            {
                // copy buildpack to deps dir so we can invoke it as part of startup
                foreach(var file in Directory.EnumerateFiles(Path.GetDirectoryName(GetType().Assembly.Location)))
                {
                    File.Copy(file, Path.Combine(buildpackDepsDir, Path.GetFileName(file)), true);
                }

                var extension = !IsLinux ? ".exe" : string.Empty;
                var prestartCommand = $"{GetType().Assembly.GetName().Name}{extension} prestartup";
                // write startup shell script to call buildpack prestart lifecycle event in deps dir
                var startupScriptName = $"{index:00}_{nameof(SpaBuildpack)}_startup";
                if (IsLinux)
                {
                    File.WriteAllText(Path.Combine(profiled,$"{startupScriptName}.sh"), $"#!/bin/bash\n$DEPS_DIR/{index}/{prestartCommand} {index}");
                }
                else
                {
                    File.WriteAllText(Path.Combine(profiled,$"{startupScriptName}.bat"),$@"%DEPS_DIR%\{index}\{prestartCommand} {index}");
                }
            }

            if (EnvironmentalVariables.Any())
            {
                var envScriptName = $"{index:00}_{nameof(SpaBuildpack)}_env";
                if (IsLinux)
                {
                    var envVars = EnvironmentalVariables.Aggregate(new StringBuilder(), (sb,x) => sb.Append($"export {x.Key}={x.Value}\n"));
                    File.WriteAllText(Path.Combine(profiled,$"{envScriptName}.sh"), $"#!/bin/bash\n{envVars}");
                }
                else
                {
                    var envVars = EnvironmentalVariables.Aggregate(new StringBuilder(), (sb,x) => sb.Append($"SET {x.Key}={x.Value}\r\n"));
                    File.WriteAllText(Path.Combine(profiled,$"{envScriptName}.bat"),envVars.ToString());
                }
            }
            
        }
    }
}