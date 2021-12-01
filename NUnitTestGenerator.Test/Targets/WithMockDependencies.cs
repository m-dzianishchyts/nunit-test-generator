// ReSharper disable ArrangeNamespaceBody

using System.Collections.Generic;

namespace NUnitTestGenerator.Test.Targets
{
    public class WithMockDependencies
    {
        public IList<string> StringList { get; }

        public WithMockDependencies(IList<string> stringList)
        {
            StringList = stringList;
        }

        public int GetStringListSize()
        {
            return StringList.Count;
        }
    }
}
