namespace Cycode.VisualStudio.Extension.Shared;

[Command(PackageIds.OpenSettingsCommand)]
internal sealed class OpenSettingsCommand : BaseCommand<OpenSettingsCommand> {
    protected override void Execute(object sender, EventArgs e) {
        Package.ShowOptionPage(typeof(OptionsProvider.GeneralOptions));
    }
}