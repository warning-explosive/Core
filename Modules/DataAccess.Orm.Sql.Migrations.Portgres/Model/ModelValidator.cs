namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Migrations.Postgres.Model
{
    using System;
    using System.Linq;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics;
    using SpaceEngineers.Core.DataAccess.Orm.Sql.Migrations.Model;

    [Component(EnLifestyle.Singleton)]
    internal class ModelValidator : IModelValidator,
                                    IResolvable<IModelValidator>
    {
        private const int PostgreSqlNameDataLength = 64 - 1 - 5;

        public void Validate(DatabaseNode model)
        {
            model
                .Schemas.Select(schemaNode => schemaNode.Schema)
                .Concat(model.Schemas.SelectMany(schemaNode => schemaNode.Types.Select(typeNode => typeNode.Type)))
                .Concat(model.Schemas.SelectMany(schemaNode => schemaNode.Types.SelectMany(typeNode => typeNode.Values)))
                .Concat(model.Schemas.SelectMany(schemaNode => schemaNode.Indexes.Select(indexNode => indexNode.Index)))
                .Concat(model.Schemas.SelectMany(schemaNode => schemaNode.Views.Select(viewNode => viewNode.View)))
                .Concat(model.Schemas.SelectMany(schemaNode => schemaNode.Tables.Select(tableNode => tableNode.Table)))
                .Concat(model.Schemas.SelectMany(schemaNode => schemaNode.Tables.SelectMany(tableNode => tableNode.Columns.Select(columnNode => columnNode.Column))))
                .Where(objectName => objectName.Length > PostgreSqlNameDataLength)
                .Each(objectName => throw new InvalidOperationException($"PostgreSQL object name {objectName} should be less than {PostgreSqlNameDataLength} bytes (actual {objectName.Length} bytes)"));
        }
    }
}