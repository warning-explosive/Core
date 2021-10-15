namespace SpaceEngineers.Core.DataAccess.Orm.InMemoryDatabase.Model
{
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Orm.Model;

    [Component(EnLifestyle.Singleton)]
    internal class ModelValidator : IModelValidator
    {
        public void Validate(DatabaseNode model)
        {
        }
    }
}