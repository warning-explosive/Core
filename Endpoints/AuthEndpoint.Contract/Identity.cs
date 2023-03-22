namespace SpaceEngineers.Core.AuthEndpoint.Contract
{
    using System.Reflection;
    using Basics;

    /// <summary>
    /// Identity
    /// </summary>
    public static class Identity
    {
        /// <summary>
        /// AuthEndpoint logical name
        /// </summary>
        public const string LogicalName = nameof(AuthEndpoint);

        /// <summary>
        /// AuthEndpoint assembly
        /// </summary>
        public static readonly Assembly Assembly = AssembliesExtensions.FindRequiredAssembly(
            AssembliesExtensions.BuildName(
                nameof(SpaceEngineers),
                nameof(Core),
                nameof(AuthEndpoint)));
    }
}