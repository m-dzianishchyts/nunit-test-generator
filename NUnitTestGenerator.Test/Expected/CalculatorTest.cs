using NUnit.Framework;
using Moq;
using System.Collections.Generic;
using NUnitTestGenerator.Test.Targets;

[TestFixture]
class CalculatorTest
{
    private Calculator _calculator;
    [SetUp]
    public void SetUp()
    {
        var _calculator = new Calculator();
    }

    [Test]
    public void Sum()
    {
        var a = default(int);
        var b = default(int);
        var actual = _calculator.Sum(a, b);
        var expected = default(int);
        Assert.AreEqual(expected, actual);
        Assert.Fail("Generated");
    }

    [Test]
    public void Subtract()
    {
        var a = default(int);
        var b = default(int);
        var actual = _calculator.Subtract(a, b);
        var expected = default(int);
        Assert.AreEqual(expected, actual);
        Assert.Fail("Generated");
    }

    [Test]
    public void Product()
    {
        var a = default(int);
        var b = default(int);
        var actual = _calculator.Product(a, b);
        var expected = default(int);
        Assert.AreEqual(expected, actual);
        Assert.Fail("Generated");
    }

    [Test]
    public void Divide()
    {
        var a = default(int);
        var b = default(int);
        var actual = _calculator.Divide(a, b);
        var expected = default(int);
        Assert.AreEqual(expected, actual);
        Assert.Fail("Generated");
    }
}