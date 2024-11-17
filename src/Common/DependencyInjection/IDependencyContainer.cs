namespace SpaceEngineers.Core.DependencyInjection;

using System;
using System.Collections.Generic;

public interface IDependencyContainer
{
    TService Resolve<TService>()
        where TService : class;

    object Resolve(Type service);

    object ResolveGeneric(Type service, params Type[] genericTypeArguments);

    IEnumerable<TService> ResolveCollection<TService>()
        where TService : class;

    IEnumerable<object> ResolveCollection(Type service);

    IDisposable OpenScope();

    IAsyncDisposable OpenScopeAsync();
}