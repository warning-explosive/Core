namespace SpaceEngineers.Core.DataAccess.Orm.PostgreSql.Model
{
    using System;
    using System.Linq;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics;
    using Orm.Model;

    [Component(EnLifestyle.Singleton)]
    internal class ModelValidator : IModelValidator
    {
        private const int PostgresNameDataLength = 64 - 1 - 5;

        public void Validate(DatabaseNode model)
        {
            model
                .Schemas.Select(schemaNode => schemaNode.Schema)
                .Concat(model.Schemas.SelectMany(schemaNode => schemaNode.Indexes.Select(indexNode => indexNode.Index)))
                .Concat(model.Schemas.SelectMany(schemaNode => schemaNode.Views.Select(viewNode => viewNode.View)))
                .Concat(model.Schemas.SelectMany(schemaNode => schemaNode.Tables.Select(tableNode => tableNode.Table)))
                .Concat(model.Schemas.SelectMany(schemaNode => schemaNode.Tables.SelectMany(tableNode => tableNode.Columns.Select(columnNode => columnNode.Column))))
                .Where(objectName => objectName.Length > PostgresNameDataLength)
                .Each(objectName => throw new InvalidOperationException($"Postgres object name {objectName} should be less than {PostgresNameDataLength} bytes (actual {objectName.Length} bytes)"));
        }
    }
}