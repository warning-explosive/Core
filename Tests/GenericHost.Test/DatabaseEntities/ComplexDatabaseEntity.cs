namespace SpaceEngineers.Core.GenericHost.Test.DatabaseEntities
{
    using System;
    using DataAccess.Orm.Sql.Model;
    using DataAccess.Orm.Sql.Model.Attributes;
    using GenericEndpoint.Contract.Abstractions;
    using Relations;

    [Schema(nameof(GenericHost) + nameof(Test))]
    internal record ComplexDatabaseEntity : BaseDatabaseEntity<Guid>
    {
        public ComplexDatabaseEntity(Guid primaryKey)
            : base(primaryKey)
        {
        }

        public string String { get; set; } = default!;

        public string? NullableString { get; set; }

        public EnEnum Enum { get; set; }

        public EnEnum? NullableEnum { get; set; }

        public EnEnumFlags EnumFlags { get; set; }

        public EnEnumFlags? NullableEnumFlags { get; set; }

        public EnEnum[] EnumArray { get; set; } = Array.Empty<EnEnum>();

        public EnEnum?[] NullableEnumArray { get; set; } = Array.Empty<EnEnum?>();

        public string[] StringArray { get; set; } = Array.Empty<string>();

        public string?[] NullableStringArray { get; set; } = Array.Empty<string?>();

        public DateTime[] DateTimeArray { get; set; } = Array.Empty<DateTime>();

        public DateTime?[] NullableDateTimeArray { get; set; } = Array.Empty<DateTime?>();

        [JsonColumn]
        public IIntegrationMessage Json { get; set; } = default!;

        [JsonColumn]
        public IIntegrationMessage? NullableJson { get; set; }

        [ForeignKey(EnOnDeleteBehavior.NoAction)]
        public Blog Relation { get; set; } = default!;

        [ForeignKey(EnOnDeleteBehavior.NoAction)]
        public Blog? NullableRelation { get; set; }

        public static ComplexDatabaseEntity Generate(IIntegrationMessage json, Blog relation)
        {
            return new ComplexDatabaseEntity(Guid.NewGuid())
            {
                String = "SomeString",
                NullableString = "SomeNullableString",
                Enum = EnEnum.Three,
                NullableEnum = EnEnum.Three,
                EnumFlags = EnEnumFlags.A | EnEnumFlags.B | EnEnumFlags.C,
                NullableEnumFlags = EnEnumFlags.A | EnEnumFlags.B | EnEnumFlags.C,
                EnumArray = new[] { EnEnum.One, EnEnum.Two, EnEnum.Three },
                NullableEnumArray = new EnEnum?[] { EnEnum.One, EnEnum.Two, EnEnum.Three },
                StringArray = new[] { "SomeString", "AnotherString" },
                NullableStringArray = new[] { "SomeString", "AnotherString" },
                DateTimeArray = new[] { DateTime.MaxValue, DateTime.MaxValue },
                NullableDateTimeArray = new DateTime?[] { DateTime.MaxValue, DateTime.MaxValue },
                Json = json,
                NullableJson = json,
                Relation = relation,
                NullableRelation = relation
            };
        }

        public static ComplexDatabaseEntity GenerateWithNulls(IIntegrationMessage json, Blog relation)
        {
            return new ComplexDatabaseEntity(Guid.NewGuid())
            {
                String = "SomeString",
                NullableString = null,
                Enum = EnEnum.Three,
                NullableEnum = null,
                EnumFlags = EnEnumFlags.A | EnEnumFlags.B | EnEnumFlags.C,
                NullableEnumFlags = null,
                EnumArray = new[] { EnEnum.One, EnEnum.Two, EnEnum.Three },
                NullableEnumArray = new EnEnum?[] { EnEnum.One, null, EnEnum.Three },
                StringArray = new[] { "SomeString", "AnotherString" },
                NullableStringArray = new[] { "SomeString", null, "AnotherString" },
                DateTimeArray = new[] { DateTime.MaxValue, DateTime.MaxValue },
                NullableDateTimeArray = new DateTime?[] { DateTime.MaxValue, null, DateTime.MaxValue },
                Json = json,
                NullableJson = null,
                Relation = relation,
                NullableRelation = null
            };
        }
    }
}