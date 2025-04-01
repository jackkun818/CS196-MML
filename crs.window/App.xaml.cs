using CloneExtensions;
using crs.core;
using crs.core.DbModels;
using crs.dialog;
using crs.extension;
using crs.theme.Extensions;
using crs.window.ViewModels;
using crs.window.Views;
using Flurl.Util;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace crs.window
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        Window mainWindow;
        Window subWindow;

        public App()
        {
            this.Startup += App_Startup;
            this.Exit += App_Exit;

            // UIThe thread does not catch exception handling event
            this.DispatcherUnhandledException += App_DispatcherUnhandledException;
            // NoUIThe thread does not catch exception handling event
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            // TaskException handling events are not caught in the thread
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
        }

        private void App_Startup(object sender, StartupEventArgs e)
        {

        }

        private void App_Exit(object sender, ExitEventArgs e)
        {
            Process.GetCurrentProcess().Kill();
        }

        private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            e.Handled = true;
            Crs_LogHelper.Error("DispatcherUnhandledException", e.Exception);
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Crs_LogHelper.Error("UnhandledException", e.ExceptionObject as Exception);
        }

        private void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            e.SetObserved();
            Crs_LogHelper.Error("UnobservedTaskException", e.Exception);
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            LiveCharts.Configure(config => config.HasGlobalSKTypeface(SKFontManager.Default.MatchCharacter('Chinese')));
        }

        protected override Window CreateShell()
        {
            // BindDialogLog delegation
            DialogEx.WriteDialogLog = Crs_LogHelper.Error;
            Crs_DialogEx.ContainerProvider = this.Container;

            subWindow = Container.Resolve<SubWindow>();
            RegionManager.SetRegionManager(subWindow, Container.Resolve<IRegionManager>());
            RegionManager.UpdateRegions();

            mainWindow = Container.Resolve<MainWindow>();
            return mainWindow;
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();
            subWindow?.Show();
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            // registerDb2ContextAs a single case
            containerRegistry.RegisterInstance(new Crs_Db2Context());

            // Register a view
            containerRegistry.RegisterForNavigation<Login>(Crs_View.Login); // Log in
            containerRegistry.RegisterForNavigation<Check>(Crs_View.Check); // Test
            containerRegistry.RegisterForNavigation<Menu>(Crs_View.Menu); // Main Menu
            containerRegistry.RegisterForNavigation<UserManagement>(Crs_View.UserManagement); // User Management           
            containerRegistry.RegisterForNavigation<EvaluateTestEx>(Crs_View.EvaluateTest); // Evaluation test
            containerRegistry.RegisterForNavigation<Train>(Crs_View.Train); // Rehabilitation training
            containerRegistry.RegisterForNavigation<ScheduleEx>(Crs_View.Schedule); // Schedule inquiry
            containerRegistry.RegisterForNavigation<Report>(Crs_View.Report); // Data Report
            containerRegistry.RegisterForNavigation<DigitalHuman>(Crs_View.DigitalHuman); // Digital Man Management
            containerRegistry.RegisterForNavigation<Null>(Crs_View.Null); // Blank page
            containerRegistry.RegisterForNavigation<SubNull>(Crs_View.SubNull); // Blank page

        }

        protected override void ConfigureModuleCatalog(IModuleCatalog moduleCatalog)
        {
            base.ConfigureModuleCatalog(moduleCatalog);

            // Loading module
            moduleCatalog.AddModule<dialogModule>(); // Pop-up window
        }
    }
}
