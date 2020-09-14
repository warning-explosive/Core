namespace SpaceEngineers.Core.Basics
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Exceptions;

    /// <summary>
    /// Execution info of method
    /// </summary>
    public class MethodExecutionInfo
    {
        private readonly Type _declaringType;

        private readonly string _methodName;

        private readonly ICollection<object?> _args = new List<object?>();

        private readonly ICollection<Type> _argumentTypes = new List<Type>();

        private readonly ICollection<Type> _typeArguments = new List<Type>();

        private object? _target;

        /// <summary> .ctor </summary>
        /// <param name="declaringType">Type that declare the method</param>
        /// <param name="methodName">Name of called method</param>
        public MethodExecutionInfo(Type declaringType, string methodName)
        {
            _declaringType = declaringType;
            _methodName = methodName;
        }

        /// <summary>
        /// Set target instance of method call
        /// </summary>
        /// <param name="target">Target instance of method call</param>
        /// <returns>MethodExecutionInfo</returns>
        public MethodExecutionInfo ForInstance(object target)
        {
            _target = target;

            return this;
        }

        /// <summary>
        /// Execute method with argument
        /// </summary>
        /// <param name="argument">Argument</param>
        /// <typeparam name="TArgument">Argument type</typeparam>
        /// <returns>MethodExecutionInfo</returns>
        public MethodExecutionInfo WithArgument<TArgument>(TArgument argument)
        {
            _args.Add(argument);
            _argumentTypes.Add(typeof(TArgument));

            return this;
        }

        /// <summary>
        /// Execute method with argument
        /// </summary>
        /// <param name="argument">Argument</param>
        /// <typeparam name="TArgument">Argument type</typeparam>
        /// <returns>MethodExecutionInfo</returns>
        public MethodExecutionInfo WithArgument<TArgument>(object? argument)
        {
            _args.Add(argument);
            _argumentTypes.Add(typeof(TArgument));

            return this;
        }

        /// <summary>
        /// Execute generic method with type argument
        /// </summary>
        /// <typeparam name="TTypeArgument">Type of Type-Argument</typeparam>
        /// <returns>MethodExecutionInfo</returns>
        public MethodExecutionInfo WithTypeArgument<TTypeArgument>()
        {
            _typeArguments.Add(typeof(TTypeArgument));

            return this;
        }

        /// <summary>
        /// Execute generic method with type argument
        /// </summary>
        /// <param name="typeArgument">Type of Type-Argument</param>
        /// <returns>MethodExecutionInfo</returns>
        public MethodExecutionInfo WithTypeArgument(Type typeArgument)
        {
            _typeArguments.Add(typeArgument);

            return this;
        }

        /// <summary>
        /// Invoke configured method
        /// </summary>
        /// <typeparam name="TResult">TResult type-argument</typeparam>
        /// <returns>Return value of method</returns>
        /// <exception cref="TypeMismatchException">Throws if TResult type is mismatched</exception>
        public TResult Invoke<TResult>()
        {
            return Invoke().EnsureType<TResult>();
        }

        /// <summary>
        /// Invoke configured method
        /// </summary>
        /// <returns>Return value of method</returns>
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
            var methodFinder = new MethodFinder(isInstanceMethod ? _target.GetType() : _declaringType,
                                                _methodName,
                                                GetBindings(isInstanceMethod))
                               {
                                   TypeArguments = _typeArguments.ToArray(),
                                   ArgumentTypes = _argumentTypes.ToArray()
                               };

            var methodInfo = methodFinder.FindMethod()
                                         .EnsureNotNull(() => new NotFoundException($"Method not found: {methodFinder}"));

            // 3 - call
            var isGenericMethod = _typeArguments.Any();

            var constructedMethod = isGenericMethod
                                        ? methodInfo.MakeGenericMethod(_typeArguments.ToArray())
                                        : methodInfo;

            Func<object> action = () => constructedMethod.Invoke(_target, _args.ToArray());

            return action.Try().Invoke();
        }

        private static BindingFlags GetBindings(bool isInstanceMethod)
        {
            return (isInstanceMethod ? BindingFlags.Instance : BindingFlags.Static)
                   | BindingFlags.Public
                   | BindingFlags.NonPublic
                   | BindingFlags.InvokeMethod;
        }
    }
}