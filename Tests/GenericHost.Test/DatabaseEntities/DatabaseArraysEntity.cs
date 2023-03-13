namespace SpaceEngineers.Core.GenericHost.Test.DatabaseEntities
{
    using System;
    using DataAccess.Orm.Sql.Model;
    using DataAccess.Orm.Sql.Model.Attributes;

    [Schema(nameof(GenericHost) + nameof(Test))]
    internal record DatabaseArraysEntity : BaseDatabaseEntity<Guid>
    {
        public DatabaseArraysEntity(Guid primaryKey)
            : base(primaryKey)
        {
        }

        public EnEnum Enum { get; set; }

        public EnEnumFlags EnumFlags { get; set; }

        public EnEnum[] EnumArray { get; set; } = Array.Empty<EnEnum>();

        public string?[] StringArray { get; set; } = Array.Empty<string?>();

        public DateTime?[] NullableDateTimeArray { get; set; } = Array.Empty<DateTime?>();

        public static DatabaseArraysEntity Generate()
        {
            return new DatabaseArraysEntity(Guid.NewGuid())
            {
                Enum = EnEnum.Three,
                EnumFlags = EnEnumFlags.A | EnEnumFlags.B | EnEnumFlags.C,
                EnumArray = new[] { EnEnum.One, EnEnum.Two, EnEnum.Three },
                StringArray = new[] { "SomeString", null, "AnotherString" },
                NullableDateTimeArray = new DateTime?[] { DateTime.MaxValue, null, DateTime.MaxValue }
            };
        }
    }
}