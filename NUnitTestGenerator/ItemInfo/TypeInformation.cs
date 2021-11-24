namespace NUnitTestGenerator.ItemInfo
{
    internal class TypeInformation
    {
        public string Name { get; }
        public IList<MethodInformation> Methods { get; }
        public IList<ConstructorInformation> Constructors { get; }

        public TypeInformation(string name, IEnumerable<MethodInformation> methods,
                               IEnumerable<ConstructorInformation> constructors)
        {
            Name = name;
            Methods = methods.ToList();
            Constructors = constructors.ToList();
        }
    }
}
