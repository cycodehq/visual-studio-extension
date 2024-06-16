using System.Text.RegularExpressions;
using Newtonsoft.Json.Serialization;

namespace Cycode.VisualStudio.Extension.Shared.JsonContractResolvers;

public class SnakeCaseIntegerSeperatedPropertyNamesContractResolver : DefaultContractResolver {
    private readonly Regex _converter = new Regex(@"((?<=[a-z])(?<b>[A-Z0-9])|(?<=[^_])(?<b>[A-Z][a-z]))");

    protected override string ResolvePropertyName(string propertyName) {
        return _converter.Replace(propertyName, "_${b}").ToLower();
    }
}