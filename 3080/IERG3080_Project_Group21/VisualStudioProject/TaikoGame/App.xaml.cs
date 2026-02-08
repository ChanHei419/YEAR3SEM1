using System.Windows;

namespace TaikoGame
{
public partial class App : Application
{
protected override void OnStartup(StartupEventArgs e)
{
    base.OnStartup(e);
}

private void Application_DispatcherUnhandledException(object sender, 
    System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
{
    MessageBox.Show($"error: {e.Exception.Message}", "game error");
    e.Handled = true;
}
}
}
