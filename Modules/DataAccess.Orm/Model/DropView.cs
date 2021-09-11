namespace SpaceEngineers.Core.DataAccess.Orm.Model
{
    /// <summary>
    /// Drop view change
    /// </summary>
    public class DropView : IDatabaseModelChange
    {
        /// <summary> .cctor </summary>
        /// <param name="viewName">View name</param>
        public DropView(string viewName)
        {
            ViewName = viewName;
        }

        /// <summary>
        /// View name
        /// </summary>
        public string ViewName { get; }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{nameof(DropView)} {ViewName}";
        }
    }
}