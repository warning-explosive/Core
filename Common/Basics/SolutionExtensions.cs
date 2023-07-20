namespace SpaceEngineers.Core.Basics
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Xml.Linq;

    /// <summary>
    /// Solution extensions
    /// </summary>
    public static class SolutionExtensions
    {
        /// <summary>
        /// Find nearest solution file (*.sln)
        /// Has valid behavior only in test environment where we have full solution structure on disk
        /// </summary>
        /// <returns>Solution directory path</returns>
        /// <exception cref="DirectoryNotFoundException">If solution directory not found or depth in 42 nested directories exceeded</exception>
        public static FileInfo SolutionFile()
        {
            return FindFile("*.sln");
        }

        /// <summary>
        /// Find nearest project file (*.csproj)
        /// Has valid behavior only in test environment where we have full solution structure on disk
        /// </summary>
        /// <returns>Project directory path</returns>
        /// <exception cref="DirectoryNotFoundException">If project directory wasn't found or depth in 42 nested directories exceeded</exception>
        public static FileInfo ProjectFile()
        {
            return FindFile("*.csproj");
        }

        /// <summary>
        /// Returns all projects files (*.csproj) in specified solution directory
        /// Has valid behavior only in test environment where we have full solution structure on disk
        /// </summary>
        /// <param name="solutionDirectory">Solution directory (*.sln)</param>
        /// <returns>Projects files paths in specified solution directory</returns>
        public static FileInfo[] ProjectFiles(this DirectoryInfo solutionDirectory)
        {
            return solutionDirectory.GetFiles("*.csproj", SearchOption.AllDirectories);
        }

        /// <summary>
        /// Returns all source files (*.cs) in specified project directory
        /// Has valid behavior only in test environment where we have full solution structure on disk
        /// </summary>
        /// <param name="projectDirectory">Project directory (*.csproj)</param>
        /// <returns>Projects files paths in specified solution directory</returns>
        public static FileInfo[] SourceFiles(this DirectoryInfo projectDirectory)
        {
            return projectDirectory.GetFiles("*.cs", SearchOption.AllDirectories);
        }

        /// <summary>
        /// Returns assembly name of specified project file (.*csproj)
        /// </summary>
        /// <param name="csproj">Project file (.*csproj)</param>
        /// <returns>Assembly name of specified project</returns>
        /// <exception cref="InvalidOperationException">Throws if project file has invalid structure</exception>
        public static string AssemblyName(this FileInfo csproj)
        {
            var projectDocument = XDocument.Load(csproj.FullName, LoadOptions.None);

            if (projectDocument.Root == null
             || projectDocument.Root.Name != "Project")
            {
                throw new InvalidOperationException("Project file must contains project node as root");
            }

            return ExtractAssemblyNames(projectDocument.Root).InformativeSingle(Amb).Value;

            IEnumerable<XElement> ExtractAssemblyNames(XElement element)
            {
                return element
                    .Flatten(e => e.Elements())
                    .Where(e => e.Name == "AssemblyName");
            }

            static string Amb(IEnumerable<XElement> source)
            {
                return string.Join(Environment.NewLine, source.Select(e => e.Value));
            }
        }

        private static FileInfo FindFile(string pattern)
        {
            var baseDirectory = AppDomain.CurrentDomain.BaseDirectory.AsDirectoryInfo();

            var directory = baseDirectory;

            for (var i = 0;
                 !FileExist(directory, out _) && i < 42;
                 ++i)
            {
                directory = directory.Parent;
            }

            if (!FileExist(directory, out var file))
            {
                throw new DirectoryNotFoundException($"Directory with {pattern} wasn't found");
            }

            return file ?? throw new InvalidOperationException("File must exists");

            bool FileExist(DirectoryInfo directoryInfo, out FileInfo? fileInfo)
            {
                fileInfo = directoryInfo.GetFiles(pattern, SearchOption.TopDirectoryOnly)
                                        .FirstOrDefault();

                return fileInfo != null;
            }
        }
    }
}