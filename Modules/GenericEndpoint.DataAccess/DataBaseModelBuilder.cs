namespace SpaceEngineers.Core.GenericEndpoint.DataAccess
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using AutoWiring.Api.Attributes;
    using AutoWiring.Api.Enumerations;
    using Basics;
    using GenericDomain.Abstractions;

    [Component(EnLifestyle.Singleton)]
    internal class DataBaseModelBuilder : IDataBaseModelBuilder
    {
        public IEnumerable<ModelNode> BuildNodes(Type type)
        {
            return new StatefulDataBaseModelBuilder(type).BuildNode();
        }

        private class StatefulDataBaseModelBuilder
        {
            private readonly Type _rootType;
            private readonly IDictionary<Type, ModelNode> _map;

            private bool _validateCyclicDependency;

            public StatefulDataBaseModelBuilder(Type rootType)
            {
                _rootType = rootType;
                _map = new Dictionary<Type, ModelNode>();

                _validateCyclicDependency = false;
            }

            internal IEnumerable<ModelNode> BuildNode()
            {
                _ = BuildNode(_rootType);

                return _map.Values;
            }

            private ModelNode BuildNode(Type type)
            {
                ValidateCyclicDependency(type);

                return _map.GetOrAdd(type, NodeProducer);
            }

            private void ValidateCyclicDependency(Type type)
            {
                if (_validateCyclicDependency)
                {
                    if (type == _rootType)
                    {
                        throw new InvalidOperationException($"Model contains cyclic dependencies in {type.FullName}");
                    }
                }
                else
                {
                    _validateCyclicDependency = true;
                }
            }

            private ModelNode NodeProducer(Type type)
            {
                if (type.IsPrimitive()
                    || typeof(EnumerationObject).IsAssignableFrom(type))
                {
                    return BuildPrimitiveNode(type);
                }

                if (typeof(IEntity).IsAssignableFrom(type)
                    || typeof(IValueObject).IsAssignableFrom(type))
                {
                    return BuildComplexNode(type, _map);
                }

                throw new NotSupportedException($"Not supported model entity: {type.FullName}");
            }

            private static ModelNode BuildPrimitiveNode(Type type)
            {
                return new ModelNode(type, Enumerable.Empty<ModelNode>());
            }

            private ModelNode BuildComplexNode(Type type, IDictionary<Type, ModelNode> map)
            {
                var childNodes = type
                    .GetProperties(BindingFlags.Instance
                                   | BindingFlags.Public
                                   | BindingFlags.NonPublic
                                   | BindingFlags.GetProperty)
                    .Select(property => BuildNode(property.PropertyType));

                return new ModelNode(type, childNodes);
            }
        }
    }
}