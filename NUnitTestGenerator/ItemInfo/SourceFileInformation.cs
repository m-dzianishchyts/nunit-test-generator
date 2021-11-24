namespace NUnitTestGenerator.ItemInfo
{
    internal class SourceFileInformation
    {
        public IList<TypeInformation> Types { get; }

        public SourceFileInformation(IEnumerable<TypeInformation> types)
        {
            Types = types.ToList();
        }
    }
}
