namespace SpaceEngineers.Core.Basics
{
    using System;

    /// <summary>
    /// Execution extension methods
    /// </summary>
    public static class ExecutionExtensions
    {
        /// <summary>
        /// Try execute client action
        /// </summary>
        /// <param name="clientAction">Client action</param>
        /// <returns>ActionExecutionInfo</returns>
        public static ActionExecutionInfo Try(this Action clientAction)
        {
            return new ActionExecutionInfo(clientAction);
        }

        /// <summary>
        /// Try execute client function
        /// </summary>
        /// <param name="clientFunction">Client function</param>
        /// <typeparam name="TResult">TResult type-argument</typeparam>
        /// <returns>ActionExecutionInfo</returns>
        public static ActionExecutionInfo Try<TResult>(this Func<TResult> clientFunction)
        {
            // return new FunctionExecutionInfo<TResult>(clientFunction);
            throw new NotImplementedException();
        }
    }
}