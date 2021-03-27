namespace SpaceEngineers.Core.AutoRegistration.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Basics;
    using Internals;

    internal static class DecoratorRegistrationInfoExtensions
    {
        internal static IEnumerable<DecoratorRegistrationInfo> GetDecoratorInfo(
            this IEnumerable<Type> decorators,
            Type decoratorType)
        {
            return decorators
                .SelectMany(decorator => decorator
                    .ExtractGenericArgumentsAt(decoratorType)
                    .Select(service => new DecoratorRegistrationInfo(service, decorator))
                    .Where(info => info.ForAutoRegistration()));
        }

        internal static IEnumerable<DecoratorRegistrationInfo> GetConditionalDecoratorInfo(
            this IEnumerable<Type> decorators,
            Type decoratorType)
        {
            return decorators
                .SelectMany(decorator => decorator
                    .ExtractGenericArguments(decoratorType)
                    .Select(args => new DecoratorRegistrationInfo(args[0], decorator)
                    {
                        ConditionAttribute = args[1]
                    })
                    .Where(info => info.ForAutoRegistration()));
        }
    }
}