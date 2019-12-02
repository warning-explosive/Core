namespace SpaceEngineers.Core.CompositionRoot.Test
{
    using System;
    using System.Reflection;
    using Basics.Test;
    using Xunit.Abstractions;

    public class CompositionRootTestBase : BasicsTestBase
    {
        protected readonly DependencyContainer DependencyContainer;

        protected CompositionRootTestBase(ITestOutputHelper output) : base(output)
        {
            DependencyContainer = new DependencyContainer(AppDomain.CurrentDomain?.GetAssemblies() ?? Array.Empty<Assembly>());
        }
    }
}