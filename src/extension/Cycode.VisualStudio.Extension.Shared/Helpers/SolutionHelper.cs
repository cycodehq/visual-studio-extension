using System.IO;

namespace Cycode.VisualStudio.Extension.Shared.Helpers;

public static class SolutionHelper {
    public static string GetSolutionRootDirectory() {
        string solutionRoot = VS.Solutions.GetCurrentSolution()?.FullPath;

        return File.Exists(solutionRoot)
            ?
            // full path contains path to the sln file; we need the dir
            Path.GetDirectoryName(solutionRoot)
            :
            // full path will be dir in non-sln projects
            solutionRoot;
    }
}