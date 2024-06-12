namespace Cycode.VisualStudio.Extension.Shared.Cli;

public static class ErrorHandling {
    public static ErrorCode DetectErrorCode(string error) {
        // TODO(MarshalX): Implement error code detection logic
        return ErrorCode.Unknown;
    }

    public static string GetUserFriendlyCliErrorMessage(ErrorCode errorCode) {
        // TODO(MarshalX): Implement user-friendly message retrieval
        return "Error message";
    }
}