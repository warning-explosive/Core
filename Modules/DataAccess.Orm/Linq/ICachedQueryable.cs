namespace SpaceEngineers.Core.DataAccess.Orm.Linq
{
    using System.Linq;

    /// <summary>
    /// ICachedQueryable
    /// </summary>
    /// <typeparam name="T">T type-argument</typeparam>
    public interface ICachedQueryable<out T> : IQueryable<T>,
                                               ICustomQueryable
    {
    }
}