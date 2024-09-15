using System.Collections.Generic;
using System.Threading.Tasks;
using Cycode.VisualStudio.Extension.Shared.Components.ToolWindows;
using Microsoft.VisualStudio.Shell.Interop;

namespace Cycode.VisualStudio.Extension.Shared.Services.ErrorList;

public interface IErrorListService {
    void Initialize(IServiceProvider serviceProvider);

    Task AddErrorTasksAsync(List<ErrorTask> errorTasks);

    Task AddErrorTaskAsync(ErrorTask task);

    void ClearErrors();
}

public class ErrorListService : IErrorListService {
    private static readonly List<ErrorTask> _errorTask = [];
    private ErrorListProvider _errorListProvider;

    public void Initialize(IServiceProvider serviceProvider) {
        _errorListProvider = new ErrorListProvider(serviceProvider);
        _errorListProvider.ProviderName = Vsix.Name;
        _errorListProvider.ProviderGuid = PackageGuids.Cycode;
    }

    public async Task AddErrorTasksAsync(List<ErrorTask> errorTasks) {
        _errorListProvider.SuspendRefresh();

        foreach (ErrorTask errorTask in errorTasks) await AddErrorTaskAsync(errorTask);

        _errorListProvider.ResumeRefresh();
        CycodeToolWindow.ShowAsync().FireAndForget();
    }

    public async Task AddErrorTaskAsync(ErrorTask task) {
        task.HierarchyItem ??= await GetHierarchyItemAsync(task.Document);
        task.Navigate += (sender, _) => {
            if (sender is not ErrorTask errorTask) return;

            // navigate counts lines differently that UI shows
            task.Line++;
            _errorListProvider.Navigate(errorTask, new Guid(EnvDTE.Constants.vsViewKindTextView));
            task.Line--;
        };

        AddErrorTask(task);
    }

    public void ClearErrors() {
        foreach (ErrorTask task in _errorTask) _errorListProvider.Tasks.Remove(task);

        _errorTask.Clear();
        RefreshErrorList();
    }

    private void AddErrorTask(ErrorTask task) {
        _errorTask.Add(task);
        _errorListProvider.Tasks.Add(task);
    }

    private void RefreshErrorList() {
        _errorListProvider.Refresh();
        CycodeToolWindow.ShowAsync().FireAndForget();
    }

    private static async Task<IVsHierarchy> GetHierarchyItemAsync(string filePath) {
        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
        IEnumerable<IVsHierarchy> projects = await VS.Solutions.GetAllProjectHierarchiesAsync();

        VSDOCUMENTPRIORITY[] priority = new VSDOCUMENTPRIORITY[1];

        foreach (IVsHierarchy hierarchy in projects) {
            IVsProject proj = (IVsProject)hierarchy;
            proj.IsDocumentInProject(filePath, out int isFound, priority, out uint _);

            if (isFound == 1) return hierarchy;
        }

        return null;
    }
}