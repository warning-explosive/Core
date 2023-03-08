namespace SpaceEngineers.Core.DataAccess.Orm.Linq
{
    /// <summary>
    /// ICachedUpdateQueryable
    /// </summary>
    /// <typeparam name="T">T type-argument</typeparam>
    public interface ICachedUpdateQueryable<out T> : ICustomQueryable
    {
    }
}