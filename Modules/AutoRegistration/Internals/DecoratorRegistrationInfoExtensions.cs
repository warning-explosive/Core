namespace SpaceEngineers.Core.AutoRegistration.Internals
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using AutoWiringApi.Attributes;
    using Basics;

    internal static class DecoratorRegistrationInfoExtensions
    {
        internal static IEnumerable<DecoratorRegistrationInfo> GetDecoratorInfo(this IEnumerable<Type> decorators, Type decoratorType)
        {
            return decorators
                  .Where(RegistrationExtensions.ForAutoRegistration)
                  .SelectMany(type => type.ExtractGenericArgumentsAt(decoratorType, 0)
                                          .Select(service => new DecoratorRegistrationInfo(service, type, type.Lifestyle())));
        }

        internal static IEnumerable<DecoratorRegistrationInfo> GetConditionalDecoratorInfo(this IEnumerable<Type> decorators, Type decoratorType)
        {
            return decorators
                  .Where(RegistrationExtensions.ForAutoRegistration)
                  .SelectMany(type => type.ExtractGenericArguments(decoratorType)
                                          .Select(args => new DecoratorRegistrationInfo(args[0], type, type.Lifestyle())
                                                          {
                                                              Attribute = args[1]
                                                          }));
        }
    }
}