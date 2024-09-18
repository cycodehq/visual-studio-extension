using System.Collections.Generic;
using Cycode.VisualStudio.Extension.Shared.Cli.DTO.ScanResult;
using Cycode.VisualStudio.Extension.Shared.Cli.DTO.ScanResult.Iac;
using Cycode.VisualStudio.Extension.Shared.Cli.DTO.ScanResult.Sast;
using Cycode.VisualStudio.Extension.Shared.Cli.DTO.ScanResult.Sca;
using Cycode.VisualStudio.Extension.Shared.Cli.DTO.ScanResult.Secret;
using Cycode.VisualStudio.Extension.Shared.DTO;
using Cycode.VisualStudio.Extension.Shared.Services.ErrorList.TaskCreators;

namespace Cycode.VisualStudio.Extension.Shared.Services.ErrorList;

public interface IErrorTaskCreatorService {
    Task RecreateAsync();
    Task ClearErrorsAsync();
}

public class ErrorTaskCreatorService(
    IErrorListService errorListService,
    IScanResultsService scanResultsService,
    IToolWindowMessengerService toolWindowMessengerService
) : IErrorTaskCreatorService {
    public async Task RecreateAsync() {
        errorListService.ClearErrors();

        await CreateErrorTasksAsync(scanResultsService.GetSecretResults());
        await CreateErrorTasksAsync(scanResultsService.GetScaResults());
        await CreateErrorTasksAsync(scanResultsService.GetIacResults());
        await CreateErrorTasksAsync(scanResultsService.GetSastResults());

        CycodePackage.ErrorTaggerProvider.Rerender();
        toolWindowMessengerService.Send(new MessageEventArgs(MessengerCommand.TreeViewRefresh));
    }

    public async Task ClearErrorsAsync() {
        scanResultsService.Clear();
        await RecreateAsync();
    }

    private async Task CreateErrorTasksAsync(ScanResultBase scanResults) {
        List<ErrorTask> errorTasks = [];

        switch (scanResults) {
            case SecretScanResult secretScanResult:
                errorTasks = SecretsErrorTaskCreator.CreateErrorTasks(secretScanResult);
                break;
            case ScaScanResult scaScanResult:
                errorTasks = ScaErrorTaskCreator.CreateErrorTasks(scaScanResult);
                break;
            case IacScanResult iacScanResult:
                errorTasks = IacErrorTaskCreator.CreateErrorTasks(iacScanResult);
                break;
            case SastScanResult sastScanResult:
                errorTasks = SastErrorTaskCreator.CreateErrorTasks(sastScanResult);
                break;
        }

        await errorListService.AddErrorTasksAsync(errorTasks);
    }
}