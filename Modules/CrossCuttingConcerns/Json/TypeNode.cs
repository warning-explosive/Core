namespace SpaceEngineers.Core.CrossCuttingConcerns.Json
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security;
    using System.Text.RegularExpressions;
    using Basics;
    using CompositionRoot.Api.Abstractions;
    using Newtonsoft.Json;

    internal class TypeNode
    {
        private static readonly Regex WithGenericArgumentsPattern =
            new Regex("(?=\\[)(.)*(?<=\\])", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        internal TypeNode(Type type)
        {
            Assembly = type.Assembly.GetName().Name;
            Type = type.GenericTypeDefinitionOrSelf().FullName;
            GenericArguments = type.IsGenericType
                                   ? type.GetGenericArguments()
                                         .Select(genericArgument => new TypeNode(genericArgument))
                                         .ToList()
                                   : new List<TypeNode>();

            IsArray = type.IsArray;
        }

        private TypeNode(string assembly, string type)
        {
            Assembly = assembly;
            Type = type;
            GenericArguments = new List<TypeNode>();
            IsArray = type.EndsWith("[]", StringComparison.OrdinalIgnoreCase);
        }

        [JsonProperty]
        internal string Assembly { get; }

        [JsonProperty]
        internal string Type { get; }

        [JsonProperty]
        internal List<TypeNode> GenericArguments { get; }

        [JsonIgnore]
        private bool IsArray { get; }

        public override string ToString()
        {
            var args = GenericArguments.Any()
                           ? $" [{string.Join("|", GenericArguments.Select(arg => arg.ToString()))}]"
                           : string.Empty;

            return $"{Assembly} {Type}{args}";
        }

        internal static TypeNode Parse(string str)
        {
            var args = WithGenericArgumentsPattern
                      .Match(str)
                      .ToString()
                      .Trim('[', ']');

            var pair = str
                      .Substring(0, str.Length - args.Length)
                      .Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            var assembly = pair[0];
            var type = pair[1];

            var node = new TypeNode(assembly, type);
            node.GenericArguments.AddRange(args.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries).Select(Parse));

            return node;
        }

        internal Type BuildType(ITypeProvider typeProvider)
        {
            if (!TryGetType(typeProvider, out var type)
             || type == null)
            {
                throw new SecurityException($"Untrusted type {Type}");
            }

            return GenericArguments.Any()
                       ? type.MakeGenericType(GenericArguments.Select(child => child.BuildType(typeProvider)).ToArray())
                       : IsArray
                           ? typeof(Array)
                               .CallMethod(nameof(Array.Empty))
                               .WithTypeArgument(type)
                               .Invoke()
                               .GetType()
                           : type;
        }

        private bool TryGetType(ITypeProvider typeProvider, out Type? type)
        {
            if (typeProvider.TypeCache.TryGetValue(Assembly, out var types)
             && types.TryGetValue(IsArray ? Type.Trim('[', ']') : Type, out type))
            {
                return true;
            }

            type = null;
            return false;
        }
    }
}