namespace NUnitTestGenerator.Core.ItemInfo;

internal class SourceFileInformation
{
    public SourceFileInformation(IEnumerable<TypeInformation> types)
    {
        Types = types.ToList();
    }

    public IList<TypeInformation> Types { get; set; }
}
