namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Linq
{
    /// <summary>
    /// IDeleteQueryable
    /// </summary>
    /// <typeparam name="T">T type-argument</typeparam>
    public interface IDeleteQueryable<out T> : ICustomQueryable
    {
    }
}