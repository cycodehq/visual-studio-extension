using System.Collections.Generic;
using System.Linq;
using Cycode.VisualStudio.Extension.Shared.Cli.DTO.ScanResult.Secret;

namespace Cycode.VisualStudio.Extension.Shared.Services.ErrorList.TaskCreators;

public static class SecretsErrorTaskCreator {
    public static List<ErrorTask> CreateErrorTasks(List<SecretDetection> detections) {
        List<ErrorTask> errorTasks = [];

        errorTasks.AddRange(detections.Select(detection => new ErrorTask {
            Text = $"Cycode: {detection.GetFormattedTitle()}",
            Line = detection.DetectionDetails.GetLineNumber() - 1,
            Document = detection.DetectionDetails.GetFilePath(),
            Category = TaskCategory.User,
            ErrorCategory = ErrorCategoryUtilities.GetTaskErrorCategory(detection.Severity),
            Priority = TaskPriority.High
        }));

        return errorTasks;
    }
}