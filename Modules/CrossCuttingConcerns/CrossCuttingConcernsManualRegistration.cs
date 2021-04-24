namespace SpaceEngineers.Core.CrossCuttingConcerns
{
    using Api.Abstractions;
    using AutoRegistration.Abstractions;

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
        }
    }
}