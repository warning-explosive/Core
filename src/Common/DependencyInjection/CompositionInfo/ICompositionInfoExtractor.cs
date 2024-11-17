namespace SpaceEngineers.Core.DependencyInjection.CompositionInfo;

using System.Collections.Generic;

public interface ICompositionInfoExtractor
{
    IReadOnlyCollection<DependencyInfo> GetCompositionInfo();
}