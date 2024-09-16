using System.Linq;

namespace Cycode.VisualStudio.Extension.Shared.Helpers;

public static class CweCveLinkHelper {
    private static string GetCweCveLink(string cweCve) {
        if (string.IsNullOrEmpty(cweCve)) return null;

        if (cweCve.StartsWith("GHSA")) return $"https://github.com/advisories/{cweCve}";

        if (cweCve.StartsWith("CWE")) {
            string[] splitCwe = cweCve.Split('-');
            if (splitCwe.Length > 1 &&
                int.TryParse(new string(splitCwe[1].TakeWhile(char.IsDigit).ToArray()), out int cweNumber))
                return $"https://cwe.mitre.org/data/definitions/{cweNumber}";

            return null;
        }

        if (cweCve.StartsWith("CVE")) return $"https://cve.mitre.org/cgi-bin/cvename.cgi?name={cweCve}";

        return null;
    }

    public static string RenderCweCveLinkHtml(string cweCve) {
        string link = GetCweCveLink(cweCve);
        if (link != null) return $"<a href=\"{link}\" target=\"_blank\">{cweCve}</a>";

        return cweCve ?? "";
    }

    public static string RenderCweCveLinkMarkdown(string cweCve) {
        string link = GetCweCveLink(cweCve);
        if (link != null) return $"[{cweCve}]({link})";

        return cweCve ?? "";
    }
}