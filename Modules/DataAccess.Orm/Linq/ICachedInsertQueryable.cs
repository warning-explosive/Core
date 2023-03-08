namespace SpaceEngineers.Core.DataAccess.Orm.Linq
{
    /// <summary>
    /// ICachedInsertQueryable
    /// </summary>
    /// <typeparam name="T">T type-argument</typeparam>
    public interface ICachedInsertQueryable<out T> : ICustomQueryable
    {
    }
}