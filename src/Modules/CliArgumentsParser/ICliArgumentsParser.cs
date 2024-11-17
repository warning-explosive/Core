namespace SpaceEngineers.Core.CliArgumentsParser;

using System.Diagnostics.CodeAnalysis;

public interface ICliArgumentsParser
{
    T Parse<T>(string[] args)
        where T : class, new();

    bool TryParse<T>(string[] args, [NotNullWhen(true)] out T? arguments)
        where T : class, new();
}