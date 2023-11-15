namespace SpaceEngineers.Core.CompositionRoot.Registration
{
    internal class TypeProviderManualRegistration : IManualRegistration
    {
        private readonly ITypeProvider _typeProvider;
        private readonly IAutoRegistrationServicesProvider _servicesProvider;

        internal TypeProviderManualRegistration(
            ITypeProvider typeProvider,
            IAutoRegistrationServicesProvider servicesProvider)
        {
            _typeProvider = typeProvider;
            _servicesProvider = servicesProvider;
        }

        public void Register(IManualRegistrationsContainer container)
        {
            container
                .RegisterInstance<ITypeProvider>(_typeProvider)
                .RegisterInstance<IAutoRegistrationServicesProvider>(_servicesProvider);
        }
    }
}