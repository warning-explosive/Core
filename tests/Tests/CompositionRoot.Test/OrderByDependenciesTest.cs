namespace SpaceEngineers.Core.CompositionRoot.Test;

public class OrderByDependenciesTest
{
    [Fact]
    internal void OrderByDependencyCycleDependencyTest()
    {
        var test1 = new[]
        {
            typeof(OrderByDependencyTestData.CycleDependencyTest1),
            typeof(OrderByDependencyTestData.CycleDependencyTest2),
            typeof(OrderByDependencyTestData.CycleDependencyTest3)
        };

        Assert.Throws<InvalidOperationException>(() => test1.OrderByDependencies().ToArray());

        var test2 = new[]
        {
            typeof(OrderByDependencyTestData.CycleDependencyTest1),
            typeof(OrderByDependencyTestData.CycleDependencyTest2),
            typeof(OrderByDependencyTestData.CycleDependencyTest3),
            typeof(OrderByDependencyTestData.CycleDependencyTest4)
        };

        Assert.Throws<InvalidOperationException>(() => test2.OrderByDependencies().ToArray());
    }

    [Fact]
    internal void OrderByDependencyTest()
    {
        var test1 = new[]
        {
            typeof(OrderByDependencyTestData.DependencyTest1),
            typeof(OrderByDependencyTestData.DependencyTest2),
            typeof(OrderByDependencyTestData.DependencyTest3),
            typeof(OrderByDependencyTestData.DependencyTest4)
        };

        Assert.True(test1.Reverse().SequenceEqual(test1.OrderByDependencies()));

        var test2 = new[]
        {
            typeof(OrderByDependencyTestData.DependencyTest1),
            typeof(OrderByDependencyTestData.DependencyTest2),
            typeof(OrderByDependencyTestData.DependencyTest3),
            typeof(OrderByDependencyTestData.DependencyTest4)
        };

        Assert.True(test2.Reverse().SequenceEqual(test2.OrderByDependencies()));

        var test3 = new[]
        {
            typeof(OrderByDependencyTestData.GenericDependencyTest1<>),
            typeof(OrderByDependencyTestData.GenericDependencyTest2<>),
            typeof(OrderByDependencyTestData.GenericDependencyTest3<>),
            typeof(OrderByDependencyTestData.GenericDependencyTest4<>)
        };

        Assert.True(test3.Reverse().SequenceEqual(test3.OrderByDependencies()));

        var test4 = new[]
        {
            typeof(OrderByDependencyTestData.GenericDependencyTest1<object>),
            typeof(OrderByDependencyTestData.GenericDependencyTest2<string>),
            typeof(OrderByDependencyTestData.GenericDependencyTest3<int>),
            typeof(OrderByDependencyTestData.GenericDependencyTest4<bool>)
        };

        Assert.True(test4.Reverse().SequenceEqual(test4.OrderByDependencies()));
    }
}