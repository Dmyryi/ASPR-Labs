using System.Windows;
using System.Windows.Input;

namespace Lab01.App;

public partial class MainWindow : Window
{
    private const string MaximizeIconGlyph = "\uE922";
    private const string RestoreIconGlyph = "\uE923";

    public MainWindow()
    {
        InitializeComponent();
        StateChanged += OnWindowStateChanged;
    }

    private void TopBar_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ClickCount == 2)
        {
            ToggleMaximize();
            return;
        }
        if (e.LeftButton == MouseButtonState.Pressed)
            DragMove();
    }

    private void MinimizeButton_OnClick(object sender, RoutedEventArgs e)
        => WindowState = WindowState.Minimized;

    private void MaximizeButton_OnClick(object sender, RoutedEventArgs e)
        => ToggleMaximize();

    private void CloseButton_OnClick(object sender, RoutedEventArgs e) => Close();

    private void ToggleMaximize()
        => WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;

    private void OnWindowStateChanged(object? sender, EventArgs e)
    {
        if (Content is FrameworkElement root)
            root.Margin = WindowState == WindowState.Maximized ? new Thickness(0) : new Thickness(20);

        MaximizeIcon.Text = WindowState == WindowState.Maximized ? RestoreIconGlyph : MaximizeIconGlyph;
    }
}
