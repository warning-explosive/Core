namespace SpaceEngineers.Core.DataAccess.Orm.Linq
{
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// IAsyncQueryable
    /// </summary>
    /// <typeparam name="T">T type-argument</typeparam>
    public interface IAsyncQueryable<out T> : IQueryable<T>, IAsyncEnumerable<T>
    {
    }
}