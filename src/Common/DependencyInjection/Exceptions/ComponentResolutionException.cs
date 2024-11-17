namespace SpaceEngineers.Core.DependencyInjection.Exceptions;

using System;

public sealed class ComponentResolutionException : Exception
{
    public ComponentResolutionException(string message, Exception inner)
        : base(message, inner)
    {
    }

    public ComponentResolutionException(string message)
        : base(message, null)
    {
    }

    public ComponentResolutionException(Type service, Exception inner)
        : base($"Component for service {service} wasn't resolved", inner)
    {
    }
}