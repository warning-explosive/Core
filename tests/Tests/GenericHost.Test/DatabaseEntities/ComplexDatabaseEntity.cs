namespace SpaceEngineers.Core.GenericHost.Test.DatabaseEntities
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using DataAccess.Orm.Sql.Model;
    using DataAccess.Orm.Sql.Model.Attributes;
    using GenericEndpoint.Contract.Abstractions;
    using Relations;

    [SuppressMessage("Analysis", "SA1011", Justification = "space between square brackets and nullable symbol")]
    [Schema(nameof(GenericHost) + nameof(Test))]
    internal record ComplexDatabaseEntity : BaseDatabaseEntity<Guid>
    {
        public ComplexDatabaseEntity(Guid primaryKey)
            : base(primaryKey)
        {
        }

        public double Number { get; set; }

        public double? NullableNumber { get; set; }

        public Guid Identifier { get; set; }

        public Guid? NullableIdentifier { get; set; }

        public bool Boolean { get; set; }

        public bool? NullableBoolean { get; set; }

        public DateTime DateTime { get; set; }

        public DateTime? NullableDateTime { get; set; }

        public TimeSpan TimeSpan { get; set; }

        public TimeSpan? NullableTimeSpan { get; set; }

        public DateOnly DateOnly { get; set; }

        public DateOnly? NullableDateOnly { get; set; }

        public TimeOnly TimeOnly { get; set; }

        public TimeOnly? NullableTimeOnly { get; set; }

        public byte[] ByteArray { get; set; } = default!;

        public string String { get; set; } = default!;

        public string? NullableString { get; set; }

        public EnEnum Enum { get; set; }

        public EnEnum? NullableEnum { get; set; }

        public EnEnumFlags EnumFlags { get; set; }

        public EnEnumFlags? NullableEnumFlags { get; set; }

        public EnEnum[] EnumArray { get; set; } = Array.Empty<EnEnum>();

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
                Number = 42,
                NullableNumber = 42,
                Identifier = Guid.NewGuid(),
                NullableIdentifier = Guid.NewGuid(),
                Boolean = true,
                NullableBoolean = true,
                DateTime = DateTime.Today,
                NullableDateTime = DateTime.Today,
                TimeSpan = TimeSpan.FromHours(3),
                NullableTimeSpan = TimeSpan.FromHours(3),
                DateOnly = DateOnly.FromDateTime(DateTime.Today),
                NullableDateOnly = DateOnly.FromDateTime(DateTime.Today),
                TimeOnly = TimeOnly.FromTimeSpan(TimeSpan.FromHours(3)),
                NullableTimeOnly = TimeOnly.FromTimeSpan(TimeSpan.FromHours(3)),
                ByteArray = new byte[] { 1, 2, 3 },
                String = "SomeString",
                NullableString = "SomeNullableString",
                Enum = EnEnum.Three,
                NullableEnum = EnEnum.Three,
                EnumFlags = EnEnumFlags.A | EnEnumFlags.B,
                NullableEnumFlags = EnEnumFlags.A | EnEnumFlags.B,
                EnumArray = new[] { EnEnum.One, EnEnum.Two, EnEnum.Three },
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
                Number = 42,
                NullableNumber = null,
                Identifier = Guid.NewGuid(),
                NullableIdentifier = null,
                Boolean = true,
                NullableBoolean = null,
                DateTime = DateTime.Today,
                NullableDateTime = null,
                TimeSpan = TimeSpan.FromHours(3),
                NullableTimeSpan = null,
                DateOnly = DateOnly.FromDateTime(DateTime.Today),
                NullableDateOnly = null,
                TimeOnly = TimeOnly.FromTimeSpan(TimeSpan.FromHours(3)),
                NullableTimeOnly = null,
                ByteArray = new byte[] { 1, 2, 3 },
                String = "SomeString",
                NullableString = null,
                Enum = EnEnum.Three,
                NullableEnum = null,
                EnumFlags = EnEnumFlags.A | EnEnumFlags.B,
                NullableEnumFlags = null,
                EnumArray = new[] { EnEnum.One, EnEnum.Two, EnEnum.Three },
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