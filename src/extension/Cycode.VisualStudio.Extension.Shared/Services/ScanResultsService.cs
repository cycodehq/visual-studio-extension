using System.Collections.Generic;
using System.Linq;
using Cycode.VisualStudio.Extension.Shared.Cli.DTO;
using Cycode.VisualStudio.Extension.Shared.Cli.DTO.ScanResult;
using Cycode.VisualStudio.Extension.Shared.Cli.DTO.ScanResult.Iac;
using Cycode.VisualStudio.Extension.Shared.Cli.DTO.ScanResult.Sast;
using Cycode.VisualStudio.Extension.Shared.Cli.DTO.ScanResult.Sca;
using Cycode.VisualStudio.Extension.Shared.Cli.DTO.ScanResult.Secret;

namespace Cycode.VisualStudio.Extension.Shared.Services;

public interface IScanResultsService {
    void SetDetections<T>(CliScanType scanType, List<T> detections);
    IEnumerable<DetectionBase> GetDetections(CliScanType scanType);
    List<SecretDetection> GetSecretDetections();
    List<ScaDetection> GetScaDetections();
    List<IacDetection> GetIacDetections();
    List<SastDetection> GetSastDetections();
    void Clear();
    bool HasResults();
    void SaveDetectedSegment(CliScanType scanType, TextRange textRange, string value);
    string GetDetectedSegment(CliScanType scanType, TextRange textRange);
    void ExcludeResultsByValue(string value);
}

public class ScanResultsService : IScanResultsService {
    private readonly Dictionary<(CliScanType, TextRange), string> _detectedSegments = new();
    private List<IacDetection> _iacScanDetections = [];
    private List<SastDetection> _sastScanDetections = [];
    private List<ScaDetection> _scaScanDetections = [];

    private List<SecretDetection> _secretScanDetections = [];

    public void SetDetections<T>(CliScanType scanType, List<T> detections) {
        ClearDetectedSegments(scanType);
        switch (detections) {
            case List<SecretDetection> secretDetections:
                _secretScanDetections = secretDetections;
                break;
            case List<ScaDetection> scaDetections:
                _scaScanDetections = scaDetections;
                break;
            case List<IacDetection> iacDetections:
                _iacScanDetections = iacDetections;
                break;
            case List<SastDetection> sastDetections:
                _sastScanDetections = sastDetections;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(scanType), scanType, null);
        }
    }

    public IEnumerable<DetectionBase> GetDetections(CliScanType scanType) {
        return scanType switch {
            CliScanType.Secret => _secretScanDetections,
            CliScanType.Sca => _scaScanDetections,
            CliScanType.Iac => _iacScanDetections,
            CliScanType.Sast => _sastScanDetections,
            _ => throw new ArgumentOutOfRangeException(nameof(scanType), scanType, null)
        };
    }

    public List<SecretDetection> GetSecretDetections() {
        return _secretScanDetections;
    }

    public List<ScaDetection> GetScaDetections() {
        return _scaScanDetections;
    }

    public List<IacDetection> GetIacDetections() {
        return _iacScanDetections;
    }

    public List<SastDetection> GetSastDetections() {
        return _sastScanDetections;
    }

    public void Clear() {
        _secretScanDetections = [];
        _scaScanDetections = [];
        _iacScanDetections = [];
        _sastScanDetections = [];
        ClearDetectedSegments();
    }

    public bool HasResults() {
        return _secretScanDetections.Any() ||
               _scaScanDetections.Any() ||
               _iacScanDetections.Any() ||
               _sastScanDetections.Any();
    }

    public void SaveDetectedSegment(CliScanType scanType, TextRange textRange, string value) {
        _detectedSegments[(scanType, textRange)] = value;
    }

    public string GetDetectedSegment(CliScanType scanType, TextRange textRange) {
        _detectedSegments.TryGetValue((scanType, textRange), out string value);
        return value;
    }

    public void ExcludeResultsByValue(string value) {
        // we have value only in secret results
        _secretScanDetections.RemoveAll(detection => detection.DetectionDetails.DetectedValue == value);
    }

    private void ClearDetectedSegments(CliScanType? scanType = null) {
        if (scanType == null) {
            _detectedSegments.Clear();
        } else {
            List<(CliScanType, TextRange)> keysToRemove =
                _detectedSegments.Keys.Where(key => key.Item1 == scanType).ToList();

            foreach ((CliScanType, TextRange) key in keysToRemove) _detectedSegments.Remove(key);
        }
    }
}

public class TextRange {
    // Implementation details would go here
}