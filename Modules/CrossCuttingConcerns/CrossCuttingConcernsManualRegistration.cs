namespace SpaceEngineers.Core.CrossCuttingConcerns
{
    using Api.Abstractions;
    using AutoRegistration.Abstractions;
    using Internals;

    /// <inheritdoc />
    public class CrossCuttingConcernsManualRegistration : IManualRegistration
    {
        /// <inheritdoc />
        public void Register(IManualRegistrationsContainer container)
        {
            container.Register<IStringFormatter, StringFormatter>();
            container.Register<StringFormatter, StringFormatter>();

            container.Register(typeof(IStringFormatter<>), typeof(DateTimeStringFormatter));
            container.Register(typeof(DateTimeStringFormatter), typeof(DateTimeStringFormatter));

            container.Register(typeof(IStringFormatter<>), typeof(ObjectStringFormatter<>));
            container.Register(typeof(ObjectStringFormatter<>), typeof(ObjectStringFormatter<>));

            container.Register(typeof(IObjectBuilder), typeof(ObjectBuilder));
            container.Register(typeof(ObjectBuilder), typeof(ObjectBuilder));

            container.Register(typeof(IObjectBuilder<>), typeof(GenericObjectBuilder<>));
            container.Register(typeof(GenericObjectBuilder<>), typeof(GenericObjectBuilder<>));

            container.Register(typeof(IObjectTransformer<,>), typeof(CharArrayToStringTransformer));
            container.Register(typeof(CharArrayToStringTransformer), typeof(CharArrayToStringTransformer));

            container.Register(typeof(IObjectTransformer<,>), typeof(StringToCharArrayTransformer));
            container.Register(typeof(StringToCharArrayTransformer), typeof(StringToCharArrayTransformer));
        }
    }
}