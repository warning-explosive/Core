namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Linq
{
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;

    /// <summary>
    /// ISetUpdateQueryable
    /// </summary>
    /// <typeparam name="T">T type-argument</typeparam>
    [SuppressMessage("Analysis", "CA1010", Justification = "custom orm features")]
    public interface ISetUpdateQueryable<out T> : IQueryable
    {
    }
}