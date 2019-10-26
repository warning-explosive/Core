namespace SpaceEngineers.Core.Utilities.Test
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;
    using CompositionRoot;
    using CompositionRoot.Extensions;
    using CompositionRoot.Test;
    using PathResolver;
    using Xunit;
    using Xunit.Abstractions;

    public class GraphSolverTest : TestBase
    {
        public GraphSolverTest(ITestOutputHelper output) : base(output) { }
        
        private const char A = 'A';
        private const char B = 'B';
        private const char C = 'C';
        private const char D = 'D';
        private const char E = 'E';
        
        private const string Ab1 = "AB1";
        private const string Ab2 = "AB2";
        private const string Ac = "AC";
        private const string Bb1 = "BB1";
        private const string Bb2 = "BB2";
        private const string Bd = "BD";
        private const string Bc = "BC";
        private const string Cb = "CB";
        private const string Cd = "CD";
        private const string De = "DE";
        private const string Ea = "EA";

        private static readonly IDictionary<string, GenericGraphEdge<char, string>> _edges = new Dictionary<string, GenericGraphEdge<char, string>>
        {
            [Ab1] = new GenericGraphEdge<char, string>(A, B, "AB1"),
            [Ab2] = new GenericGraphEdge<char, string>(A, B, "AB2"),
            [Ac] = new GenericGraphEdge<char, string>(A, C, "AC"),
            [Bb1] = new GenericGraphEdge<char, string>(B, B, "BB1"),
            [Bb2] = new GenericGraphEdge<char, string>(B, B, "BB2"),
            [Bd] = new GenericGraphEdge<char, string>(B, D, "BD"),
            [Bc] = new GenericGraphEdge<char, string>(B, C, "BC"),
            [Cb] = new GenericGraphEdge<char, string>(C, B, "CB"),
            [Cd] = new GenericGraphEdge<char, string>(C, D, "CD"),
            [De] = new GenericGraphEdge<char, string>(D, E, "DE"),
            [Ea] = new GenericGraphEdge<char, string>(E, A, "EA")
        };

        private static readonly ICollection<GenericGraphEdge<char, string>> _edgesHeap = _edges.Select(pair => pair.Value)
                                                                                               .ToList();

        /// <summary>
        /// Test1 - All paths
        /// </summary>
        [Fact]
        public void Test1()
        {
            var sw = new Stopwatch();
            var graph = BuildGenericGraph(sw);

            int WeightFunc(string edge) => 1;
            
            sw.Restart();
            var paths = DependencyContainer.Resolve<IPathResolver<char, string>>().GetAllGroupedWeightedPaths(graph, A, WeightFunc);
            sw.Stop();

            var strPaths = ShowGroupedWeightedPaths(paths, WeightFunc, sw);
            
            var candidates = new List<string>
            {
                "(1) => [AB1(1), AB2(1)]",
                "(1) => [AC(1)]",
                "(2) => [AB1(1), AB2(1)] => [BB1(1), BB2(1)]",
                "(2) => [AB1(1), AB2(1)] => [BD(1)]",
                "(2) => [AB1(1), AB2(1)] => [BC(1)]",
                "(2) => [AC(1)] => [CB(1)]",
                "(2) => [AC(1)] => [CD(1)]",
                "(3) => [AB1(1), AB2(1)] => [BB1(1), BB2(1)] => [BD(1)]",
                "(3) => [AB1(1), AB2(1)] => [BB1(1), BB2(1)] => [BC(1)]",
                "(3) => [AB1(1), AB2(1)] => [BD(1)] => [DE(1)]",
                "(3) => [AB1(1), AB2(1)] => [BC(1)] => [CD(1)]",
                "(3) => [AC(1)] => [CB(1)] => [BB1(1), BB2(1)]",
                "(3) => [AC(1)] => [CB(1)] => [BD(1)]",
                "(3) => [AC(1)] => [CD(1)] => [DE(1)]",
                "(4) => [AB1(1), AB2(1)] => [BB1(1), BB2(1)] => [BD(1)] => [DE(1)]",
                "(4) => [AB1(1), AB2(1)] => [BB1(1), BB2(1)] => [BC(1)] => [CD(1)]",
                "(4) => [AB1(1), AB2(1)] => [BD(1)] => [DE(1)] => [EA(1)]",
                "(4) => [AB1(1), AB2(1)] => [BC(1)] => [CD(1)] => [DE(1)]",
                "(4) => [AC(1)] => [CB(1)] => [BB1(1), BB2(1)] => [BD(1)]",
                "(4) => [AC(1)] => [CB(1)] => [BD(1)] => [DE(1)]",
                "(4) => [AC(1)] => [CD(1)] => [DE(1)] => [EA(1)]",
                "(5) => [AB1(1), AB2(1)] => [BB1(1), BB2(1)] => [BD(1)] => [DE(1)] => [EA(1)]",
                "(5) => [AB1(1), AB2(1)] => [BB1(1), BB2(1)] => [BC(1)] => [CD(1)] => [DE(1)]",
                "(5) => [AB1(1), AB2(1)] => [BD(1)] => [DE(1)] => [EA(1)] => [AC(1)]",
                "(5) => [AB1(1), AB2(1)] => [BC(1)] => [CD(1)] => [DE(1)] => [EA(1)]",
                "(5) => [AC(1)] => [CB(1)] => [BB1(1), BB2(1)] => [BD(1)] => [DE(1)]",
                "(5) => [AC(1)] => [CB(1)] => [BD(1)] => [DE(1)] => [EA(1)]",
                "(5) => [AC(1)] => [CD(1)] => [DE(1)] => [EA(1)] => [AB1(1), AB2(1)]",
                "(6) => [AB1(1), AB2(1)] => [BB1(1), BB2(1)] => [BD(1)] => [DE(1)] => [EA(1)] => [AC(1)]",
                "(6) => [AB1(1), AB2(1)] => [BB1(1), BB2(1)] => [BC(1)] => [CD(1)] => [DE(1)] => [EA(1)]",
                "(6) => [AC(1)] => [CB(1)] => [BB1(1), BB2(1)] => [BD(1)] => [DE(1)] => [EA(1)]",
                "(6) => [AC(1)] => [CD(1)] => [DE(1)] => [EA(1)] => [AB1(1), AB2(1)] => [BB1(1), BB2(1)]",
            };

            CheckCandidates(candidates, strPaths);
        }

        /// <summary>
        /// Test2 - Filtred single path
        /// </summary>
        [Fact]
        public void Test2()
        {
            var sw = new Stopwatch();
            var graph = BuildGenericGraph(sw);
            
            var requiredKeys = new Queue<char>();
            requiredKeys.Enqueue(C);
            
            var requiredEdges1 = new Queue<string>();
            requiredEdges1.Enqueue(_edges[Ab1].EdgeInfo);
            requiredEdges1.Enqueue(_edges[Bb2].EdgeInfo);
            requiredEdges1.Enqueue(_edges[Bb1].EdgeInfo);
            
            var requiredEdges2 = new Queue<string>();
            requiredEdges2.Enqueue(_edges[Ab1].EdgeInfo);
            requiredEdges2.Enqueue(_edges[Bb1].EdgeInfo);
            requiredEdges2.Enqueue(_edges[Bb2].EdgeInfo);

            var solverInfo1 = new PathResolverInfo<char, string>(A,
                                                                E,
                                                                edge => 1)
                              {
                                  RequiredKeys = requiredKeys,
                                  RequiredEdges = requiredEdges1,
                                  WithoutLoops = false
                              };

            var solverInfo2 = new PathResolverInfo<char, string>(A,
                                                                E,
                                                                edge => 1)
                              {
                                  RequiredKeys = requiredKeys,
                                  RequiredEdges = requiredEdges2,
                                  WithoutLoops = false
                              };

            var solverInfo3 = new PathResolverInfo<char, string>(A,
                                                                E,
                                                                edge => 1)
                              {
                                  RequiredKeys = requiredKeys,
                                  WithoutLoops = true
                              };

            // 1
            var strPath = GetShortestStrPath(graph, solverInfo1, sw);
            Assert.Equal("[AB1] => [BB2] => [BB1] => [BC] => [CD] => [DE]", strPath);

            // 2
            strPath = GetShortestStrPath(graph, solverInfo2, sw);
            Assert.Equal("[AB1] => [BB1] => [BB2] => [BC] => [CD] => [DE]", strPath);

            // 3
            strPath = GetShortestStrPath(graph, solverInfo3, sw);
            Assert.Equal("[AC] => [CD] => [DE]", strPath);
        }

        /// <summary>
        /// Test3 - Ambiguous paths
        /// </summary>
        [Fact]
        public void Test3()
        {
            var sw = new Stopwatch();
            var graph = BuildGenericGraph(sw);

            var solverInfo1 = new PathResolverInfo<char, string>(A,
                                                                E,
                                                                edge => 1)
                              {
                                  WithoutLoops = false
                              };

            var requiredEdges = new Queue<string>();
            requiredEdges.Enqueue(_edges[Cd].EdgeInfo);
            
            var solverInfo2 = new PathResolverInfo<char, string>(A,
                                                                E,
                                                                edge => 1)
                              {
                                  WithoutLoops = false,
                                  RequiredEdges = requiredEdges
                              };
            
            // 1
            Exception? exception = null;

            Action action = () => GetShortestStrPath(graph, solverInfo1, sw);
            action.HandleException(ex => exception = ex);
            
            Assert.NotNull(exception);
            Assert.Equal(typeof(AmbiguousMatchException), exception?.GetType());
            
            var candidates = new List<string>
            {
                "(3) => [AB1(1), AB2(1)] => [BD(1)] => [DE(1)]",
                "(3) => [AC(1)] => [CD(1)] => [DE(1)]",
            };

            var exeptionMessagePathsList = ExceptionExtensions.ThrowIfNull(exception?.Message?.Split('\n')?.ToList());
            foreach (var msg in exeptionMessagePathsList)
            {
                Output.WriteLine(msg);
            }

            CheckCandidates(candidates, exeptionMessagePathsList);
            
            var strPath = GetShortestStrPath(graph, solverInfo2, sw);
            Assert.Equal("[AC] => [CD] => [DE]", strPath);
        }

        
        /// <summary>
        /// Test4 - Not Found
        /// </summary>
        [Fact]
        public void Test4()
        {
            var sw = new Stopwatch();
            var graph = BuildGenericGraph(sw);
            
            var requiredEdges = new Queue<string>();
            requiredEdges.Enqueue(_edges[Ab1].EdgeInfo);
            requiredEdges.Enqueue(_edges[Bb2].EdgeInfo);
            requiredEdges.Enqueue(_edges[Bc].EdgeInfo);
            requiredEdges.Enqueue(_edges[Bb1].EdgeInfo);

            var solverInfo = new PathResolverInfo<char, string>(A,
                                                               E,
                                                               edge => 1)
                             {
                                 RequiredEdges = requiredEdges
                             };
            
            Exception? exception = null;

            Action action = () => GetShortestStrPath(graph, solverInfo, sw);
            action.HandleException(ex => exception = ex);
            
            Assert.NotNull(exception);
            Assert.Equal(typeof(Exception), exception?.GetType());
            Output.WriteLine(exception?.Message);
            Assert.Contains("Path not found", exception?.Message);
        }

        /// <summary>
        /// Test5 - Target status == Input status
        /// Not empty circle test
        /// </summary>
        [Fact]
        public void Test5()
        {
            var sw = new Stopwatch();
            var graph = BuildGenericGraph(sw);

            var requiredEdges = new Queue<string>();

            requiredEdges.Enqueue(_edges[Bb1].EdgeInfo);

            var solverInfo = new PathResolverInfo<char, string>(B,
                                                                B,
                                                                edge => 1);

            // 1.1
            solverInfo.NotEmptyCircle = false;
            solverInfo.RequiredEdges = null;
            var strPath = GetShortestStrPath(graph, solverInfo, sw);
            Assert.Equal(string.Empty, strPath);

            // 1.2
            solverInfo.NotEmptyCircle = true;
            solverInfo.RequiredEdges = null;
            Exception? exception = null;

            Action action = () => GetShortestStrPath(graph, solverInfo, sw); 
            action.HandleException(ex => exception = ex);
            
            Assert.NotNull(exception);
            Assert.Equal(typeof(AmbiguousMatchException), exception?.GetType());
            Output.WriteLine(exception?.Message);
            Assert.Contains("(1) => [BB1(1), BB2(1)]", exception?.Message);

            // 1.3
            solverInfo.NotEmptyCircle = false;
            solverInfo.RequiredEdges = requiredEdges;
            strPath = GetShortestStrPath(graph, solverInfo, sw);
            Assert.Equal("[BB1]", strPath);

            // 1.4
            solverInfo.NotEmptyCircle = true;
            solverInfo.RequiredEdges = requiredEdges;
            strPath = GetShortestStrPath(graph, solverInfo, sw);
            Assert.Equal("[BB1]", strPath);
        }
        
        /// <summary>
        /// Test6 - Weight test
        /// </summary>
        [Fact]
        public void Test6()
        {
            var sw = new Stopwatch();
            var graph = BuildGenericGraph(sw);

            int WeightFunc(string edge)
            {
                if (edge == Ab1) return 2;
                if (edge == Ab2) return 2;
                if (edge == Ac) return 1;
                if (edge == Bb1) return 1;
                if (edge == Bb2) return 2;
                if (edge == Bd) return 3;
                if (edge == Bc) return 4;
                if (edge == Cb) return 4;
                if (edge == Cd) return 3;
                if (edge == De) return 2;
                if (edge == Ea) return 1;

                return 1;
            }

            var solverInfo1 = new PathResolverInfo<char, string>(A,
                                                                E,
                                                                WeightFunc)
                              {
                                  WithoutLoops = false
                              };
            
            sw.Restart();
            var paths = DependencyContainer.Resolve<IPathResolver<char, string>>().GetAllGroupedWeightedPaths(graph, A, WeightFunc);
            sw.Stop();

            var strPaths = ShowGroupedWeightedPaths(paths.Where(z => z.Value.Last().Key == E), WeightFunc, sw);
            
            var candidates = new List<string>
                             {
                                 "(6) => [AC(1)] => [CD(3)] => [DE(2)]",
                                 "(7) => [AB1(2), AB2(2)] => [BD(3)] => [DE(2)]",
                                 "(8) => [AB1(2), AB2(2)] => [BB1(1), BB2(2)] => [BD(3)] => [DE(2)]",
                                 "(10) => [AC(1)] => [CB(4)] => [BD(3)] => [DE(2)]",
                                 "(11) => [AB1(2), AB2(2)] => [BC(4)] => [CD(3)] => [DE(2)]",
                                 "(11) => [AC(1)] => [CB(4)] => [BB1(1), BB2(2)] => [BD(3)] => [DE(2)]",
                                 "(12) => [AB1(2), AB2(2)] => [BB1(1), BB2(2)] => [BC(4)] => [CD(3)] => [DE(2)]", 
                             };

            CheckCandidates(candidates, strPaths);
            
            var strPath = GetShortestStrPath(graph, solverInfo1, sw);
            Assert.Equal("[AC] => [CD] => [DE]", strPath);
        }

        private GenericGraph<char, string> BuildGenericGraph(Stopwatch sw)
        {
            sw.Restart();
            var graph = new GenericGraph<char, string>(_edgesHeap);
            sw.Stop();
            
            Output.WriteLine($"[Graph build time] = {sw.ElapsedMilliseconds} ms");

            return graph;
        }

        private string GetShortestStrPath(GenericGraph<char, string> graph, PathResolverInfo<char, string> solverInfo, Stopwatch sw)
        {
            sw.Restart();
            var path = DependencyContainer.Resolve<IPathResolver<char, string>>().GetShortestPath(graph, solverInfo);
            sw.Stop();
            
            var strPath = ShowPath(path, sw);

            return strPath;
        }

        private List<string> ShowGroupedWeightedPaths(IEnumerable<KeyValuePair<int, Queue<KeyValuePair<char, ICollection<string>>>>> groupedWeightedPaths,
                                                      Func<string, int> weightFunc,
                                                      Stopwatch sw)
        {
            Output.WriteLine($"[Path Groupping time] = {sw.ElapsedMilliseconds} ms");
            
            var strPaths = new List<string>();

            foreach (var groupedWeightedPath in groupedWeightedPaths.OrderBy(z => z.Key))
            {
                var strPath = $"({groupedWeightedPath.Key}) => " +  string.Join(" => ", groupedWeightedPath.Value.Select(nodeGroup => "[" + string.Join(", ", nodeGroup.Value.Select(edge => edge + $"({weightFunc(edge)})")) + "]"));
                strPaths.Add(strPath);
                Output.WriteLine(strPath);
            }

            return strPaths;
        }

        private string ShowPath(Queue<KeyValuePair<char, string>> path, Stopwatch sw)
        {
            Output.WriteLine($"[Path search time] = {sw.ElapsedMilliseconds} ms");
            
            var strPath = string.Join(" => ", path.Select(node => "[" + node.Value + "]"));
            
            Output.WriteLine(strPath);
            return strPath;
        }

        private static void CheckCandidates(List<string> candidates, List<string> strPaths)
        {
            foreach (var c in candidates)
            {
                var occurrence = strPaths.SingleOrDefault(z => z.Equals(c));

                occurrence.ThrowIfNull($"Path not found: {c}");
            }
        }
    }
}