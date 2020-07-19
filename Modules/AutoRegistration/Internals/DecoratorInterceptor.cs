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
            var lifestyle = context.Registration.Lifestyle;

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

            // Resolve service with override as dependency
            expression = GetExpression(_madeDependencyMethod,
                                       new[] { serviceType, instanceProducer().GetType() },
                                       container,
                                       lifestyle,
                                       instanceProducer,
                                       overrides);

            return Invoke(expression);
        }

        private IReadOnlyDictionary<Type, Expression> BuildOverrides(Container container, InstanceProducer producer, Func<object> instanceProducer)
        {
            return _stacks.Where(pair => Overrides(producer, p => ProducerForService(p, pair.Key)))
                          .ToDictionary(pair => pair.Key,
                                        pair =>
                                        {
                                            var info = pair.Value.Peek();
                                            var p = producer.ServiceType == info.ServiceType
                                                               ? instanceProducer
                                                               : null;

                                            return GetExpression(_madeDependencyMethod,
                                                                 new[] { info.ServiceType, info.ComponentType },
                                                                 container,
                                                                 info.Lifestyle,
                                                                 p,
                                                                 null);
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
                                                                     Lifestyle lifestyle,
                                                                     Func<object>? instanceProducer = null,
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
                                              if (instanceProducer != null
                                               && parameter.ParameterType == serviceType
                                               && typeof(IDecorator<>).MakeGenericType(serviceType).IsAssignableFrom(implementationType))
                                              {
                                                  return GetExpression(_typedExpressionInvocationMethod, new[] { serviceType }, instanceProducer);
                                              }

                                              if (overrides != null
                                               && overrides.TryGetValue(parameter.ParameterType, out var expression))
                                              {
                                                  return expression;
                                              }

                                              var registration = container.GetRegistration(parameter.ParameterType).EnsureNotNull($"Parameter {parameter} must be registered in container");

                                              if (registration.GetRelationships().Any())
                                              {
                                                  return GetExpression(_madeDependencyMethod,
                                                                       new[] { registration.ServiceType, registration.ImplementationType },
                                                                       container,
                                                                       registration.Lifestyle,
                                                                       instanceProducer,
                                                                       overrides);
                                              }

                                              return registration.BuildExpression();
                                          })
                                  .ToList();

            var cctorCall = Expression.Lambda<Func<TService>>(Expression.New(cctor, parameters)).Compile();

            var coldProducer = lifestyle.CreateProducer(cctorCall, container);

            var producerExpression = Expression.Lambda<Func<TService>>(coldProducer.BuildExpression());

            return Expression.Invoke(producerExpression);
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