namespace NUnitTestGenerator.ItemInfo
{
    internal class MethodInformation
    {
        public string Name { get; }
        public string ReturnType { get; }
        public IDictionary<string, string> ParametersNameTypeDictionary { get; }

        public MethodInformation(string name, string returnType,
                                 IDictionary<string, string> parametersNameTypeDictionary)
        {
            ReturnType = returnType;
            Name = name;
            ParametersNameTypeDictionary = parametersNameTypeDictionary
                .ToDictionary(pair => pair.Key, pair => pair.Value);
        }
    }
}
