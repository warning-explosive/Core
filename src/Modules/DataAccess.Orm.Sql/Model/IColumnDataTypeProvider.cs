namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Model
{
    internal interface IColumnDataTypeProvider
    {
        string GetColumnDataType(ColumnInfo column);
    }
}