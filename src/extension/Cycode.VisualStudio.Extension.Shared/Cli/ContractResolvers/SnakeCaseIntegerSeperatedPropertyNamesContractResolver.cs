using System.Text.RegularExpressions;
using Newtonsoft.Json.Serialization;

namespace Cycode.VisualStudio.Extension.Shared.Cli;

public class SnakeCaseIntegerSeperatedPropertyNamesContractResolver : DefaultContractResolver {
    protected override string ResolvePropertyName(string propertyName) {
        Match startUnderscores = Regex.Match(propertyName, @"^_+");
        return startUnderscores + Regex.Replace(propertyName, @"([A-Z0-9])", "_$1").ToLower().TrimStart('_');
    }
}