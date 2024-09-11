﻿using Cycode.VisualStudio.Extension.Shared.Cli.DTO;
using Cycode.VisualStudio.Extension.Shared.Components.ToolWindows;
using Cycode.VisualStudio.Extension.Shared.DTO;
using Cycode.VisualStudio.Extension.Shared.Services.ErrorList;

namespace Cycode.VisualStudio.Extension.Shared.Services.SuggestedActions.Actions;

public class OpenViolationCardAction(DetectionTag tag) : BaseAction {
    private readonly ILoggerService _logger = ServiceLocator.GetService<ILoggerService>();

    private static readonly IToolWindowMessengerService _toolWindowMessengerService =
        ServiceLocator.GetService<IToolWindowMessengerService>();

    public override string DisplayText {
        get {
            string text = Tag.Detection.GetFormattedMessage();
            // cut too long messages
            if (text.Length > 50) {
                text = text.Substring(0, 50) + "...";
            }

            return $"Cycode: {text}";
        }
    }

    private DetectionTag Tag { get; } = tag;

    protected override void Invoke() {
        _logger.Debug("OpenViolationCardAction: Invoke");

        string command = Tag.DetectionType switch {
            CliScanType.Secret => MessengerCommand.LoadSecretViolationCardControl,
            CliScanType.Sca => MessengerCommand.LoadScaViolationCardControl,
            _ => null
        };

        if (command == null) return;

        _toolWindowMessengerService.Send(new MessageEventArgs(command, Tag.Detection));
        CycodeToolWindow.ShowAsync().FireAndForget();
    }
}