using System.IO;

namespace NUnitTestGenerator.Test.Targets;

public class WithValueTypeDependencies
{
    private readonly string _path;
    private readonly int _maxSize;

    public WithValueTypeDependencies(string path, int maxSize)
    {
        _path = path;
        _maxSize = maxSize;
    }

    public string ReadFile()
    {
        string content = File.ReadAllText(_path);
        return content[.._maxSize];
    }
}
