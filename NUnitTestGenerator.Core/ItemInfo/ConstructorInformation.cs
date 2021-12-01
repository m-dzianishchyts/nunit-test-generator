namespace NUnitTestGenerator.Core.ItemInfo;

internal class ConstructorInformation
{
    public ConstructorInformation(string name, IDictionary<string, string> parametersNameTypeDictionary)
    {
        Name = name;
        ParametersNameTypeDictionary = parametersNameTypeDictionary
            .ToDictionary(pair => pair.Key, pair => pair.Value);
    }

    public string Name { get; set; }
    public IDictionary<string, string> ParametersNameTypeDictionary { get; set; }
}
