namespace SpaceEngineers.Core.Basics;

public static class BooleanExtensions
{
    public static int Bit(this bool condition)
    {
        return condition ? 1 : 0;
    }
}