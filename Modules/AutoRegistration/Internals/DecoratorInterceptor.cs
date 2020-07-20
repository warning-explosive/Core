namespace SpaceEngineers.Core.AutoRegistration.Internals
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using AutoWiringApi.Abstractions;
    using Basics;
    using SimpleInjector;
    using SimpleInjector.Advanced;

    internal class DecoratorInterceptor
    {
        private readonly IReadOnlyDictionary<Type, Stack<ServiceRegistrationInfo>> _stacks;

        private readonly MethodInfo _typedExpressionInvocationMethod;
        private readonly MethodInfo _madeDependencyMethod;

        internal DecoratorInterceptor(IReadOnlyDictionary<Type, Stack<ServiceRegistrationInfo>> stacks)
        {
            _stacks = stacks;
            _typedExpressionInvocationMethod = FindMethod(nameof(TypedProducerInvocation));
            _madeDependencyMethod = FindMethod(nameof(MadeDependency));
        }

        internal object InterceptResolution(InitializationContext context, Func<object> instanceProducer)
        {
            var container = context.Registration.Container;
            var serviceType = context.Producer.ServiceType;

            var overrides = BuildOverrides(container, context.Producer, instanceProducer);

            if (!overrides.Any())
            {
                return instanceProducer();
            }

            // Resolve directly overriden service
            if (overrides.TryGetValue(serviceType, out var expression))
            {
                return Invoke(expression);
            }

            // Resolve service with override as dependency in object graph
            expression = GetExpression(_madeDependencyMethod,
                                       new[] { serviceType, instanceProducer().GetType() },
                                       container,
                                       null,
                                       overrides);

            return Invoke(expression);
        }

        private IReadOnlyDictionary<Type, Expression> BuildOverrides(Container container, InstanceProducer producer, Func<object> instanceProducer)
        {
            return _stacks.Where(pair => Overrides(producer, p => ProducerForService(p, pair.Key)))
                          .ToDictionary(pair => pair.Key,
                                        pair =>
                                        {
                                            var interceptedExpression = producer.ServiceType == pair.Key
                                                                            ? GetExpression(_typedExpressionInvocationMethod, new[] { producer.ServiceType }, instanceProducer)
                                                                            : null;

                                            var expression = interceptedExpression;

                                            if (!pair.Value.Any())
                                            {
                                                throw new InvalidOperationException("ApplyDecorator scope must have at least one override");
                                            }

                                            foreach (var info in pair.Value.Reverse())
                                            {
                                                expression = GetExpression(_madeDependencyMethod,
                                                                           new[] { info.ServiceType, info.ComponentType },
                                                                           container,
                                                                           expression,
                                                                           null);
                                            }

                                            return expression.EnsureNotNull("Expression must have value");
                                        });
        }

        private static bool Overrides(InstanceProducer producer, Func<InstanceProducer, bool> condition)
        {
            return condition(producer)
                || producer.GetRelationships().Any(z => Overrides(z.Dependency, condition));
        }

        private static bool ProducerForService(InstanceProducer producer, Type service) => producer.ServiceType == service;

        private static object Invoke(Expression expression) => Expression.Lambda<Func<object>>(expression).Compile().Invoke();

        private Expression MadeDependency<TService, TImplementation>(Container container,
                                                                     Expression? expression = null,
                                                                     IReadOnlyDictionary<Type, Expression>? overrides = null)
            where TService : class
            where TImplementation : TService
        {
            var serviceType = typeof(TService);
            var implementationType = typeof(TImplementation);

            var cctor = container.Options.ConstructorResolutionBehavior.GetConstructor(implementationType);

            var parameters = cctor.GetParameters()
                                  .Select(parameter =>
                                          {
                                              if (expression != null
                                               && parameter.ParameterType == serviceType
                                               && typeof(IDecorator<>).MakeGenericType(serviceType).IsAssignableFrom(implementationType))
                                              {
                                                  if (IsRegisteredDependency(container, serviceType, implementationType))
                                                  {
                                                      throw new InvalidOperationException();
                                                  }

                                                  return expression;
                                              }

                                              if (overrides != null
                                               && overrides.TryGetValue(parameter.ParameterType, out var @override))
                                              {
                                                  return @override;
                                              }

                                              var registration = container.GetRegistration(parameter.ParameterType)
                                                                          .EnsureNotNull($"Parameter {parameter} must be registered in container");

                                              var decorated = registration.GetRelationships()
                                                                          .All(r => IsDecorator(r, container, parameter.ParameterType));

                                              if (decorated)
                                              {
                                                  return registration.BuildExpression();
                                              }

                                              return GetExpression(_madeDependencyMethod,
                                                                   new[] { registration.ServiceType, registration.ImplementationType },
                                                                   container,
                                                                   null,
                                                                   overrides);
                                          })
                                  .ToList();

            return Expression.New(cctor, parameters);
        }

        private static bool IsRegisteredDependency(Container container, Type serviceType, Type implementation)
        {
            if (container.GetRegistration(implementation) != null)
            {
                return true;
            }

            var registration = container.GetRegistration(serviceType);

            return registration.GetRelationships()
                               .Select(r => r.ImplementationType)
                               .Any(type =>
                                    {
                                        if (implementation.IsGenericType
                                         && !implementation.IsConstructedGenericType)
                                        {
                                            var relationship = type.IsGenericType ? type.GetGenericTypeDefinition() : type;
                                            return relationship == implementation.GetGenericTypeDefinition();
                                        }

                                        return type == implementation;
                                    });
        }

        private static bool IsDecorator(KnownRelationship relationship, Container container, Type serviceType)
        {
            return typeof(IDecorator<>).MakeGenericType(serviceType).IsAssignableFrom(relationship.ImplementationType)
                || container.Options.ConstructorResolutionBehavior
                            .GetConstructor(relationship.ImplementationType)
                            .GetParameters()
                            .Any(parameter => parameter.ParameterType == serviceType);
        }

        private static Expression TypedProducerInvocation<TService>(Func<object> instanceProducer)
        {
            Expression<Func<TService>> wrapped = () => (TService)instanceProducer();

            return Expression.Invoke(wrapped);
        }

        private static MethodInfo FindMethod(string methodName)
        {
            return typeof(DecoratorInterceptor).GetMethod(methodName, BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic)
                                               .EnsureNotNull($"{methodName} method must exists");
        }

        private Expression GetExpression(MethodInfo methodInfo, Type[] genericArgs, params object?[] args)
        {
            return (Expression)methodInfo.MakeGenericMethod(genericArgs).Invoke(methodInfo.IsStatic ? null : this, args);
        }
    }
}