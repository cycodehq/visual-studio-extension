using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Imaging.Interop;
using Microsoft.VisualStudio.Language.Intellisense;

namespace Cycode.VisualStudio.Extension.Shared.Services.SuggestedActions.Actions;

public abstract class BaseAction : ISuggestedAction {
    public void Dispose() {
    }

    public bool TryGetTelemetryId(out Guid telemetryId) {
        telemetryId = Guid.Empty;
        return false;
    }

    public Task<IEnumerable<SuggestedActionSet>> GetActionSetsAsync(CancellationToken cancellationToken) {
        return Task.FromResult<IEnumerable<SuggestedActionSet>>(null);
    }

    public Task<object> GetPreviewAsync(CancellationToken cancellationToken) {
        return Task.FromResult<object>(null);
    }

    public void Invoke(CancellationToken cancellationToken) {
        if (cancellationToken.IsCancellationRequested) return;
        Invoke();
    }

    public bool HasActionSets { get; } = false;
    public ImageMoniker IconMoniker { get; } = default;
    public string IconAutomationText { get; } = null;
    public string InputGestureText { get; } = null;
    public bool HasPreview { get; } = false;

    public abstract string DisplayText { get; }

    protected abstract void Invoke();
}