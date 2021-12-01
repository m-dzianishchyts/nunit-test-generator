using NUnit.Framework;
using Moq;
using System.Collections.Generic;

[TestFixture]
class WithValueTypeDependenciesTest
{
    private WithValueTypeDependencies _withValueTypeDependencies;
    [SetUp]
    public void SetUp()
    {
        var path = default(string);
        var maxSize = default(int);
        var _withValueTypeDependencies = new WithValueTypeDependencies(path, maxSize);
    }

    [Test]
    public void ReadFile()
    {
        var actual = _withValueTypeDependencies.ReadFile();
        var expected = default(string);
        Assert.AreEqual(expected, actual);
        Assert.Fail("Generated");
    }
}