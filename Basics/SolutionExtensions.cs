namespace SpaceEngineers.Core.Basics
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Xml.Linq;
    using Exceptions;

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
            var assembly = Assembly.GetExecutingAssembly()
                                   .EnsureNotNull("ExecutingAssembly must exists");

            var solutionDirectory = new FileInfo(assembly.Location).Directory.FullName;

            for (var i = 0;
                 !SolutionFileExist(solutionDirectory) && i < 42;
                 ++i)
            {
                solutionDirectory = Path.Combine(solutionDirectory, "..");
            }

            if (!SolutionFileExist(solutionDirectory))
            {
                throw new DirectoryNotFoundException("SolutionDirectory not found");
            }

            return new DirectoryInfo(solutionDirectory).FullName;

            bool SolutionFileExist(string dir) => Directory.GetFiles(dir, "*.sln", SearchOption.TopDirectoryOnly).Any();
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
    }
}