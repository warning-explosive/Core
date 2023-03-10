namespace SpaceEngineers.Core.GenericHost.Test.DatabaseEntities
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using DataAccess.Orm.Sql.Model;
    using DataAccess.Orm.Sql.Model.Attributes;

    [SuppressMessage("Analysis", "SA1649", Justification = "StyleCop analyzer error")]
    [Schema(nameof(GenericHost) + nameof(Test))]
    [Index(nameof(StringField), IncludedColumns = new[] { nameof(IntField) })]
    internal record DatabaseEntity : BaseDatabaseEntity<Guid>
    {
        public DatabaseEntity(
            Guid primaryKey,
            bool booleanField,
            string stringField,
            string? nullableStringField,
            int intField,
            EnEnum @enum)
            : base(primaryKey)
        {
            BooleanField = booleanField;
            StringField = stringField;
            NullableStringField = nullableStringField;
            IntField = intField;
            Enum = @enum;
        }

        public bool BooleanField { get; set; }

        public string StringField { get; set; }

        public string? NullableStringField { get; set; }

        public int IntField { get; set; }

        public EnEnum Enum { get; set; }

        public static DatabaseEntity Generate()
        {
            return Generate(Guid.NewGuid());
        }

        public static DatabaseEntity Generate(Guid primaryKey)
        {
            return new DatabaseEntity(primaryKey, true, "SomeString", "SomeNullableString", 42, EnEnum.Three);
        }
    }
}