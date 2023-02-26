namespace SpaceEngineers.Core.DataAccess.Api.Sql.Attributes
{
    using System;

    /// <summary>
    /// JsonColumnAttribute
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public sealed class JsonColumnAttribute : Attribute
    {
    }
}