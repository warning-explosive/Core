namespace SpaceEngineers.Core.Modules.Test.InterceptedContainerTest
{
    internal interface IWithExtra
    {
        IExtraDependency Extra { get; }

        ImplementationExtra ImplExtra { get; }
    }
}