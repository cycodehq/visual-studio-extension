using System.Collections.Generic;
using System.Linq;
using Cycode.VisualStudio.Extension.Shared.Cli.DTO.ScanResult.Iac;

namespace Cycode.VisualStudio.Extension.Shared.Services.ErrorList.TaskCreators;

public static class IacErrorTaskCreator {
    public static List<ErrorTask> CreateErrorTasks(List<IacDetection> detections) {
        List<ErrorTask> errorTasks = [];

        errorTasks.AddRange(detections.Select(detection => new ErrorTask {
            Text = $"Cycode: {detection.GetFormattedTitle()}",
            Line = detection.DetectionDetails.LineInFile,
            Document = detection.DetectionDetails.GetFilePath(),
            Category = TaskCategory.User,
            ErrorCategory = ErrorCategoryUtilities.GetTaskErrorCategory(detection.Severity),
            Priority = TaskPriority.High
        }));

        return errorTasks;
    }
}