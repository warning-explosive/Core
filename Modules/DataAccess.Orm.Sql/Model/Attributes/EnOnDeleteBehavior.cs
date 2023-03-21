namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Model.Attributes
{
    /// <summary>
    /// EnDeleteBehavior
    /// </summary>
    public enum EnOnDeleteBehavior
    {
        /// <summary>
        /// ON DELETE NO ACTION
        /// </summary>
        NoAction = 0,

        /// <summary>
        /// ON DELETE RESTRICT
        /// </summary>
        Restrict = 1,

        /// <summary>
        /// ON DELETE CASCADE
        /// </summary>
        Cascade = 2,

        /// <summary>
        /// ON DELETE SET NULL
        /// </summary>
        SetNull = 3,

        /// <summary>
        /// ON DELETE SET DEFAULT
        /// </summary>
        SetDefault = 4
    }
}