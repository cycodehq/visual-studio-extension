﻿namespace Cycode.VisualStudio.Extension.Shared
{
    using System.Windows;
    using System.Windows.Controls;

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