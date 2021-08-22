namespace SpaceEngineers.Core.CompositionRoot.ManualRegistrations
{
    using Api.Abstractions;
    using Implementations;

    internal class TypeProviderManualRegistration : IManualRegistration
    {
        private readonly TypeProvider _typeProvider;

        internal TypeProviderManualRegistration(TypeProvider typeProvider)
        {
            _typeProvider = typeProvider;
        }

        public void Register(IManualRegistrationsContainer container)
        {
            container
                .RegisterInstance<ITypeProvider>(_typeProvider)
                .RegisterInstance<TypeProvider>(_typeProvider);
        }
    }
}