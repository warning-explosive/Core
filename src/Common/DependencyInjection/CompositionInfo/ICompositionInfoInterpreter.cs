namespace SpaceEngineers.Core.DependencyInjection.CompositionInfo;

using System.Collections.Generic;

public interface ICompositionInfoInterpreter<out TOutput>
{
    TOutput Visualize(IReadOnlyCollection<DependencyInfo> compositionInfo);
}