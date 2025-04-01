using HandyControl.Data;
using System.Windows;
using crs.extension;
using System.Windows.Forms;
using System.Linq;
using HandyControl.Tools;
using System.Windows.Interop;
using Prism.Events;
using Prism.Ioc;
using Prism.Regions;
using static crs.extension.Crs_EventAggregator;
using System;

namespace crs.window.Views
{
    /// <summary>
    /// Interaction logic for SubWindow.xaml
    /// </summary>
    public partial class SubWindow
    {
        const int WM_NCHITTEST = 0x84;
        const int HTCLIENT = 1;

        readonly IRegionManager regionManager;
        readonly IContainerProvider containerProvider;
        readonly IEventAggregator eventAggregator;

        HwndSource hwndSource;

        public SubWindow(IRegionManager regionManager, IContainerProvider containerProvider, IEventAggregator eventAggregator)
        {
            this.regionManager = regionManager;
            this.containerProvider = containerProvider;
            this.eventAggregator = eventAggregator;

            InitializeComponent();
            this.Loaded += SubWindow_Loaded;
            this.Unloaded += SubWindow_Unloaded;
            this.Closing += SubWindow_Closing;

            this.eventAggregator.GetEvent<WindowStateChangedEvent>().Subscribe(WindowStateChanged);

            try
            {
                var screens = Screen.AllScreens;
                if (screens.Length >= 2)
                {
                    this.WindowStartupLocation = WindowStartupLocation.Manual;
                    var screen = Screen.AllScreens[1];
                    this.Left = screen.Bounds.Left + (screen.WorkingArea.Width - this.Width) / 2;
                    this.Top = screen.Bounds.Top + (screen.WorkingArea.Height - this.Height) / 2;
                }
            }
            catch
            {
                this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            }
        }

        private void SubWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
        }

        private void SubWindow_Loaded(object sender, RoutedEventArgs e)
        {
            hwndSource ??= this.GetHwndSource();
            hwndSource?.AddHook(WndProc);
        }

        private void SubWindow_Unloaded(object sender, RoutedEventArgs e)
        {
            this.eventAggregator.GetEvent<WindowStateChangedEvent>().Unsubscribe(WindowStateChanged);
            hwndSource?.RemoveHook(WndProc);
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
        }
    }
}
