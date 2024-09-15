using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Cycode.VisualStudio.Extension.Shared.Cli.DTO;
using Cycode.VisualStudio.Extension.Shared.DTO;
using Cycode.VisualStudio.Extension.Shared.Options;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;

namespace Cycode.VisualStudio.Extension.Shared.Services;

public interface IDocTableEventsHandlerService {
    void Initialize(IServiceProvider serviceProvider);
    void Deinitialize();
}

public class DocTableEventsHandlerService(
    ILoggerService logger,
    ICycodeService cycode,
    IStateService stateService
) : IDocTableEventsHandlerService, IVsRunningDocTableEvents {
    private readonly HashSet<string> _collectedPathsToScan = [];
    private readonly object _lock = new();
    private readonly ExtensionState _pluginState = stateService.Load();

    private IServiceProvider _serviceProvider;
    private uint rdtCookie;

    public void Initialize(IServiceProvider serviceProvider) {
        logger.Info("Initializing DocTableEventsHandlerService");

        _serviceProvider = serviceProvider;

        ThreadHelper.ThrowIfNotOnUIThread();
        IVsRunningDocumentTable rdt =
            (IVsRunningDocumentTable)_serviceProvider.GetService(typeof(SVsRunningDocumentTable));
        rdt?.AdviseRunningDocTableEvents(this, out rdtCookie);

        ScheduleScanPathsFlush();
    }

    public void Deinitialize() {
        // FIXME(MarshalX): Someone need to call this method xd

        // release cookie
        ThreadHelper.ThrowIfNotOnUIThread();
        IVsRunningDocumentTable rdt =
            (IVsRunningDocumentTable)_serviceProvider.GetService(typeof(SVsRunningDocumentTable));
        rdt?.UnadviseRunningDocTableEvents(rdtCookie);
    }

    public int OnAfterFirstDocumentLock(
        uint docCookie, uint dwRDTLockType, uint dwReadLocksRemaining, uint dwEditLocksRemaining
    ) {
        return VSConstants.S_OK;
    }

    public int OnBeforeLastDocumentUnlock(
        uint docCookie, uint dwRDTLockType, uint dwReadLocksRemaining, uint dwEditLocksRemaining
    ) {
        return VSConstants.S_OK;
    }

    public int OnAfterSave(uint docCookie) {
        ThreadHelper.JoinableTaskFactory.RunAsync(async () => {
            try {
                General general = await General.GetLiveInstanceAsync();
                if (!general.ScanOnSave) return;

                string fileFullPath = await GetActiveFullPathAsync();
                if (fileFullPath != null)
                    lock (_lock) {
                        _collectedPathsToScan.Add(fileFullPath);
                    }
            } catch (Exception ex) {
                logger.Error(ex, "Failed to handle OnAfterSave event");
            }
        }).FireAndForget();

        return VSConstants.S_OK;
    }

    public int OnAfterAttributeChange(uint docCookie, uint grfAttribs) {
        return VSConstants.S_OK;
    }

    public int OnBeforeDocumentWindowShow(uint docCookie, int fFirstShow, IVsWindowFrame pFrame) {
        return VSConstants.S_OK;
    }

    public int OnAfterDocumentWindowHide(uint docCookie, IVsWindowFrame pFrame) {
        return VSConstants.S_OK;
    }

    private void ScheduleScanPathsFlush() {
        TimeSpan initialDelay = TimeSpan.FromSeconds(Constants.PluginAutoSaveFlushInitialDelaySec);
        TimeSpan flushDelay = TimeSpan.FromSeconds(Constants.PluginAutoSaveFlushDelaySec);

        // TODO(MarshalX): graceful shutdown?
        ThreadHelper.JoinableTaskFactory.RunAsync(async () => {
            await Task.Delay(initialDelay);
            while (true) {
                await ScanPathsFlushAsync();
                await Task.Delay(flushDelay);
            }
        });
    }

    private static List<string> ExcludeNotExistingPaths(List<string> paths) {
        return paths.Where(File.Exists).ToList();
    }

    private static List<string> ExcludeNonScaRelatedPaths(List<string> paths) {
        return paths.Where(ScaHelper.IsSupportedPackageFile).ToList();
    }

    private async Task ScanPathsFlushAsync() {
        List<string> pathsToScan;
        lock (_lock) {
            pathsToScan = ExcludeNotExistingPaths(_collectedPathsToScan.ToList());
            _collectedPathsToScan.Clear();
        }

        if (!_pluginState.CliAuthed) return;

        if (pathsToScan.Any()) await cycode.StartPathSecretScanAsync(pathsToScan);

        List<string> scaPathsToScan = ExcludeNonScaRelatedPaths(pathsToScan);
        if (scaPathsToScan.Any()) await cycode.StartPathScaScanAsync(scaPathsToScan);
    }

    private static async Task<string> GetActiveFullPathAsync() {
        SolutionItem activeItem = await VS.Solutions.GetActiveItemAsync();
        if (activeItem != null) return activeItem.FullPath;

        DocumentView documentView = await VS.Documents.GetActiveDocumentViewAsync();
        return documentView?.FilePath;
    }
}