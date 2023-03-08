namespace SpaceEngineers.Core.DataAccess.Orm.Linq
{
    /// <summary>
    /// IInsertQueryable
    /// </summary>
    /// <typeparam name="T">T type-argument</typeparam>
    public interface IInsertQueryable<out T> : ICustomQueryable
    {
    }
}