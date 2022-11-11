namespace SpaceEngineers.Core.DataAccess.Orm.PostgreSql.Host.Model
{
    using System;
    using System.Linq;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics;
    using Sql.Host.Model;

    [Component(EnLifestyle.Singleton)]
    internal class ModelValidator : IModelValidator,
                                    IResolvable<IModelValidator>
    {
        private const int PostgreSqlNameDataLength = 64 - 1 - 5;

        public void Validate(DatabaseNode model)
        {
            model
                .Schemas.Select(schemaNode => schemaNode.Schema)
                .Concat(model.Schemas.SelectMany(schemaNode => schemaNode.Indexes.Select(indexNode => indexNode.Index)))
                .Concat(model.Schemas.SelectMany(schemaNode => schemaNode.Views.Select(viewNode => viewNode.View)))
                .Concat(model.Schemas.SelectMany(schemaNode => schemaNode.Tables.Select(tableNode => tableNode.Table)))
                .Concat(model.Schemas.SelectMany(schemaNode => schemaNode.Tables.SelectMany(tableNode => tableNode.Columns.Select(columnNode => columnNode.Column))))
                .Where(objectName => objectName.Length > PostgreSqlNameDataLength)
                .Each(objectName => throw new InvalidOperationException($"PostgreSQL object name {objectName} should be less than {PostgreSqlNameDataLength} bytes (actual {objectName.Length} bytes)"));
        }
    }
}