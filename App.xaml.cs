using System;
using System.Windows;
using System.Runtime;

namespace TikTak;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : System.Windows.Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        try
        {
            // JIT optimizasyonu için profil kullan
            ProfileOptimization.SetProfileRoot(System.IO.Path.GetTempPath());
            ProfileOptimization.StartProfile("TikTak.Profile");
            
            // Windows Forms integration
            System.Windows.Forms.Application.EnableVisualStyles();
            System.Windows.Forms.Application.SetCompatibleTextRenderingDefault(false);
            
            base.OnStartup(e);
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show($"Startup Error: {ex.Message}\n\nStack Trace:\n{ex.StackTrace}", "Application Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
    {
        System.Windows.MessageBox.Show($"Unhandled Exception: {e.Exception.Message}\n\nStack Trace:\n{e.Exception.StackTrace}", "Application Error", MessageBoxButton.OK, MessageBoxImage.Error);
        e.Handled = true;
    }
}

