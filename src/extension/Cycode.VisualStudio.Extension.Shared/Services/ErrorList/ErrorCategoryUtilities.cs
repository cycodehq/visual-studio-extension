namespace Cycode.VisualStudio.Extension.Shared.Services.ErrorList;

public static class ErrorCategoryUtilities {
    public static TaskErrorCategory GetTaskErrorCategory(string severity) {
        return severity.ToLower() switch {
            "critical" => TaskErrorCategory.Error,
            "high" => TaskErrorCategory.Error,
            "medium" => TaskErrorCategory.Warning,
            "low" => TaskErrorCategory.Message,
            "info" => TaskErrorCategory.Message,
            _ => TaskErrorCategory.Warning
        };
    }
}