using System.Text.RegularExpressions;
using Newtonsoft.Json.Serialization;

namespace Cycode.VisualStudio.Extension.Shared.Cli;

public class SnakeCasePropertyNamesContractResolver : DefaultContractResolver {
    private readonly Regex _converter = new Regex(@"((?<=[a-z])(?<b>[A-Z])|(?<=[^_])(?<b>[A-Z][a-z]))");

    protected override string ResolvePropertyName(string propertyName) {
        return _converter.Replace(propertyName, "_${b}").ToLower();
    }
}