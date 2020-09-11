namespace SpaceEngineers.Core.Basics
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Xml.Linq;

    /// <summary>
    /// Solution extensions
    /// </summary>
    public static class SolutionExtensions
    {
        /// <summary>
        /// Find solution directory
        /// Has valid behavior only in test environment where we have full solution structure on disk
        /// </summary>
        /// <returns>Solution directory path</returns>
        /// <exception cref="DirectoryNotFoundException">If solution directory not found or depth in 42 nested directories exceeded</exception>
        public static string SolutionDirectory()
        {
            return FindDirectory("*.sln");
        }

        /// <summary>
        /// Find project directory
        /// Has valid behavior only in test environment where we have full solution structure on disk
        /// </summary>
        /// <returns>Project directory path</returns>
        /// <exception cref="DirectoryNotFoundException">If project directory not found or depth in 42 nested directories exceeded</exception>
        public static string ProjectDirectory()
        {
            return FindDirectory("*.csproj");
        }

        /// <summary>
        /// Returns all projects files paths (*.csproj) in specified solution directory
        /// Has valid behavior only in test environment where we have full solution structure on disk
        /// </summary>
        /// <param name="solutionDirectory">Solution directory path</param>
        /// <returns>Projects files paths in specified solution directory</returns>
        public static string[] Projects(this string solutionDirectory)
        {
            return Directory.GetFiles(solutionDirectory, "*.csproj", SearchOption.AllDirectories);
        }

        /// <summary>
        /// Returns assembly name of specified project file (.*csproj)
        /// </summary>
        /// <param name="projectFilePath">Project file path (.*csproj)</param>
        /// <returns>Assembly name of specified project</returns>
        /// <exception cref="InvalidOperationException">Throws if project file has invalid structure</exception>
        public static string AssemblyName(this string projectFilePath)
        {
            var projectDocument = XDocument.Load(projectFilePath, LoadOptions.None);

            if (projectDocument.Root == null
             || projectDocument.Root.Name != "Project")
            {
                throw new InvalidOperationException("Project file must contains project node as root");
            }

            return ExtractAssemblyNames(projectDocument.Root).InformativeSingle(Amb).Value;

            IEnumerable<XElement> ExtractAssemblyNames(XElement element)
            {
                return element.Elements().Where(e => e.Name == "AssemblyName")
                              .Concat(element.Elements().SelectMany(ExtractAssemblyNames));
            }

            string Amb(IEnumerable<XElement> source)
            {
                return string.Join(Environment.NewLine, source.Select(e => e.Value));
            }
        }

        private static string FindDirectory(string pattern)
        {
            var assembly = Assembly.GetExecutingAssembly()
                                   .EnsureNotNull("ExecutingAssembly must exists");

            var directory = new FileInfo(assembly.Location).Directory.FullName;

            for (var i = 0;
                 !SolutionFileExist(directory) && i < 42;
                 ++i)
            {
                directory = Path.Combine(directory, "..");
            }

            if (!SolutionFileExist(directory))
            {
                throw new DirectoryNotFoundException($"Directory with {pattern} not found");
            }

            return new DirectoryInfo(directory).FullName;

            bool SolutionFileExist(string dir) => Directory.GetFiles(dir, pattern, SearchOption.TopDirectoryOnly).Any();
        }
    }
}