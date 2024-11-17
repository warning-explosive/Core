namespace SpaceEngineers.Core.DependencyInjection.CompositionInfo;

using System.Collections.Generic;
using System.Linq;
using SimpleInjector;

[Component(EnLifestyle.Singleton)]
public class CompositionInfoExtractor : ICompositionInfoExtractor
{
    private readonly Container _container;

    public CompositionInfoExtractor(Container container)
    {
        _container = container;
    }

    public IReadOnlyCollection<DependencyInfo> GetCompositionInfo()
    {
        return _container
            .GetCurrentRegistrations()
            .Select(DependencyInfo.RetrieveDependencyGraph)
            .ToArray();
    }

    /*private Type CloseOpenGeneric(Type type)
    {
        var closedOrSame = _provider.CloseByConstraints(type, ctx => ctx.Matches.OrderBy(t => t.IsGenericType).FirstOrDefault());

        // build graph by invocation
        Func<object?> getInstance = () => _container.GetInstance(closedOrSame);

        ExecutionExtensions
            .Try(getInstance)
            .Catch<ActivationException>()
            .Invoke(_ => default);

        return closedOrSame;
    }

    private Type CloseOpenGenericCollection(Type type)
    {
        var closedOrSame = _provider.CloseByConstraints(type, ctx => ctx.Matches.OrderBy(t => t.IsGenericType).FirstOrDefault());

        // build graph by invocation
        Func<IEnumerable<object>?> getAllInstances = () => _container.GetAllInstances(closedOrSame);

        ExecutionExtensions
            .Try(getAllInstances)
            .Catch<ActivationException>()
            .Invoke(_ => default);

        return typeof(IEnumerable<>).MakeGenericType(closedOrSame);
    }*/
}