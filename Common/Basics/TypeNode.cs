namespace SpaceEngineers.Core.Basics
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security;
    using System.Text;

    /// <summary>
    /// TypeNode
    /// </summary>
    public class TypeNode
    {
        private TypeNode(Type type)
        {
            Assembly = type.Assembly.GetName().Name;

            Type = type.GenericTypeDefinitionOrSelf().FullName;

            GenericArguments = type.IsGenericType
                ? type.GetGenericArguments()
                   .Where(genericArgument => !genericArgument.IsGenericParameter)
                   .Select(genericArgument => new TypeNode(genericArgument))
                   .ToList()
                : new List<TypeNode>();

            IsArray = type.IsArray;
        }

        private TypeNode(string assembly, string type, IReadOnlyCollection<TypeNode> genericArguments)
        {
            Assembly = assembly;
            Type = type;
            GenericArguments = genericArguments;
            IsArray = type.EndsWith("[]", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Assembly
        /// </summary>
        public string Assembly { get; }

        /// <summary>
        /// Type
        /// </summary>
        public string Type { get; }

        /// <summary>
        /// GenericArguments
        /// </summary>
        public IReadOnlyCollection<TypeNode> GenericArguments { get; }

        /// <summary>
        /// IsArray
        /// </summary>
        public bool IsArray { get; }

        /// <summary>
        /// Implicit conversion operator to System.String
        /// </summary>
        /// <param name="node">TypeNode</param>
        /// <returns>System.String</returns>
        public static implicit operator string(TypeNode node) => node.ToString();

        /// <summary>
        /// Implicit conversion operator from System.String
        /// </summary>
        /// <param name="type">Type</param>
        /// <returns>TypeNode</returns>
        public static implicit operator TypeNode(string type) => FromString(type);

        /// <summary>
        /// Implicit conversion operator to System.Type
        /// </summary>
        /// <param name="node">TypeNode</param>
        /// <returns>System.Type</returns>
        public static implicit operator Type(TypeNode node) => ToType(node);

        /// <summary>
        /// Implicit conversion operator from System.Type
        /// </summary>
        /// <param name="type">Type</param>
        /// <returns>TypeNode</returns>
        public static implicit operator TypeNode(Type type) => FromType(type);

        /// <inheritdoc />
        public override string ToString()
        {
            return Stringify(this, 0);

            static string Stringify(TypeNode node, int depth)
            {
                var sb = new StringBuilder();

                sb.Append(new string('\t', depth));
                sb.Append(node.Assembly);
                sb.Append(' ');

                if (node.GenericArguments.Any())
                {
                    sb.AppendLine(node.Type);

                    var lastGenericArgument = node.GenericArguments.Count - 1;

                    node.GenericArguments.Select(arg => Stringify(arg, depth + 1))
                       .Each((arg, i) =>
                       {
                           if (i < lastGenericArgument)
                           {
                               sb.AppendLine(arg);
                           }
                           else
                           {
                               sb.Append(arg);
                           }
                       });
                }
                else
                {
                    sb.Append(node.Type);
                }

                return sb.ToString();
            }
        }

        /// <summary>
        /// Converts string representation to TypeNode
        /// </summary>
        /// <param name="typeFullName">String representation of TypeNode</param>
        /// <returns>TypeNode</returns>
        public static TypeNode FromString(string typeFullName)
        {
            var infos = typeFullName
               .Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries)
               .Select(row =>
               {
                   var depth = 0;

                   while (row[depth] == '\t')
                   {
                       depth++;
                   }

                   var pair = row
                      .TrimStart('\t')
                      .Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                   var assembly = pair[0];
                   var type = pair[1];

                   return (assembly, type, depth);
               })
               .ToArray();

            var index = 0;

            return ParseType(infos, ref index, 0);

            static TypeNode ParseType(
                (string assembly, string type, int depth)[] infos,
                ref int index,
                int depth)
            {
                var assembly = infos[index].assembly;
                var type = infos[index].type;
                var genericArguments = new List<TypeNode>();

                index++;

                while (index < infos.Length && infos[index].depth > depth)
                {
                    genericArguments.Add(ParseType(infos, ref index, depth + 1));
                }

                return new TypeNode(assembly, type, genericArguments);
            }
        }

        /// <summary>
        /// Converts System.Type to TypeNode
        /// </summary>
        /// <param name="type">Type</param>
        /// <returns>TypeNode</returns>
        public static TypeNode FromType(Type type)
        {
            return new TypeNode(type);
        }

        /// <summary>
        /// Converts TypeNode to System.Type
        /// </summary>
        /// <param name="node">TypeNode</param>
        /// <returns>System.Type</returns>
        public static Type ToType(TypeNode node)
        {
            return BuildType(node);

            static Type BuildType(TypeNode node)
            {
                if (TypeInfoStorage.TryGet(node.Assembly, node.IsArray ? node.Type.Trim('[', ']') : node.Type, out var type))
                {
                    return node.GenericArguments.Any()
                        ? type.MakeGenericType(node.GenericArguments.Select(BuildType).ToArray())
                        : node.IsArray
                            ? typeof(Array)
                               .CallMethod(nameof(Array.Empty))
                               .WithTypeArgument(type)
                               .Invoke()
                               .GetType()
                            : type;
                }

                throw new SecurityException($"Untrusted type {node.Type}");
            }
        }
    }
}