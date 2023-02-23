namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Text;
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
        /// <param name="collect">Will command be collected by transaction or not</param>
        public SqlCommand(
            string commandText,
            IReadOnlyCollection<SqlCommandParameter> commandParameters,
            bool collect = true)
        {
            CommandText = commandText;
            CommandParameters = commandParameters;
            Collect = collect;
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
        /// Will command be collected by transaction or not
        /// </summary>
        public bool Collect { get; }

        /// <inheritdoc />
        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.AppendLine(CommandText);

            if (CommandParameters.Any())
            {
                sb.Append("--Parameters:");
                sb.Append("[");
                sb.Append(CommandParameters.ToString(", "));
                sb.Append("]");
            }

            return sb.ToString();
        }

        /// <summary>
        /// Merges two commands in a single one
        /// </summary>
        /// <param name="command">SqlCommand</param>
        /// <param name="separator">Separator</param>
        /// <returns>Merged SqlCommand</returns>
        public SqlCommand Merge(SqlCommand command, string separator)
        {
            var nextIndex = CommandParameters.Any()
                ? CommandParameters
                    .Select(param => int.Parse(param.Name.Substring(TranslationContext.CommandParameterFormat.Format(string.Empty).Length).Trim(), CultureInfo.InvariantCulture))
                    .Max() + 1
                : 0;

            var nextCommandText = command.CommandText;
            var nextCommandParameters = new List<SqlCommandParameter>(command.CommandParameters.Count);

            foreach (var param in command.CommandParameters)
            {
                var nextCommandParameterName = TranslationContext.CommandParameterFormat.Format(nextIndex.ToString(CultureInfo.InvariantCulture));

                var pattern = new Regex($"(@{param.Name})(?:\\b)", RegexOptions.IgnoreCase | RegexOptions.Compiled);

                var match = pattern.Match(nextCommandText);

                if (!match.Success)
                {
                    throw new InvalidOperationException("Unable to merge command parameters");
                }

                nextCommandText = nextCommandText.Substring(0, match.Groups[0].Index)
                    + $"@{nextCommandParameterName}"
                    + nextCommandText.Substring(Math.Min(nextCommandText.Length, match.Groups[0].Index + match.Groups[0].Length));

                nextCommandParameters.Add(new SqlCommandParameter(nextCommandParameterName, param.Value, param.Type, param.IsJsonValue));

                nextIndex++;
            }

            var commandText = string.Join(separator, CommandText, nextCommandText);

            var commandParameters = CommandParameters
                .Concat(nextCommandParameters)
                .ToArray();

            var collect = Collect || command.Collect;

            return new SqlCommand(commandText, commandParameters, collect);
        }
    }
}