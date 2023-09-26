namespace SpaceEngineers.Core.Test.Api.Logging
{
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.CompilerServices;

    internal static class XUnitModuleInitializer
    {
        [ModuleInitializer]
        [SuppressMessage("Analysis", "CA2255", Justification = "Redirects outputs to xUnit ITestOutputHelper")]
        public static void Initialize() => TestBase.Redirect();
    }
}