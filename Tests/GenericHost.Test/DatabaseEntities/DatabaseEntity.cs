namespace SpaceEngineers.Core.GenericHost.Test.DatabaseEntities
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using DataAccess.Api.Sql;
    using DataAccess.Api.Sql.Attributes;

    [SuppressMessage("Analysis", "SA1649", Justification = "StyleCop analyzer error")]
    [Schema(nameof(GenericHost) + nameof(Test))]
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

        public bool BooleanField { get; set; }

        public string StringField { get; set; }

        public string? NullableStringField { get; set; }

        public int IntField { get; set; }

        public static DatabaseEntity Generate()
        {
            return Generate(Guid.NewGuid());
        }

        public static DatabaseEntity Generate(Guid primaryKey)
        {
            return new DatabaseEntity(primaryKey, true, "SomeString", "SomeNullableString", 42);
        }
    }
}