namespace SpaceEngineers.Core.DataAccess.Orm.PostgreSql.Host
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using Api.Exceptions;
    using Basics;
    using Extensions;
    using Orm.Connection;

    /// <summary>
    /// PostgreSqlDatabaseProvider
    /// </summary>
    public class PostgreSqlDatabaseProvider : IDatabaseProvider
    {
        /// <inheritdoc />
        public IEnumerable<Assembly> Implementation()
        {
            return new[]
            {
                AssembliesExtensions.FindRequiredAssembly(AssembliesExtensions.BuildName(nameof(SpaceEngineers), nameof(Core), nameof(Core.DataAccess), nameof(Core.DataAccess.Api))),
                AssembliesExtensions.FindRequiredAssembly(AssembliesExtensions.BuildName(nameof(SpaceEngineers), nameof(Core), nameof(Core.DataAccess), nameof(Core.DataAccess.Orm))),
                AssembliesExtensions.FindRequiredAssembly(AssembliesExtensions.BuildName(nameof(SpaceEngineers), nameof(Core), nameof(Core.DataAccess), nameof(Core.DataAccess.Orm), nameof(Core.DataAccess.Orm.Sql))),
                AssembliesExtensions.FindRequiredAssembly(AssembliesExtensions.BuildName(nameof(SpaceEngineers), nameof(Core), nameof(Core.DataAccess), nameof(Core.DataAccess.Orm), nameof(Core.DataAccess.Orm.PostgreSql))),
                AssembliesExtensions.FindRequiredAssembly(AssembliesExtensions.BuildName(nameof(SpaceEngineers), nameof(Core), nameof(Core.Dynamic)))
            };
        }

        /// <inheritdoc />
        public IEnumerable<Assembly> Migrations()
        {
            return new[]
            {
                AssembliesExtensions.FindRequiredAssembly(AssembliesExtensions.BuildName(nameof(SpaceEngineers), nameof(Core), nameof(Core.DataAccess), nameof(Core.DataAccess.Api))),
                AssembliesExtensions.FindRequiredAssembly(AssembliesExtensions.BuildName(nameof(SpaceEngineers), nameof(Core), nameof(Core.DataAccess), nameof(Core.DataAccess.Orm))),
                AssembliesExtensions.FindRequiredAssembly(AssembliesExtensions.BuildName(nameof(SpaceEngineers), nameof(Core), nameof(Core.DataAccess), nameof(Core.DataAccess.Orm), nameof(Core.DataAccess.Orm.Sql))),
                AssembliesExtensions.FindRequiredAssembly(AssembliesExtensions.BuildName(nameof(SpaceEngineers), nameof(Core), nameof(Core.DataAccess), nameof(Core.DataAccess.Orm), nameof(Core.DataAccess.Orm.PostgreSql))),
                AssembliesExtensions.FindRequiredAssembly(AssembliesExtensions.BuildName(nameof(SpaceEngineers), nameof(Core), nameof(Core.Dynamic))),

                AssembliesExtensions.FindRequiredAssembly(AssembliesExtensions.BuildName(nameof(SpaceEngineers), nameof(Core), nameof(Core.DataAccess), nameof(Core.DataAccess.Orm), nameof(Core.DataAccess.Orm.Host))),
                AssembliesExtensions.FindRequiredAssembly(AssembliesExtensions.BuildName(nameof(SpaceEngineers), nameof(Core), nameof(Core.DataAccess), nameof(Core.DataAccess.Orm), nameof(Core.DataAccess.Orm.Sql), nameof(Core.DataAccess.Orm.Sql.Host))),
                AssembliesExtensions.FindRequiredAssembly(AssembliesExtensions.BuildName(nameof(SpaceEngineers), nameof(Core), nameof(Core.DataAccess), nameof(Core.DataAccess.Orm), nameof(Core.DataAccess.Orm.PostgreSql), nameof(Core.DataAccess.Orm.PostgreSql.Host)))
            };
        }

        /// <inheritdoc />
        public void Handle(string commandText, Exception exception)
        {
            var databaseException = exception.IsSerializationFailure()
                ? new DatabaseConcurrentUpdateException(commandText, exception)
                : new DatabaseException(commandText, exception);

            throw databaseException;
        }
    }
}