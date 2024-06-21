using System.ComponentModel;
using System.Windows.Controls;
using Cycode.VisualStudio.Extension.Shared.DTO;
using Cycode.VisualStudio.Extension.Shared.Services;

namespace Cycode.VisualStudio.Extension.Shared;

public class CycodeToolWindowViewModel : INotifyPropertyChanged {
    private UserControl _currentView;

    public UserControl CurrentView {
        get => _currentView;
        set {
            _currentView = value;
            OnPropertyChanged(nameof(CurrentView));
        }
    }

    public CycodeToolWindowViewModel(IToolWindowMessengerService toolWindowMessengerService) {
        toolWindowMessengerService.MessageReceived += OnMessageReceived;
        CurrentView = new LoadingControl();
    }

    public event PropertyChangedEventHandler PropertyChanged;

    private void OnPropertyChanged(string propertyName) {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private void OnMessageReceived(object sender, string e) {
        CurrentView = e switch {
            MessengerCommand.LoadLoadingControl => new LoadingControl(),
            MessengerCommand.LoadAuthControl => new AuthControl(),
            MessengerCommand.LoadMainControl => new MainControl(),
            _ => CurrentView
        };
    }
}