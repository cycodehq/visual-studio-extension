using System.ComponentModel;
using System.Windows.Controls;
using Cycode.VisualStudio.Extension.Shared.Cli.DTO.ScanResult.Sca;
using Cycode.VisualStudio.Extension.Shared.Cli.DTO.ScanResult.Secret;
using Cycode.VisualStudio.Extension.Shared.Components.TreeView;
using Cycode.VisualStudio.Extension.Shared.Components.ViolationCards;
using Cycode.VisualStudio.Extension.Shared.DTO;
using Cycode.VisualStudio.Extension.Shared.Services;

namespace Cycode.VisualStudio.Extension.Shared.Components.ToolWindows;

public class CycodeToolWindowViewModel : INotifyPropertyChanged {
    private readonly CycodeTreeViewControl _cycodeTreeView;
    private UserControl _leftSideView;
    private UserControl _rightSideView;

    public CycodeToolWindowViewModel(IToolWindowMessengerService toolWindowMessengerService) {
        toolWindowMessengerService.MessageReceived += OnMessageReceived;

        _cycodeTreeView = new CycodeTreeViewControl();

        LeftSideView = _cycodeTreeView;
        RightSideView = new LoadingControl();
    }

    public UserControl LeftSideView {
        get => _leftSideView;
        set {
            _leftSideView = value;
            OnPropertyChanged(nameof(LeftSideView));
        }
    }

    public UserControl RightSideView {
        get => _rightSideView;
        set {
            _rightSideView = value;
            OnPropertyChanged(nameof(RightSideView));
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    private void OnPropertyChanged(string propertyName) {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private void OnMessageReceived(object sender, MessageEventArgs args) {
        RightSideView = args.Command switch {
            MessengerCommand.LoadLoadingControl => new LoadingControl(),
            MessengerCommand.LoadAuthControl => new AuthControl(),
            MessengerCommand.LoadMainControl => new MainControl(),
            MessengerCommand.LoadSecretViolationCardControl => new SecretViolationCardControl((SecretDetection) args.Data),
            MessengerCommand.LoadScaViolationCardControl => new ScaViolationCardControl((ScaDetection) args.Data),
            _ => RightSideView
        };

        if (args.Command == MessengerCommand.RefreshTreeView) _cycodeTreeView.RefreshTree();
    }
}