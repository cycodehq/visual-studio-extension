using System.Reflection;
using System.Windows.Media.Imaging;
using Microsoft.VisualStudio.Imaging.Interop;
using Microsoft.VisualStudio.Shell.Interop;

namespace Cycode.VisualStudio.Extension.Shared.Icons;

public static class ExtensionIcons {
    private static readonly string _assemblyName = Assembly.GetExecutingAssembly().GetName().Name;

    public static readonly string ScanTypeSecrets = GetResourcePath("ScanType/Secrets.png");
    public static readonly string ScanTypeSca = GetResourcePath("ScanType/SCA.png");
    public static readonly string ScanTypeIac = GetResourcePath("ScanType/IaC.png");
    public static readonly string ScanTypeSast = GetResourcePath("ScanType/SAST.png");

    private static readonly string _severityCritical = GetResourcePath("Severity/C.png");
    private static readonly string _severityHigh = GetResourcePath("Severity/H.png");
    private static readonly string _severityMedium = GetResourcePath("Severity/M.png");
    private static readonly string _severityLow = GetResourcePath("Severity/L.png");
    private static readonly string _severityInfo = GetResourcePath("Severity/I.png");

    private static readonly string _cardSeverityCritical = GetResourcePath("CardSeverity/C.png");
    private static readonly string _cardSeverityHigh = GetResourcePath("CardSeverity/H.png");
    private static readonly string _cardSeverityMedium = GetResourcePath("CardSeverity/M.png");
    private static readonly string _cardSeverityLow = GetResourcePath("CardSeverity/L.png");
    private static readonly string _cardSeverityInfo = GetResourcePath("CardSeverity/I.png");

    private static string GetResourcePath(string resourceName) {
        // ref: https://stackoverflow.com/a/57504059/8032027
        return $"pack://application:,,,/{_assemblyName};component/Resources/{resourceName}";
    }

    public static string GetSeverityIconPath(string severity) {
        return severity.ToLower() switch {
            "critical" => _severityCritical,
            "high" => _severityHigh,
            "medium" => _severityMedium,
            "low" => _severityLow,
            "info" => _severityInfo,
            _ => _severityInfo
        };
    }

    private static string GetCardSeverityIconPath(string severity) {
        return severity.ToLower() switch {
            "critical" => _cardSeverityCritical,
            "high" => _cardSeverityHigh,
            "medium" => _cardSeverityMedium,
            "low" => _cardSeverityLow,
            "info" => _cardSeverityInfo,
            _ => _cardSeverityInfo
        };
    }

    public static BitmapSource GetCardSeverityBitmapSource(string severity, int size = 40) {
        string iconPath = GetCardSeverityIconPath(severity);

        BitmapImage bitmapImage = new(new Uri(iconPath, UriKind.RelativeOrAbsolute)) {
            DecodePixelWidth = size
        };

        return bitmapImage;
    }

    public static BitmapSource GetFileIconPath(string filename, int size = 16) {
        ThreadHelper.ThrowIfNotOnUIThread();

        IVsImageService2 imageService = (IVsImageService2)Package.GetGlobalService(typeof(SVsImageService));
        ImageMoniker fileIcon = imageService.GetImageMonikerForFile(filename);

        return fileIcon.ToBitmapSourceAsync(size).Result;
    }
}