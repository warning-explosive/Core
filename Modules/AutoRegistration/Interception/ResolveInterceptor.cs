namespace SpaceEngineers.Core.AutoRegistration.Interception
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using Basics;
    using Internals;
    using SimpleInjector;
    using SimpleInjector.Advanced;

    internal class ResolveInterceptor
    {
        private static readonly MethodInfo InstanceProducerInvocationMethod = typeof(ResolveInterceptor).FindMethod(nameof(InstanceProducerInvocation));
        private static readonly MethodInfo MadeDependencyMethod = typeof(ResolveInterceptor).FindMethod(nameof(MadeDependency));

        private readonly IReadOnlyDictionary<Type, Stack<ServiceRegistrationInfo>> _coldDecorators;

        internal ResolveInterceptor(IReadOnlyDictionary<Type, Stack<ServiceRegistrationInfo>> coldDecorators)
        {
            _coldDecorators = coldDecorators;
        }

        internal object InterceptResolution(InitializationContext context, Func<object> instanceProducer)
        {
            lock (_coldDecorators)
            {
                return InterceptResolutionInternal(context, instanceProducer);
            }
        }

        internal object InterceptResolutionInternal(InitializationContext context, Func<object> instanceProducer)
        {
            var container = context.Registration.Container;
            var serviceType = context.Producer.ServiceType;
            var implementationType = context.Registration.ImplementationType;

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
            expression = MadeDependencyUntyped(serviceType,
                                               implementationType,
                                               container,
                                               decorators,
                                               context.Producer,
                                               null,
                                               null);

            return expression.Invoke();
        }

        private IDictionary<Type, Expression?> BuildDecorators(Container container, InstanceProducer producer)
        {
            var decorators = _coldDecorators.Keys.ToDictionary(t => t, t => (Expression?)null);

            _coldDecorators.Where(pair => producer.Flatten().Any(p => p.ForService(pair.Key)))
                           .Each(pair => BuildDecorator(container, pair.Key, decorators, null));

            return decorators;
        }

        private Expression BuildDecorator(Container container, Type serviceType, IDictionary<Type, Expression?> decorators, DecoratorScope? outerScope)
        {
            var decoratorScope = outerScope == null
                                     ? new DecoratorScope(serviceType)
                                     : outerScope.Open(serviceType);

            var expression = _coldDecorators[serviceType]
                            .Reverse()
                            .Aggregate((Expression?)null, (expr, info) => MadeDependencyUntyped(info.ServiceType, info.ComponentType, container, decorators, null, expr, decoratorScope))
                            .EnsureNotNull("Expression must have value");

            decorators[serviceType] = expression;
            return expression;
        }

        private Expression MadeDependency<TService, TImplementation>(Container container,
                                                                     IDictionary<Type, Expression?> decorators,
                                                                     InstanceProducer? producer,
                                                                     Expression? expression,
                                                                     DecoratorScope? decoratorScope)
            where TService : class
            where TImplementation : TService
        {
            var parentServiceType = typeof(TService);
            var parentImplementationType = typeof(TImplementation);

            var cctor = container.Options.ConstructorResolutionBehavior.GetConstructor(parentImplementationType);

            var parameters = cctor.GetParameters()
                                  .Select((parameter, i) =>
                                          {
                                              if (decorators.TryGetValue(parameter.ParameterType, out var decorator))
                                              {
                                                  if (decorator != null)
                                                  {
                                                      return decorator;
                                                  }

                                                  if (!decoratorScope.AlreadyOpened(parameter.ParameterType))
                                                  {
                                                      return BuildDecorator(container, parameter.ParameterType, decorators, decoratorScope);
                                                  }
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
                                                  return MadeDependencyUntyped(parameterProducer.ServiceType,
                                                                               parameterProducer.Registration.ImplementationType,
                                                                               container,
                                                                               decorators,
                                                                               parameterProducer,
                                                                               null,
                                                                               decoratorScope);
                                              }

                                              return parameterProducer.BuildExpression();
                                          })
                                  .ToList();

            return Expression.New(cctor, parameters);
        }

        private Expression MadeDependencyUntyped(Type serviceType,
                                                 Type implementationType,
                                                 Container container,
                                                 IDictionary<Type, Expression?> decorators,
                                                 InstanceProducer? producer,
                                                 Expression? expression,
                                                 DecoratorScope? decoratorScope)
        {
            return this.CallGenericMethod<Expression>(MadeDependencyMethod,
                                                      new[] { serviceType, implementationType },
                                                      container,
                                                      decorators,
                                                      producer,
                                                      expression,
                                                      decoratorScope);
        }

        private static Expression InstanceProducerInvocation<TService>(Func<object> instanceProducer)
        {
            Expression<Func<TService>> wrapped = () => (TService)instanceProducer();

            return Expression.Invoke(wrapped);
        }

        private static Expression InstanceProducerInvocationUntyped(Type serviceType, Func<object> instanceProducer)
        {
            return InstanceProducerInvocationMethod.CallStaticGenericMethod<Expression>(new[] { serviceType }, instanceProducer);
        }
    }
}