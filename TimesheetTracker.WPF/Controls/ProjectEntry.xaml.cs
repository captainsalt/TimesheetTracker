using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TimesheetTracker.Controls
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class ProjectEntry : UserControl
    {
        public ProjectEntry()
        {
            InitializeComponent();

            for (int i = 0; i < 20; i++)
            {
                var thickness = new Thickness(1, 0, 1, 0);
                dailyHoursStack.Children.Add(new TextBox() { Width = 30, TextAlignment = TextAlignment.Center, Text = "8", Margin = thickness, FontSize = 20 });
            }
        }
    }
}
