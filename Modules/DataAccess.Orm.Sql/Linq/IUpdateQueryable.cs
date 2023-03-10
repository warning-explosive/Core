namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Linq
{
    /// <summary>
    /// IUpdateQueryable
    /// </summary>
    /// <typeparam name="T">T type-argument</typeparam>
    public interface IUpdateQueryable<out T> : ICustomQueryable
    {
    }
}