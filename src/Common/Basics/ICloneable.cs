namespace SpaceEngineers.Core.Basics;

using System;

public interface ICloneable<out T> : ICloneable
{
    new T Clone();
}