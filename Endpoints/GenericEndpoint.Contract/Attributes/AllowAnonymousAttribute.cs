namespace SpaceEngineers.Core.GenericEndpoint.Contract.Attributes
{
    using System;

    /// <summary>
    /// Allows anonymously execute message handler
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class AllowAnonymousAttribute : Attribute
    {
    }
}