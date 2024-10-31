namespace SpaceEngineers.Core.Basics.Test;

using Xunit.Sdk;

public static class TestExtensions
{
    public static FalseException FalseException()
    {
        return Xunit.Sdk.FalseException.ForNonFalseValue(nameof(FalseException), null);
    }

    public static TrueException TrueException()
    {
        return Xunit.Sdk.TrueException.ForNonTrueValue(nameof(TrueException), null);
    }
}