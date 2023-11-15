namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Migrations.Model
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// IDatabaseFunctionProvider
    /// </summary>
    /// <typeparam name="T">T type-argument</typeparam>
    public interface IDatabaseFunctionProvider<T>
        where T : Attribute
    {
        /// <summary>
        /// Gets function definition
        /// </summary>
        /// <param name="context">Context</param>
        /// <returns>Function definition</returns>
        string GetDefinition(IReadOnlyDictionary<string, string> context);
    }
}