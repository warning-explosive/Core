namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Linq
{
    /// <summary>
    /// IFilteredDeleteQueryable
    /// </summary>
    /// <typeparam name="T">T type-argument</typeparam>
    public interface IFilteredDeleteQueryable<out T> : ICustomQueryable
    {
    }
}