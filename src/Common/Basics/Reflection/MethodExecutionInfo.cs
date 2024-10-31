namespace SpaceEngineers.Core.Basics.Reflection;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Exceptions;

public class MethodExecutionInfo
{
    private readonly Type _declaringType;

    private readonly string _methodName;

    private readonly ICollection<object?> _args = new List<object?>();

    private readonly ICollection<Type> _argumentTypes = new List<Type>();

    private readonly ICollection<Type> _typeArguments = new List<Type>();

    private object? _target;

    public MethodExecutionInfo(Type declaringType, string methodName)
    {
        _declaringType = declaringType;
        _methodName = methodName;
    }

    public MethodExecutionInfo ForInstance(object target)
    {
        _target = target;

        return this;
    }

    public MethodExecutionInfo WithArgument<TArgument>(TArgument argument)
    {
        _args.Add(argument);
        _argumentTypes.Add(argument?.GetType() ?? typeof(object));

        return this;
    }

    public MethodExecutionInfo WithArgument(Type argumentType, object? argument)
    {
        _args.Add(argument);
        _argumentTypes.Add(argumentType);

        return this;
    }

    public MethodExecutionInfo WithArgument<TArgument>(object? argument)
    {
        _args.Add(argument);
        _argumentTypes.Add(argument?.GetType() ?? typeof(TArgument));

        return this;
    }

    public MethodExecutionInfo WithArguments(params object[] arguments)
    {
        foreach (var argument in arguments)
        {
            _args.Add(argument);
            _argumentTypes.Add(argument?.GetType() ?? typeof(object));
        }

        return this;
    }

    public MethodExecutionInfo WithTypeArgument<TTypeArgument>()
    {
        _typeArguments.Add(typeof(TTypeArgument));

        return this;
    }

    public MethodExecutionInfo WithTypeArgument(Type typeArgument)
    {
        _typeArguments.Add(typeArgument);

        return this;
    }

    public MethodExecutionInfo WithTypeArguments(params Type[] typeArguments)
    {
        typeArguments.Each(_typeArguments.Add);

        return this;
    }

    public TResult Invoke<TResult>()
    {
        return Invoke().EnsureType<TResult>();
    }

    public object? Invoke()
    {
        // 1 - prepare and check
        var isInstanceMethod = _target != null;

        if (isInstanceMethod
            && _target.GetType() != _declaringType)
        {
            throw new TypeMismatchException(_declaringType, _target.GetType());
        }

        // 2 - find
        var methodFinder = new MethodFinder(
            isInstanceMethod ? _target.GetType() : _declaringType,
            _methodName,
            GetBindings(isInstanceMethod))
        {
            TypeArguments = _typeArguments.ToArray(),
            ArgumentTypes = _argumentTypes.ToArray()
        };

        var methodInfo = methodFinder.FindMethod()
                         ?? throw new NotFoundException($"Method wasn't found: {methodFinder}");

        // 3 - call
        var isGenericMethod = _typeArguments.Any();

        var constructedMethod = isGenericMethod
            ? methodInfo.MakeGenericMethod(_typeArguments.ToArray())
            : methodInfo;

        return ExecutionExtensions
            .Try(InvokeMethod, (constructedMethod, _target, _args.ToArray()))
            .Catch<Exception>()
            .Invoke(ex => throw ex.Rethrow());

        static object? InvokeMethod((MethodInfo, object?, object?[]) state)
        {
            var (methodInfo, target, args) = state;
            return methodInfo.Invoke(target, args);
        }
    }

    private static BindingFlags GetBindings(bool isInstanceMethod)
    {
        return (isInstanceMethod ? BindingFlags.Instance : BindingFlags.Static)
               | BindingFlags.Public
               | BindingFlags.NonPublic
               | BindingFlags.InvokeMethod;
    }
}