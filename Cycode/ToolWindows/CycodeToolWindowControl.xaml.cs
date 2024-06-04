using System.Windows;
using System.Windows.Controls;

namespace Cycode
{
    public partial class CycodeToolWindowControl : UserControl
    {
        public CycodeToolWindowControl()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            VS.MessageBox.Show("Cycode", "Button clicked");
        }
    }
}