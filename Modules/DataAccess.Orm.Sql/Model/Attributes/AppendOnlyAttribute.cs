namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Model.Attributes
{
    using System;

    /// <summary>
    /// AppendOnlyAttribute
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class AppendOnlyAttribute : Attribute
    {
    }
}