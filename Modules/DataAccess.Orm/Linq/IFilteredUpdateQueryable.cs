namespace SpaceEngineers.Core.DataAccess.Orm.Linq
{
    /// <summary>
    /// IFilteredUpdateQueryable
    /// </summary>
    /// <typeparam name="T">T type-argument</typeparam>
    public interface IFilteredUpdateQueryable<out T> : ICustomQueryable
    {
    }
}