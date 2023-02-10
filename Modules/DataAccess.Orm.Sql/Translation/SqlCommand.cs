namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Text.RegularExpressions;
    using Basics;
    using Orm.Linq;

    /// <summary>
    /// SqlCommand
    /// </summary>
    public sealed class SqlCommand : ICommand
    {
        /// <summary> .cctor </summary>
        /// <param name="commandText">Command text</param>
        /// <param name="commandParameters">Command parameters</param>
        // TODO: check creations
        public SqlCommand(string commandText, IReadOnlyCollection<SqlCommandParameter> commandParameters)
        {
            CommandText = commandText;
            CommandParameters = commandParameters;
        }

        /// <summary>
        /// Command text
        /// </summary>
        public string CommandText { get; }

        /// <summary>
        /// Command parameters
        /// </summary>
        public IReadOnlyCollection<SqlCommandParameter> CommandParameters { get; }

        /// <summary>
        /// Merges two commands in a single one
        /// </summary>
        /// <param name="command">SqlCommand</param>
        /// <param name="separator">Separator</param>
        /// <returns>Merged SqlCommand</returns>
        // TODO: optimize as much as possible
        public SqlCommand Merge(SqlCommand command, string separator)
        {
            var nextIndex = CommandParameters.Any()
                ? CommandParameters
                    .Select(param => int.Parse(param.Name.Substring(TranslationContext.QueryParameterFormat.Format(string.Empty).Length).Trim(), CultureInfo.InvariantCulture))
                    .Max() + 1
                : 0;

            var nextCommandText = command.CommandText;
            var nextCommandParameters = new List<SqlCommandParameter>(command.CommandParameters.Count);

            foreach (var param in command.CommandParameters)
            {
                var nextCommandParameterName = TranslationContext.QueryParameterFormat.Format(nextIndex.ToString(CultureInfo.InvariantCulture));

                var pattern = new Regex($"(@{param.Name})(?:\\b)", RegexOptions.IgnoreCase | RegexOptions.Compiled);

                var match = pattern.Match(nextCommandText);

                if (!match.Success)
                {
                    throw new InvalidOperationException("Unable to merge command parameters");
                }

                nextCommandText = nextCommandText.Substring(0, match.Groups[0].Index)
                    + $"@{nextCommandParameterName}"
                    + nextCommandText.Substring(Math.Min(nextCommandText.Length, match.Groups[0].Index + match.Groups[0].Length));

                nextCommandParameters.Add(new SqlCommandParameter(nextCommandParameterName, param.Value, param.Type));

                nextIndex++;
            }

            var commandText = string.Join(separator, CommandText, nextCommandText);

            var commandParameters = CommandParameters
                .Concat(nextCommandParameters)
                .ToArray();

            return new SqlCommand(commandText, commandParameters);
        }
    }
}