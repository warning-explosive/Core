namespace SpaceEngineers.Core.Basics;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public static class FileSystemExtensions
{
    public static FileInfo RelativeFile(this DirectoryInfo sourceDirectory, string relativePath)
    {
        var path = Path.Combine(sourceDirectory.FullName, relativePath);
        var info = new FileInfo(path);
        return info.Exists
            ? info
            : throw new DirectoryNotFoundException(path);
    }

    public static bool TryStepInto(this DirectoryInfo source, string to, out DirectoryInfo? inner)
    {
        inner = source
            .EnumerateDirectories()
            .Where(it => string.Equals(it.Name, to, StringComparison.OrdinalIgnoreCase))
            .InformativeSingleOrDefault(Amb);

        return inner != null;
    }

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

    public static DirectoryInfo AsDirectoryInfo(this string path)
    {
        var info = new DirectoryInfo(path);

        return info.Exists
            ? info
            : throw new DirectoryNotFoundException(path);
    }

    public static FileInfo AsFileInfo(this string path)
    {
        var info = new FileInfo(path);

        return info.Exists
            ? info
            : throw new FileNotFoundException(path);
    }

    public static FileInfo WithExtension(this FileInfo fileInfo, string extension)
    {
        return new FileInfo(Path.ChangeExtension(fileInfo.FullName, extension));
    }

    public static string NameWithoutExtension(this FileInfo fileInfo)
    {
        return fileInfo.Name.Substring(0, fileInfo.Name.Length - fileInfo.Extension.Length);
    }

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
        return source
            .Select(info => info.FullName)
            .ToString(Environment.NewLine);
    }
}