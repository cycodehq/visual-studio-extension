using System.Collections.Generic;
using System.Linq;
using Cycode.VisualStudio.Extension.Shared.Cli.DTO.ScanResult.Secret;

namespace Cycode.VisualStudio.Extension.Shared.Services.ErrorList.TaskCreators;

public static class SecretsErrorTaskCreator {
    public static List<ErrorTask> CreateErrorTasks(SecretScanResult scanResult) {
        List<ErrorTask> errorTasks = [];

        errorTasks.AddRange(scanResult.Detections.Select(detection => new ErrorTask {
            Text = $"Cycode: {detection.GetFormattedTitle()}",
            Line = detection.DetectionDetails.Line,
            Document = detection.DetectionDetails.GetFilePath(),
            Category = TaskCategory.User,
            ErrorCategory = ErrorCategoryUtilities.GetTaskErrorCategory(detection.Severity),
            Priority = TaskPriority.High
        }));

        return errorTasks;
    }
}