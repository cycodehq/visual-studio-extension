﻿using Cycode.VisualStudio.Extension.Shared.Cli.DTO;
using Cycode.VisualStudio.Extension.Shared.Cli.DTO.ScanResult.Secret;

namespace Cycode.VisualStudio.Extension.Shared.Services;

public interface IScanResultsService {
    void SetSecretResults(SecretScanResult result);
    SecretScanResult GetSecretResults();
    void Clear();
    bool HasResults();
    void SaveDetectedSegment(CliScanType scanType, TextRange textRange, string value);
    string GetDetectedSegment(CliScanType scanType, TextRange textRange);
}