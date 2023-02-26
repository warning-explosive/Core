namespace SpaceEngineers.Core.DataAccess.Orm.Linq
{
    using System.Linq;

    /// <summary>
    /// Class that represents cached query (expression) to the database
    /// </summary>
    /// <typeparam name="T">T type-argument</typeparam>
    public interface ICachedQueryable<out T> : IQueryable<T>
    {
        /// <summary>
        /// Converts ICachedQueryable to regular IQueryable
        /// </summary>
        /// <returns>IQueryable</returns>
        IQueryable<T> AsQueryable();
    }
}