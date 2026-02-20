using System.Windows;

namespace TimesheetTracker.WPF;
/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        _ = Initialize();
    }

    async Task Initialize()
    {
        var viewModel = new MainWindowViewModel();
        await viewModel.Initialize();
        DataContext = viewModel;
        InitializeComponent();
    }
}
