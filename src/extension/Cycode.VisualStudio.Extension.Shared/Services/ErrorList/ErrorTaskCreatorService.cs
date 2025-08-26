using System.Collections.Generic;
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
        await CreateErrorTasksAsync();

        CycodePackage.ErrorTaggerProvider?.Rerender();
        toolWindowMessengerService.Send(new MessageEventArgs(MessengerCommand.TreeViewRefresh));
    }

    public async Task ClearErrorsAsync() {
        scanResultsService.Clear();
        await RecreateAsync();
    }

    private async Task CreateErrorTasksAsync() {
        List<ErrorTask> errorTasks = [];

        errorTasks.AddRange(SecretsErrorTaskCreator.CreateErrorTasks(scanResultsService.GetSecretDetections()));
        errorTasks.AddRange(ScaErrorTaskCreator.CreateErrorTasks(scanResultsService.GetScaDetections()));
        errorTasks.AddRange(IacErrorTaskCreator.CreateErrorTasks(scanResultsService.GetIacDetections()));
        errorTasks.AddRange(SastErrorTaskCreator.CreateErrorTasks(scanResultsService.GetSastDetections()));

        await errorListService.AddErrorTasksAsync(errorTasks);
    }
}