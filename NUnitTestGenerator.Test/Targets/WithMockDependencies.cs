// ReSharper disable ArrangeNamespaceBody

using System.Collections.Generic;

namespace NUnitTestGenerator.Test.Targets
{
    public class WithMockDependencies
    {
        public WithMockDependencies(IList<string> stringList)
        {
            StringList = stringList;
        }

        public IList<string> StringList { get; }

        public int GetStringListSize()
        {
            return StringList.Count;
        }
    }
}
