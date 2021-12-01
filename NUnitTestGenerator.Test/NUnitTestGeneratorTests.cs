using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using NUnit.Framework;
using NUnitTestGenerator.Core;
using NUnitTestGenerator.Test.Targets;

namespace NUnitTestGenerator.Test;

[TestFixture]
public class NUnitTestGeneratorTests
{
    private const string PathToTargetDirectory =
        @"C:\Users\maxiemar\source\repos\NUnitTestGenerator\NUnitTestGenerator.Test\Targets";

    private const string PathToOutputDirectory =
        @"C:\Users\maxiemar\source\repos\NUnitTestGenerator\NUnitTestGenerator.Test\Generated";

    private const string PathToExpectedDirectory =
        @"C:\Users\maxiemar\source\repos\NUnitTestGenerator\NUnitTestGenerator.Test\Expected";

    private const string TestClassPostfix = "Test";
    private const string CSharpExtension = "cs";

    readonly IList<string> _pathsToTargetFiles;

    public NUnitTestGeneratorTests()
    {
        _pathsToTargetFiles = Directory.GetFiles(PathToTargetDirectory);
    }

    [SetUp]
    public void Setup()
    {
        Directory.CreateDirectory(PathToOutputDirectory);
    }

    [Test]
    public async Task NUnitTestGenerator_GeneratesFiles()
    {
        await Task.Run(() => GenerationGoalPipeline.PrepareGoal(_pathsToTargetFiles, PathToOutputDirectory));

        const int expectedResultFilesAmount = 2;
        IList<string> pathToResultFiles = Directory.GetFiles(PathToOutputDirectory);
        Assert.AreEqual(expectedResultFilesAmount, pathToResultFiles.Count);
    }

    [Test]
    public async Task NUnitTestGenerator_GeneratesTestsFor_TrivialClass()
    {
        const string testClassName = nameof(Trivial);
        string pathToTargetFile = Path.ChangeExtension(Path.Combine(PathToTargetDirectory, testClassName),
                                                       CSharpExtension);
        await Task.Run(() => GenerationGoalPipeline.PrepareGoal(new[] { pathToTargetFile }, PathToOutputDirectory));

        string testFileName = Path.ChangeExtension($"{testClassName}{TestClassPostfix}", CSharpExtension);
        string pathToExpectedFile = Path.Combine(PathToExpectedDirectory, testFileName);
        string pathToResultFile = Path.Combine(PathToOutputDirectory, testFileName);
        FileAssert.AreEqual(pathToExpectedFile, pathToResultFile);
    }

    [Test]
    public async Task NUnitTestGenerator_GeneratesTestsFor_CalcClass()
    {
        const string testClassName = nameof(Calculator);
        string pathToTargetFile = Path.ChangeExtension(Path.Combine(PathToTargetDirectory, testClassName),
                                                       CSharpExtension);
        await Task.Run(() => GenerationGoalPipeline.PrepareGoal(new[] { pathToTargetFile }, PathToOutputDirectory));

        string testFileName = Path.ChangeExtension($"{testClassName}{TestClassPostfix}", CSharpExtension);
        string pathToExpectedFile = Path.Combine(PathToExpectedDirectory, testFileName);
        string pathToResultFile = Path.Combine(PathToOutputDirectory, testFileName);
        FileAssert.AreEqual(pathToExpectedFile, pathToResultFile);
    }
}
