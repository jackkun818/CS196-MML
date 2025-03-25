using crs.window;
using HandyControl.Tools;
using Microsoft.VisualBasic.ApplicationServices;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using ShutdownMode = System.Windows.ShutdownMode;
using StartupEventArgs = Microsoft.VisualBasic.ApplicationServices.StartupEventArgs;

namespace crs.window
{
    class Startup
    {
        [STAThread]
        public static void Main(string[] args)
        {
            var wrapper = new SingleInstanceApplicationWrapper();
            wrapper.Run(args);
        }

        public class SingleInstanceApplicationWrapper : WindowsFormsApplicationBase
        {
            App app;

            public SingleInstanceApplicationWrapper()
            {
                this.IsSingleInstance = true;
            }

            protected override bool OnStartup(StartupEventArgs e)
            {
                app = new App();
                app.InitializeComponent();

                app.ShutdownMode = ShutdownMode.OnMainWindowClose;
                app.Run();

                return false;
            }

            protected override void OnStartupNextInstance(StartupNextInstanceEventArgs e)
            {
                app?.Dispatcher.Invoke(() =>
                {
                    if (app.MainWindow.WindowState == WindowState.Minimized)
                    {
                        app.MainWindow.WindowState = WindowState.Normal;
                        app.MainWindow.Show();
                    }
                    app.MainWindow.Activate();
                });
            }
        }
    }
}

