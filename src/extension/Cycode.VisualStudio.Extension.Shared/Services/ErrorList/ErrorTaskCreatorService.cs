using System.Collections.Generic;
using Cycode.VisualStudio.Extension.Shared.Cli.DTO.ScanResult;
using Cycode.VisualStudio.Extension.Shared.Cli.DTO.ScanResult.Secret;

namespace Cycode.VisualStudio.Extension.Shared.Services.ErrorList;

public class ErrorTaskCreatorService(
    IErrorListService errorListService, 
    IScanResultsService scanResultsService
    ) : IErrorTaskCreatorService {
    private async Task CreateErrorTasksAsync(ScanResultBase scanResults) {
        if (scanResults is SecretScanResult secretScanResult) {
            List<ErrorTask> errorTasks = SecretsErrorTaskCreator.CreateErrorTasks(secretScanResult);
            await errorListService.AddErrorTasksAsync(errorTasks);
        }
    }

    public async Task RecreateAsync() {
        errorListService.ClearErrors();
        await CreateErrorTasksAsync(scanResultsService.GetSecretResults());
    }
}