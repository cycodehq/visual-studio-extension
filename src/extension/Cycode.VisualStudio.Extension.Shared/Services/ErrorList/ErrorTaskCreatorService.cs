using System.Collections.Generic;
using Cycode.VisualStudio.Extension.Shared.Cli.DTO.ScanResult;
using Cycode.VisualStudio.Extension.Shared.Cli.DTO.ScanResult.Sca;
using Cycode.VisualStudio.Extension.Shared.Cli.DTO.ScanResult.Secret;

namespace Cycode.VisualStudio.Extension.Shared.Services.ErrorList;

public class ErrorTaskCreatorService(
    IErrorListService errorListService,
    IScanResultsService scanResultsService
) : IErrorTaskCreatorService {
    private async Task CreateErrorTasksAsync(ScanResultBase scanResults) {
        List<ErrorTask> errorTasks = [];

        switch (scanResults) {
            case SecretScanResult secretScanResult:
                errorTasks = SecretsErrorTaskCreator.CreateErrorTasks(secretScanResult);
                break;
            case ScaScanResult scaScanResult:
                errorTasks = ScaErrorTaskCreator.CreateErrorTasks(scaScanResult);
                break;
        }

        await errorListService.AddErrorTasksAsync(errorTasks);
    }

    public async Task RecreateAsync() {
        errorListService.ClearErrors();

        await CreateErrorTasksAsync(scanResultsService.GetSecretResults());
        await CreateErrorTasksAsync(scanResultsService.GetScaResults());

        CycodePackage.ErrorTaggerProvider.Rerender();
    }

    public async Task ClearErrorsAsync() {
        scanResultsService.Clear();
        await RecreateAsync();
    }
}