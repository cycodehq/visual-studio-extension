namespace Cycode.VisualStudio.Extension.Shared;

[Command(PackageIds.ToolbarOpenSettingsCommand)]
internal sealed class OpenSettingsCommand : BaseCommand<OpenSettingsCommand> {
    protected override void Execute(object sender, EventArgs e) {
        Package.ShowOptionPage(typeof(OptionsProvider.GeneralOptions));
    }
}

[Command(PackageIds.TopMenuOpenSettingsCommand)]
internal sealed class TopMenuOpenSettingsCommand : BaseCommand<TopMenuOpenSettingsCommand> {
    protected override void Execute(object sender, EventArgs e) {
        Package.ShowOptionPage(typeof(OptionsProvider.GeneralOptions));
    }
}