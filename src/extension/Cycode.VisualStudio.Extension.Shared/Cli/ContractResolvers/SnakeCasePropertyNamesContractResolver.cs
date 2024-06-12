using System.Text.RegularExpressions;
using Newtonsoft.Json.Serialization;

namespace Cycode.VisualStudio.Extension.Shared.Cli;

public class SnakeCasePropertyNamesContractResolver : DefaultContractResolver {
    protected override string ResolvePropertyName(string propertyName) {
        Match startUnderscores = Regex.Match(propertyName, @"^_+");
        return startUnderscores + Regex.Replace(propertyName, @"([A-Z])", "_$1").ToLower().TrimStart('_');
    }
}