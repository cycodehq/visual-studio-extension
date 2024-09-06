using Cycode.VisualStudio.Extension.Shared.DTO;
using Cycode.VisualStudio.Extension.Shared.Services;

namespace Cycode.VisualStudio.Extension.Shared;

[Command(PackageIds.BackToHomeScreenCommand)]
internal sealed class BackToHomeScreenCommand : BaseCommand<BackToHomeScreenCommand> {
    protected override void Execute(object sender, EventArgs e) {
        IToolWindowMessengerService toolWindowMessengerService =
            ServiceLocator.GetService<IToolWindowMessengerService>();
        toolWindowMessengerService.Send(new MessageEventArgs(MessengerCommand.BackToHomeScreen));
    }
}