using NUnit.Framework;
using Moq;
using System.Collections.Generic;
using NUnitTestGenerator.Test.Targets;

[TestFixture]
class WithMockDependenciesTest
{
    private Mock<IList<string>> _stringList;
    private WithMockDependencies _withMockDependencies;
    [SetUp]
    public void SetUp()
    {
        var stringList = default(IList<string>);
        var _stringList = new Mock<IList<string>>();
        var _withMockDependencies = new WithMockDependencies(_stringList.Object);
    }

    [Test]
    public void GetStringListSize()
    {
        var actual = _withMockDependencies.GetStringListSize();
        var expected = default(int);
        Assert.AreEqual(expected, actual);
        Assert.Fail("Generated");
    }
}