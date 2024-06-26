namespace Cycode.VisualStudio.Extension.Shared.Services;

public interface IDocTableEventsHandlerService {
    void Initialize(IServiceProvider serviceProvider);
    void Deinitialize();
}