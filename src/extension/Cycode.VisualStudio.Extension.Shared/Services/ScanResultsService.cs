﻿using System.Collections.Generic;
using System.Linq;
using Cycode.VisualStudio.Extension.Shared.Cli.DTO;
using Cycode.VisualStudio.Extension.Shared.Cli.DTO.ScanResult.Iac;
using Cycode.VisualStudio.Extension.Shared.Cli.DTO.ScanResult.Sast;
using Cycode.VisualStudio.Extension.Shared.Cli.DTO.ScanResult.Sca;
using Cycode.VisualStudio.Extension.Shared.Cli.DTO.ScanResult.Secret;

namespace Cycode.VisualStudio.Extension.Shared.Services;

public interface IScanResultsService {
    void SetSecretResults(SecretScanResult result);
    SecretScanResult GetSecretResults();
    void SetScaResults(ScaScanResult result);
    ScaScanResult GetScaResults();
    void SetIacResults(IacScanResult result);
    IacScanResult GetIacResults();
    void SetSastResults(SastScanResult result);
    SastScanResult GetSastResults();
    void Clear();
    bool HasResults();
    void SaveDetectedSegment(CliScanType scanType, TextRange textRange, string value);
    string GetDetectedSegment(CliScanType scanType, TextRange textRange);
    void ExcludeResultsByValue(string value);
}

public class ScanResultsService : IScanResultsService {
    private readonly Dictionary<(CliScanType, TextRange), string> _detectedSegments = new();
    private IacScanResult _iacResults;
    private SastScanResult _sastResults;
    private ScaScanResult _scaResults;

    private SecretScanResult _secretResults;

    public void SetSecretResults(SecretScanResult result) {
        ClearDetectedSegments(CliScanType.Secret);
        _secretResults = result;
    }

    public SecretScanResult GetSecretResults() {
        return _secretResults;
    }

    public void SetScaResults(ScaScanResult result) {
        ClearDetectedSegments(CliScanType.Sca);
        _scaResults = result;
    }

    public ScaScanResult GetScaResults() {
        return _scaResults;
    }

    public void SetIacResults(IacScanResult result) {
        ClearDetectedSegments(CliScanType.Iac);
        _iacResults = result;
    }

    public IacScanResult GetIacResults() {
        return _iacResults;
    }

    public void SetSastResults(SastScanResult result) {
        _sastResults = result;
    }

    public SastScanResult GetSastResults() {
        return _sastResults;
    }

    public void Clear() {
        _secretResults = null;
        _scaResults = null;
        _iacResults = null;
        _sastResults = null;
        ClearDetectedSegments();
    }

    public bool HasResults() {
        return _secretResults != null || _scaResults != null || _iacResults != null || _sastResults != null;
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
        _secretResults?.Detections.RemoveAll(detection => detection.DetectionDetails.DetectedValue == value);
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