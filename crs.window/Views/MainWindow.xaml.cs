using HandyControl.Data;
using System.Windows;
using crs.extension;
using System.Linq;
using System.Windows.Forms;
using System;
using HandyControl.Tools;
using System.Windows.Interop;
using Prism.Events;
using Prism.Ioc;
using Prism.Regions;
using static crs.extension.Crs_EventAggregator;
using System.IO;
using Microsoft.Identity.Client.NativeInterop;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Text;

namespace crs.window.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
#if DEBUG
        readonly static string configPath = @".\configs\window_max.json";

        static MainWindow()
        {
            var dir = new DirectoryInfo(Path.GetDirectoryName(configPath));
            if (!dir.Exists)
            {
                dir.Create();
            }
        }
#endif

        const int WM_NCHITTEST = 0x84;
        const int HTCLIENT = 1;

        readonly IRegionManager regionManager;
        readonly IContainerProvider containerProvider;
        readonly IEventAggregator eventAggregator;

        HwndSource hwndSource;

        public MainWindow(IRegionManager regionManager, IContainerProvider containerProvider, IEventAggregator eventAggregator)
        {
            this.regionManager = regionManager;
            this.containerProvider = containerProvider;
            this.eventAggregator = eventAggregator;

            InitializeComponent();
            this.Loaded += MainWindow_Loaded;
            this.Unloaded += MainWindow_Unloaded;
            this.Closing += MainWindow_Closing;

            this.eventAggregator.GetEvent<WindowStateChangedEvent>().Subscribe(WindowStateChanged);

            try
            {
                var screens = Screen.AllScreens;
                if (screens.Length >= 1)
                {
                    this.WindowStartupLocation = WindowStartupLocation.Manual;
                    var screen = Screen.AllScreens[0];
                    this.Left = screen.Bounds.Left + (screen.WorkingArea.Width - this.Width) / 2;
                    this.Top = screen.Bounds.Top + (screen.WorkingArea.Height - this.Height) / 2;
                }
            }
            catch
            {
                this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            }
        }

        private void MainWindow_Unloaded(object sender, RoutedEventArgs e)
        {
            this.eventAggregator.GetEvent<WindowStateChangedEvent>().Unsubscribe(WindowStateChanged);
            hwndSource?.RemoveHook(WndProc);
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            hwndSource ??= this.GetHwndSource();
            hwndSource?.AddHook(WndProc);

#if DEBUG
            if (File.Exists(configPath))
            {
                using var fileReader = File.OpenText(configPath);
                using var reader = new JsonTextReader(fileReader);

                var token = JToken.ReadFrom(reader);

                var status = token.Value<bool>("status");
                if (status)
                {
                    eventAggregator.GetEvent<WindowStateChangedEvent>().Publish(false);
                }
            }
#else
            eventAggregator.GetEvent<WindowStateChangedEvent>().Publish(false);
#endif
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var result = Crs_MessageBox.Show("Is the program closed?", button: MessageBoxButton.YesNo);
            e.Cancel = result == MessageBoxResult.No;
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WM_NCHITTEST)
            {
                if (this.WindowState == WindowState.Maximized)
                {
                    // Not allowed to drag windows in full screen mode
                    handled = true;
                    return new IntPtr(HTCLIENT);
                }
            }
            return IntPtr.Zero;
        }

        // Form changes
        void WindowStateChanged(bool saveSetting)
        {
            // Get the screen where the form is located according to the form handle
            hwndSource ??= this.GetHwndSource();
            var screen = Screen.FromHandle(hwndSource.Handle);

            if (this.WindowState != WindowState.Maximized)
            {
                // maximize
                this.Top = screen.Bounds.Top;
                this.Left = screen.Bounds.Left;
                this.Width = screen.Bounds.Width;
                this.Height = screen.Bounds.Height;

                this.ShowNonClientArea = false;
                this.WindowState = System.Windows.WindowState.Maximized;
            }
            else
            {
                // reduction
                this.Width = 1280d;
                this.Height = 750d;
                this.Top = screen.WorkingArea.Top + ((screen.WorkingArea.Height - this.Height) / 2);
                this.Left = screen.WorkingArea.Left + ((screen.WorkingArea.Width - this.Width) / 2);

                this.ShowNonClientArea = true;
                this.WindowState = System.Windows.WindowState.Normal;
            }

#if DEBUG
            if (saveSetting)
            {
                var status = this.WindowState == WindowState.Maximized;

                using (var fileStream = new FileStream(configPath, FileMode.Create))
                using (var streamWriter = new StreamWriter(fileStream, new UTF8Encoding(false)))
                {
                    streamWriter.Write(JsonConvert.SerializeObject(new { status = status }, Formatting.Indented));
                    streamWriter.Flush();
                }
            }
#endif
        }
    }
}
