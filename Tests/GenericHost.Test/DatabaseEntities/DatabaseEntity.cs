namespace SpaceEngineers.Core.GenericHost.Test.DatabaseEntities
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using SpaceEngineers.Core.DataAccess.Api.Model;

    [SuppressMessage("Analysis", "SA1649", Justification = "StyleCop analyzer error")]
    [Schema(nameof(GenericHost) + nameof(GenericHost.Test))]
    internal record DatabaseEntity : BaseDatabaseEntity<Guid>
    {
        public DatabaseEntity(
            Guid primaryKey,
            bool booleanField,
            string stringField,
            string? nullableStringField,
            int intField)
            : base(primaryKey)
        {
            BooleanField = booleanField;
            StringField = stringField;
            NullableStringField = nullableStringField;
            IntField = intField;
        }

        public bool BooleanField { get; init; }

        public string StringField { get; init; }

        public string? NullableStringField { get; init; }

        public int IntField { get; init; }
    }
}