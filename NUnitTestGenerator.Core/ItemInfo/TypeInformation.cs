﻿namespace NUnitTestGenerator.Core.ItemInfo;

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

    public string? NamespaceName { get; }
    public string Name { get; }
    public string InnerName { get; }
    public IList<MethodInformation> Methods { get; }
    public IList<ConstructorInformation> Constructors { get; }
}
