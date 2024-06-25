using System.Collections.Generic;

namespace Cycode.VisualStudio.Extension.Shared.Services;

public interface IErrorListService {
    void Initialize(IServiceProvider serviceProvider);

    Task AddErrorTasksAsync(List<ErrorTask> errorTasks);

    Task AddErrorTaskAsync(ErrorTask task);

    void ClearErrors();
}