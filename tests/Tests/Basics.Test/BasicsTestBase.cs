namespace SpaceEngineers.Core.Basics.Test;

using Xunit.Abstractions;

public abstract class BasicsTestBase(ITestOutputHelper output)
{
    protected ITestOutputHelper Output { get; } = output;
}