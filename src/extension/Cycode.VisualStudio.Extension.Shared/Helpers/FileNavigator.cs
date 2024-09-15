using System.IO;
using Cycode.VisualStudio.Extension.Shared.Services;
using EnvDTE;
using EnvDTE80;

namespace Cycode.VisualStudio.Extension.Shared.Helpers;

public static class FileNavigator {
    public static void NavigateToFileAndLine(string filePath, int lineNumber) {
        ThreadHelper.ThrowIfNotOnUIThread();

        ILoggerService logger = ServiceLocator.GetService<ILoggerService>();

        try {
            DTE2 dte = (DTE2)Package.GetGlobalService(typeof(DTE));
            if (dte == null) return;

            Document activeDoc = dte.ActiveDocument;

            string normalizedDocFilePath = Path.GetFullPath(activeDoc.FullName);
            string normalizedFilePath = Path.GetFullPath(filePath);
            if (normalizedDocFilePath != normalizedFilePath) dte.ItemOperations.OpenFile(filePath);

            activeDoc = dte.ActiveDocument;

            TextSelection selection = (TextSelection)activeDoc.Selection;
            selection.GotoLine(lineNumber, true);
        } catch (Exception e) {
            logger.Error(e, "Failed to navigate to file: {0} line: {1}", filePath, lineNumber);
        }
    }
}