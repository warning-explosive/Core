namespace SpaceEngineers.Core.DataAccess.Orm.PostgreSql.Host.Migrations
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics;
    using Sql.Host.Model;
    using Sql.Translation;

    [Component(EnLifestyle.Singleton)]
    internal class CreateTriggerModelChangeCommandBuilder : IModelChangeCommandBuilder<CreateTrigger>,
                                                            IResolvable<IModelChangeCommandBuilder<CreateTrigger>>,
                                                            ICollectionResolvable<IModelChangeCommandBuilder>
    {
        private const string CommandFormat = @"create trigger ""{0}""
{4} {5} on ""{1}"".""{2}""
execute procedure ""{1}"".""{3}""()";

        public IEnumerable<ICommand> BuildCommands(IModelChange change)
        {
            return change is CreateTrigger createTrigger
                ? BuildCommands(createTrigger)
                : throw new NotSupportedException($"Unsupported model change type {change.GetType()}");
        }

        [SuppressMessage("Analysis", "CA1308", Justification = "sql script readability")]
        public IEnumerable<ICommand> BuildCommands(CreateTrigger change)
        {
            var type = change.Type.ToString().ToLowerInvariant();

            var events = new List<string>();

            if (change.Event.HasFlag(EnTriggerEvent.Insert))
            {
                events.Add(EnTriggerEvent.Insert.ToString().ToLowerInvariant());
            }

            if (change.Event.HasFlag(EnTriggerEvent.Update))
            {
                events.Add(EnTriggerEvent.Update.ToString().ToLowerInvariant());
            }

            if (change.Event.HasFlag(EnTriggerEvent.Delete))
            {
                events.Add(EnTriggerEvent.Delete.ToString().ToLowerInvariant());
            }

            var @event = events.ToString(" or ");

            yield return new SqlCommand(
                CommandFormat.Format(change.Trigger, change.Schema, change.Table, change.Function, type, @event),
                Array.Empty<SqlCommandParameter>());
        }
    }
}