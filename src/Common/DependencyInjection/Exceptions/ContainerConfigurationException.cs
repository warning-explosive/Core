namespace SpaceEngineers.Core.DependencyInjection.Exceptions;

using System;

public sealed class ContainerConfigurationException : Exception
{
    public ContainerConfigurationException(string message, Exception inner)
        : base(message, inner)
    {
    }

    public ContainerConfigurationException(string message)
        : base(message, null)
    {
    }

    public ContainerConfigurationException(Exception inner)
        : base("Container has invalid configuration", inner)
    {
    }
}