using crs.extension;
using Prism.Events;
using Prism.Ioc;
using Prism.Regions;
using System.Windows;
using System.Windows.Controls;
using crs.theme.Extensions;
using static crs.extension.Crs_EventAggregator;
using MessageBoxButton = crs.theme.Extensions.MessageBoxButton;
using HandyControl.Tools.Extension;

namespace crs.window.Views
{
    /// <summary>
    /// Interaction logic for Menu
    /// </summary>
    public partial class Menu : UserControl
    {
        readonly IRegionManager regionManager;
        readonly IContainerProvider containerProvider;
        readonly IEventAggregator eventAggregator;

        public Menu(IRegionManager regionManager, IContainerProvider containerProvider, IEventAggregator eventAggregator)
        {
            this.regionManager = regionManager;
            this.containerProvider = containerProvider;
            this.eventAggregator = eventAggregator;

            InitializeComponent();

#if DEBUG
#else
            this.maxWindowButton.Visibility = Visibility.Collapsed;
#endif
        }

        private async void CloseButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (await Crs_DialogEx.MessageBoxShow().GetMessageBoxResultAsync("Is the program closed?", button: MessageBoxButton.OKOrCancel) == null)
            {
                return;
            }

            Application.Current.Shutdown();
        }

        private void MaxWindowButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            eventAggregator.GetEvent<WindowStateChangedEvent>().Publish(true);
        }

    }
}
