﻿using System.Collections.Generic;
using System.Linq;
using Cycode.VisualStudio.Extension.Shared.Cli.DTO.ScanResult.Sca;

namespace Cycode.VisualStudio.Extension.Shared.Services.ErrorList.TaskCreators;

public static class ScaErrorTaskCreator {
    public static List<ErrorTask> CreateErrorTasks(List<ScaDetection> detections) {
        List<ErrorTask> errorTasks = [];

        errorTasks.AddRange(detections.Select(detection => new ErrorTask {
            Text = $"Cycode: {detection.GetFormattedTitle()}",
            Line = detection.DetectionDetails.LineInFile - 1,
            Document = detection.DetectionDetails.GetFilePath(),
            Category = TaskCategory.User,
            ErrorCategory = ErrorCategoryUtilities.GetTaskErrorCategory(detection.Severity),
            Priority = TaskPriority.High
        }));

        return errorTasks;
    }
}