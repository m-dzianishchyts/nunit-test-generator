namespace NUnitTestGenerator.Core.ItemInfo;

internal class TypeInformation
{
    public TypeInformation(string? namespaceName, string name, string innerName, IEnumerable<MethodInformation> methods,
                           IEnumerable<ConstructorInformation> constructors)
    {
        NamespaceName = namespaceName;
        Name = name;
        InnerName = innerName;
        Methods = methods.ToList();
        Constructors = constructors.ToList();
    }

    public string? NamespaceName { get; set; }
    public string Name { get; set; }
    public string InnerName { get; set; }
    public IList<MethodInformation> Methods { get; set; }
    public IList<ConstructorInformation> Constructors { get; set; }
}
