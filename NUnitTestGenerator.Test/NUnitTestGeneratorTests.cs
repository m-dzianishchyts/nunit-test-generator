using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using NUnit.Framework;
using NUnitTestGenerator.Core;

namespace NUnitTestGenerator.Test;

[TestFixture]
public class NUnitTestGeneratorTests
{
    private const string TargetDirectoryPath =
        @"C:\Users\maxiemar\source\repos\NUnitTestGenerator\NUnitTestGenerator.Test\NUnitTestGenerationTargets";

    private const string OutputDirectoryPath =
        @"C:\Users\maxiemar\source\repos\NUnitTestGenerator\NUnitTestGenerator.Test\GeneratedTests";

    readonly IList<string> _targetFilePaths;

    public NUnitTestGeneratorTests()
    {
        _targetFilePaths = Directory.GetFiles(TargetDirectoryPath);
    }

    [SetUp]
    public void Setup()
    {
        Directory.CreateDirectory(OutputDirectoryPath);
    }

    [Test]
    public async Task NUnitTestGenerator_GeneratesFiles()
    {
        await Task.Run(() => GenerationGoalPipeline.PrepareGoal(_targetFilePaths, OutputDirectoryPath));

        IList<string> generatedFilePaths = Directory.GetFiles(OutputDirectoryPath);
        Assert.AreEqual(1, generatedFilePaths.Count);
    }
}
