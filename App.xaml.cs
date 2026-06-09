using System;
using System.Windows;
using System.Windows.Threading;

namespace BnsMaterialTracker
{
    public partial class App : Application
    {
        public App()
        {
            DispatcherUnhandledException += OnDispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += OnDomainUnhandledException;
        }

        private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            MessageBox.Show(
                $"未處理的例外：\n{e.Exception.GetType().Name}: {e.Exception.Message}\n\n{e.Exception.StackTrace}",
                "程式錯誤", MessageBoxButton.OK, MessageBoxImage.Error);
            e.Handled = true;   // prevent app from closing
        }

        private void OnDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            // Background-thread exceptions — show message then let it crash (can't recover)
            if (e.ExceptionObject is Exception ex)
                MessageBox.Show(
                    $"背景執行緒例外（程式將關閉）：\n{ex.GetType().Name}: {ex.Message}\n\n{ex.StackTrace}",
                    "嚴重錯誤", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
