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

    internal class ResolveInterceptor
    {
        private static readonly MethodInfo InstanceProducerInvocationMethod = typeof(ResolveInterceptor).FindMethod(nameof(InstanceProducerInvocation));
        private static readonly MethodInfo MadeDependencyMethod = typeof(ResolveInterceptor).FindMethod(nameof(MadeDependency));

        private readonly IReadOnlyDictionary<Type, Stack<ServiceRegistrationInfo>> _stacks;

        internal ResolveInterceptor(IReadOnlyDictionary<Type, Stack<ServiceRegistrationInfo>> stacks)
        {
            _stacks = stacks;
        }

        internal object InterceptResolution(InitializationContext context, Func<object> instanceProducer)
        {
            lock (_stacks)
            {
                return InterceptResolutionInternal(context, instanceProducer);
            }
        }

        internal object InterceptResolutionInternal(InitializationContext context, Func<object> instanceProducer)
        {
            var container = context.Registration.Container;
            var serviceType = context.Producer.ServiceType;

            var decorators = BuildDecorators(container, context.Producer);

            if (!decorators.Any())
            {
                return instanceProducer();
            }

            // Resolve directly overriden service
            if (decorators.TryGetValue(serviceType, out var expression)
             && expression != null)
            {
                return expression.Invoke();
            }

            // Resolve service with override as dependency in object graph
            expression = MadeDependencyUntyped(serviceType, context.Producer.BuildExpression().Type, container, decorators, context.Producer);

            return expression.Invoke();
        }

        private IReadOnlyDictionary<Type, Expression?> BuildDecorators(Container container, InstanceProducer producer)
        {
            var decorators = _stacks.Keys.ToDictionary(t => t, t => (Expression?)null);

            return _stacks.Where(pair => producer.Flatten().Any(p => p.ForService(pair.Key)))
                          .OrderBy(z => 1) // TODO: order by dependencies
                          .ToDictionary(pair => pair.Key,
                                        pair =>
                                        {
                                            var expression = (Expression?)pair.Value
                                                                              .Reverse()
                                                                              .Aggregate((Expression?)null, (current, info) => MadeDependencyUntyped(info.ServiceType, info.ComponentType, container, decorators, null, current))
                                                                              .EnsureNotNull("Expression must have value");

                                            decorators[pair.Key] = expression;

                                            return expression;
                                        });
        }

        private static Expression MadeDependency<TService, TImplementation>(Container container,
                                                                            IReadOnlyDictionary<Type, Expression?> decorators,
                                                                            InstanceProducer? producer = null,
                                                                            Expression? expression = null)
            where TService : class
            where TImplementation : TService
        {
            var parentServiceType = typeof(TService);
            var parentImplementationType = typeof(TImplementation);

            var cctor = container.Options.ConstructorResolutionBehavior.GetConstructor(parentImplementationType);

            var parameters = cctor.GetParameters()
                                  .Select((parameter, i) =>
                                          {
                                              if (decorators.TryGetValue(parameter.ParameterType, out var decorator)
                                               && decorator != null)
                                              {
                                                  return decorator;
                                              }

                                              if (expression != null
                                               && parameter.ParameterType == parentServiceType
                                               && container.IsDecorator(parentServiceType, parentImplementationType))
                                              {
                                                  if (container.IsRegisteredDependency(parentServiceType, parentImplementationType))
                                                  {
                                                      throw new InvalidOperationException($"Decorator {parentImplementationType} already registered in container");
                                                  }

                                                  return expression;
                                              }

                                              var parameterProducer = producer?.GetRelationships()[i].Dependency
                                                                   ?? container.GetRegistration(parameter.ParameterType)
                                                                               .EnsureNotNull($"Service {parameter.ParameterType} must be registered in container");

                                              if (decorators.Keys.Any(service => parameterProducer.Flatten().Any(p => p.ForService(service))))
                                              {
                                                  /*
                                                   * InstanceProducer.ImplementationType has wrong type, without applied decorators
                                                   * InstanceProducer.Registration.ImplementationType has valid type
                                                   */
                                                  return MadeDependencyUntyped(parameterProducer.ServiceType, parameterProducer.Registration.ImplementationType, container, decorators, parameterProducer);
                                              }

                                              return parameterProducer.BuildExpression();
                                          })
                                  .ToList();

            return Expression.New(cctor, parameters);
        }

        private static Expression MadeDependencyUntyped(Type serviceType,
                                                        Type implementationType,
                                                        Container container,
                                                        IReadOnlyDictionary<Type, Expression?> decorators,
                                                        InstanceProducer? producer = null,
                                                        Expression? expression = null)
        {
            return MadeDependencyMethod.CallStaticGenericMethod(new[] { serviceType, implementationType },
                                                                container,
                                                                decorators,
                                                                producer,
                                                                expression);
        }

        private static Expression InstanceProducerInvocation<TService>(Func<object> instanceProducer)
        {
            Expression<Func<TService>> wrapped = () => (TService)instanceProducer();

            return Expression.Invoke(wrapped);
        }

        private static Expression InstanceProducerInvocationUntyped(Type serviceType, Func<object> instanceProducer)
        {
            return InstanceProducerInvocationMethod.CallStaticGenericMethod(new[] { serviceType }, instanceProducer);
        }
    }
}