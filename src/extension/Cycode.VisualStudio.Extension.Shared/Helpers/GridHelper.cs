using System.Windows;
using System.Windows.Controls;

namespace Cycode.VisualStudio.Extension.Shared.Helpers;

public static class GridHelper {
    public static void HideRow(Grid grid, int rowIndex) {
        grid.RowDefinitions[rowIndex].Height = new GridLength(0);
    }

    public static void ShowRow(Grid grid, int rowIndex) {
        grid.RowDefinitions[rowIndex].Height = new GridLength(1, GridUnitType.Auto);
    }
}