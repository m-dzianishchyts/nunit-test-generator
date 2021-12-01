using NUnit.Framework;
using Moq;
using System.Collections.Generic;
using NUnitTestGenerator.Test.Targets;

[TestFixture]
class TrivialTest
{
    private Trivial _trivial;
    [SetUp]
    public void SetUp()
    {
        var _trivial = new Trivial();
    }
}