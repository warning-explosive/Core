namespace SpaceEngineers.Core.AutoRegistration.Interception
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using AutoWiringApi.Abstractions;
    using Basics;
    using SimpleInjector;

    internal static class ResolveInterceptorExtensions
    {
        internal static bool IsDecorator(this Container container,
                                         Type serviceType,
                                         Type implementationType)
        {
            return typeof(IDecorator<>).MakeGenericType(serviceType)
                                       .IsAssignableFrom(implementationType)
                || container.Options.ConstructorResolutionBehavior
                            .GetConstructor(implementationType)
                            .GetParameters()
                            .Any(parameter => parameter.ParameterType == serviceType);
        }

        internal static bool IsRegisteredDependency(this Container container,
                                                    Type serviceType,
                                                    Type implementationType)
        {
            var producer = container.GetRegistration(serviceType)
                        ?? container.GetRegistration(implementationType);

            if (producer == null)
            {
                return false;
            }

            return producer.Flatten().Any(p => p.ImplementationType == implementationType);
        }

        internal static IEnumerable<InstanceProducer> Flatten(this InstanceProducer producer)
        {
            yield return producer;

            foreach (var innerProducer in producer.GetRelationships().SelectMany(r => r.Dependency.Flatten()))
            {
                yield return innerProducer;
            }
        }

        internal static bool ForService(this InstanceProducer producer, Type serviceType)
        {
            return producer.ServiceType == serviceType;
        }

        internal static object Invoke(this Expression expression)
        {
            return Expression.Lambda<Func<object>>(expression).Compile().Invoke();
        }

        internal static MethodInfo FindMethod(this Type type, string methodName)
        {
            return type.GetMethod(methodName, BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic)
                       .EnsureNotNull($"{methodName} method must exists");
        }

        internal static TReturn CallGenericMethod<TReturn>(this object target,
                                                           MethodInfo methodInfo,
                                                           Type[] genericArgs,
                                                           params object?[] args)
        {
            return (TReturn)methodInfo.MakeGenericMethod(genericArgs).Invoke(target, args);
        }

        internal static TReturn CallStaticGenericMethod<TReturn>(this MethodInfo methodInfo,
                                                                 Type[] genericArgs,
                                                                 params object?[] args)
        {
            return (TReturn)methodInfo.MakeGenericMethod(genericArgs).Invoke(null, args);
        }
    }
}