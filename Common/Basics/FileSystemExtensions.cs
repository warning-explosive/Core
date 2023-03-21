namespace SpaceEngineers.Core.Basics
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    /// <summary>
    /// File system extensions
    /// </summary>
    public static class FileSystemExtensions
    {
        /// <summary>
        /// Ges relative FileInfo
        /// </summary>
        /// <param name="sourceDirectory">Source directory</param>
        /// <param name="relativePath">Relative file path</param>
        /// <returns>Relative FileInfo</returns>
        public static FileInfo RelativeFile(this DirectoryInfo sourceDirectory, string relativePath)
        {
            var path = Path.Combine(sourceDirectory.FullName, relativePath);
            var info = new FileInfo(path);
            return info.Exists
                ? info
                : throw new DirectoryNotFoundException(path);
        }

        /// <summary>
        /// Step into specified directory
        /// </summary>
        /// <param name="source">Source directory</param>
        /// <param name="to">Target child directory</param>
        /// <param name="inner">Child directory (is step was successful)</param>
        /// <returns>Inner DirectoryInfo</returns>
        /// <exception cref="DirectoryNotFoundException">Directory doesn't contains specified subdirectories</exception>
        public static bool TryStepInto(this DirectoryInfo source, string to, out DirectoryInfo? inner)
        {
            inner = source
               .EnumerateDirectories()
               .Where(it => string.Equals(it.Name, to, StringComparison.OrdinalIgnoreCase))
               .InformativeSingleOrDefault(Amb);

            return inner != null;
        }

        /// <summary>
        /// Step into specified directory
        /// </summary>
        /// <param name="source">Source directory</param>
        /// <param name="to">Target child directory</param>
        /// <param name="additionalTargets">Additional child directories</param>
        /// <returns>Inner DirectoryInfo</returns>
        /// <exception cref="DirectoryNotFoundException">Directory doesn't contains specified subdirectories</exception>
        public static DirectoryInfo StepInto(this DirectoryInfo source, string to, params string[] additionalTargets)
        {
            return new[] { to }
               .Concat(additionalTargets)
               .Aggregate(source,
                    (acc, next) => acc
                       .EnumerateDirectories()
                       .Where(it => string.Equals(it.Name, next, StringComparison.OrdinalIgnoreCase))
                       .InformativeSingleOrDefault(Amb) ?? throw new DirectoryNotFoundException(Path.Combine(acc.FullName, next)));
        }

        /// <summary>
        /// Try get file from directory with specified name (without extension)
        /// </summary>
        /// <param name="directory">Source directory</param>
        /// <param name="fileNameWithoutExtension">File name without extension</param>
        /// <param name="extension">File extension (optional)</param>
        /// <param name="info">Found file</param>
        /// <returns>Inner FileInfo</returns>
        public static bool TryGetFile(
            this DirectoryInfo directory,
            string fileNameWithoutExtension,
            string? extension,
            out FileInfo? info)
        {
            info = directory
               .EnumerateFiles()
               .Where(file => EqualsFileName(file, fileNameWithoutExtension, extension))
               .InformativeSingleOrDefault(Amb);

            return info != null;
        }

        /// <summary>
        /// Get file from directory with specified name (without extension)
        /// </summary>
        /// <param name="directory">Source directory</param>
        /// <param name="fileNameWithoutExtension">File name without extension</param>
        /// <param name="extension">File extension (optional)</param>
        /// <returns>Inner FileInfo</returns>
        /// <exception cref="FileNotFoundException">Directory doesn't contains specified file</exception>
        public static FileInfo GetFile(
            this DirectoryInfo directory,
            string fileNameWithoutExtension,
            string? extension = null)
        {
            return directory
                       .EnumerateFiles()
                       .Where(file => EqualsFileName(file, fileNameWithoutExtension, extension))
                       .InformativeSingleOrDefault(Amb)
                   ?? throw new FileNotFoundException(Path.Combine(directory.FullName, fileNameWithoutExtension));
        }

        /// <summary>
        /// Converts string directory path to DirectoryInfo
        /// </summary>
        /// <param name="path">String directory path</param>
        /// <returns>DirectoryInfo</returns>
        /// <exception cref="DirectoryNotFoundException">Specified directory doesn't exist</exception>
        public static DirectoryInfo AsDirectoryInfo(this string path)
        {
            var info = new DirectoryInfo(path);

            return info.Exists
                ? info
                : throw new DirectoryNotFoundException(path);
        }

        /// <summary>
        /// Converts string file path to FileInfo
        /// </summary>
        /// <param name="path">String file path</param>
        /// <returns>FileInfo</returns>
        /// <exception cref="FileNotFoundException">Specified file doesn't exist</exception>
        public static FileInfo AsFileInfo(this string path)
        {
            var info = new FileInfo(path);

            return info.Exists
                ? info
                : throw new FileNotFoundException(path);
        }

        /// <summary>
        /// Setup FileInfo with specified extension
        /// </summary>
        /// <param name="fileInfo">Source FileInfo</param>
        /// <param name="extension">File extension</param>
        /// <returns>FileInfo with specified extension</returns>
        public static FileInfo WithExtension(this FileInfo fileInfo, string extension)
        {
            return new FileInfo(Path.ChangeExtension(fileInfo.FullName, extension));
        }

        /// <summary>
        /// Gets file name without extension
        /// </summary>
        /// <param name="fileInfo">FileInfo</param>
        /// <returns>File name without extension</returns>
        public static string NameWithoutExtension(this FileInfo fileInfo)
        {
            return fileInfo.Name.Substring(0, fileInfo.Name.Length - fileInfo.Extension.Length);
        }

        /// <summary>
        /// Gets file name without extension
        /// </summary>
        /// <param name="fileName">fileName</param>
        /// <returns>File name without extension</returns>
        public static string NameWithoutExtension(this string fileName)
        {
            var extension = fileName
               .Split(".", StringSplitOptions.RemoveEmptyEntries)
               .LastOrDefault();

            if (extension == null)
            {
                return fileName;
            }

            return fileName.Substring(0, fileName.Length - extension.Length - 1);
        }

        private static bool EqualsFileName(FileInfo file, string fileNameWithoutExtension, string? extension)
        {
            string left, right;

            if (extension.IsNullOrEmpty())
            {
                left = file.NameWithoutExtension();
                right = fileNameWithoutExtension;
            }
            else
            {
                left = file.Name;
                right = fileNameWithoutExtension + extension;
            }

            return string.Equals(left, right, StringComparison.OrdinalIgnoreCase);
        }

        private static string Amb(IEnumerable<FileSystemInfo> source)
        {
            return string.Join(Environment.NewLine, source.Select(info => info.FullName));
        }
    }
}