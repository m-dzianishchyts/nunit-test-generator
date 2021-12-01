using System.Threading.Tasks.Dataflow;

namespace NUnitTestGenerator.Core;

public static class GenerationGoalPipeline
{
    public static Task PrepareGoal(ICollection<string> sourcePaths, string outputPath)
    {
        int sourceLoadingMaxDegreeOfParallelism = sourcePaths.Count;
        int testsGenerationMaxDegreeOfParallelism = sourcePaths.Count;
        int testsWritingMaxDegreeOfParallelism = sourcePaths.Count;
        var linkOptions = new DataflowLinkOptions { PropagateCompletion = true };
        TransformBlock<string, string> sourceLoading = PrepareSourceLoading(sourceLoadingMaxDegreeOfParallelism);
        TransformManyBlock<string, KeyValuePair<string, string>> testGeneration = PrepareTestGeneration(testsGenerationMaxDegreeOfParallelism);
        ActionBlock<KeyValuePair<string, string>> testWriting = PrepareTestWriting(testsWritingMaxDegreeOfParallelism, outputPath);

        sourceLoading.LinkTo(testGeneration, linkOptions);
        testGeneration.LinkTo(testWriting, linkOptions);

        foreach (string sourcePath in sourcePaths)
        {
            sourceLoading.Post(sourcePath);
        }

        sourceLoading.Complete();
        return testWriting.Completion;
    }

    private static ActionBlock<KeyValuePair<string, string>> PrepareTestWriting(int maxDegreeOfParallelism, string outputPath)
    {
        var executionOptions = new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = maxDegreeOfParallelism };
        var writeFileBlock = new ActionBlock<KeyValuePair<string, string>>
        (
            async fileNameTestSourcePair =>
            {
                (string fileName, string testSource) = fileNameTestSourcePair;
                string filePath = Path.Join(outputPath, Path.ChangeExtension(fileName, "cs"));
                await File.WriteAllTextAsync(filePath, testSource);
            },
            executionOptions
        );
        return writeFileBlock;
    }

    private static TransformManyBlock<string, KeyValuePair<string, string>> PrepareTestGeneration(int maxDegreeOfParallelism)
    {
        var executionOptions = new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = maxDegreeOfParallelism };
        var testGeneration = new TransformManyBlock<string, KeyValuePair<string, string>>
        (
            async sourceFileContent =>
            {
                return await Task.Run(() => NUnitTestGenerator.GenerateTests(sourceFileContent));
            },
            executionOptions
        );
        return testGeneration;
    }

    private static TransformBlock<string, string> PrepareSourceLoading(int maxDegreeOfParallelism)
    {
        var executionOptions = new ExecutionDataflowBlockOptions {MaxDegreeOfParallelism = maxDegreeOfParallelism};
        var downloadStringBlock = new TransformBlock<string, string>
        (
            async sourceFilePath =>
            {
                using var reader = new StreamReader(sourceFilePath);
                string sourceFileContent = await reader.ReadToEndAsync();
                return sourceFileContent;
            },
            executionOptions
        );
        return downloadStringBlock;
    }
}
