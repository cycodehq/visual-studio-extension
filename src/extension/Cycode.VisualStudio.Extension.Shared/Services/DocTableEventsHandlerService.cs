using System.Threading.Tasks;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;

namespace Cycode.VisualStudio.Extension.Shared.Services;

public class DocTableEventsHandlerService : IVsRunningDocTableEvents {
    private IServiceProvider _serviceProvider;
    private uint rdtCookie;

    public void Initialize(IServiceProvider serviceProvider) {
        _serviceProvider = serviceProvider;

        ThreadHelper.ThrowIfNotOnUIThread();
        IVsRunningDocumentTable rdt =
            (IVsRunningDocumentTable)_serviceProvider.GetService(typeof(SVsRunningDocumentTable));
        rdt?.AdviseRunningDocTableEvents(this, out rdtCookie);
    }

    public void Deinitialize() {
        // FIXME(MarshalX): Someone need to call this method xd

        // release cookie
        ThreadHelper.ThrowIfNotOnUIThread();
        IVsRunningDocumentTable rdt =
            (IVsRunningDocumentTable)_serviceProvider.GetService(typeof(SVsRunningDocumentTable));
        rdt?.UnadviseRunningDocTableEvents(rdtCookie);
    }

    private static async Task<string> GetActiveFullPathAsync() {
        SolutionItem activeItem = await VS.Solutions.GetActiveItemAsync();
        if (activeItem != null) {
            return activeItem.FullPath;
        }

        DocumentView documentView = await VS.Documents.GetActiveDocumentViewAsync();
        return documentView?.FilePath;
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
                if (!general.ScanOnSave) {
                    return;
                }

                ILoggerService logger = ServiceLocator.GetService<ILoggerService>();
                logger.Debug("OnAfterSave called");
                ICycodeService cycode = ServiceLocator.GetService<ICycodeService>();

                // TODO(MarshalX): save events to batches and flush every N seconds
                await cycode.StartPathSecretScanAsync(await GetActiveFullPathAsync());
            } catch (Exception) {
                // ignore
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
}