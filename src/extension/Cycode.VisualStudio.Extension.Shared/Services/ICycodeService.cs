namespace Cycode.VisualStudio.Extension.Shared.Services;

public interface ICycodeService {
    Task InstallCliIfNeededAndCheckAuthenticationAsync();
}