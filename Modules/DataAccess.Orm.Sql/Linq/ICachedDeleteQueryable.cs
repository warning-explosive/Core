namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Linq
{
    /// <summary>
    /// ICachedDeleteQueryable
    /// </summary>
    /// <typeparam name="T">T type-argument</typeparam>
    public interface ICachedDeleteQueryable<out T> : ICustomQueryable
    {
    }
}