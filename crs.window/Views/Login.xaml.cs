using Prism.Events;
using Prism.Ioc;
using Prism.Regions;
using System.Windows.Controls;
using static crs.extension.Crs_EventAggregator;

namespace crs.window.Views
{
    /// <summary>
    /// Interaction logic for Login
    /// </summary>
    public partial class Login : UserControl
    {
        readonly IRegionManager regionManager;
        readonly IContainerProvider containerProvider;
        readonly IEventAggregator eventAggregator;

        public Login(IRegionManager regionManager, IContainerProvider containerProvider, IEventAggregator eventAggregator)
        {
            this.regionManager = regionManager;
            this.containerProvider = containerProvider;
            this.eventAggregator = eventAggregator;

            InitializeComponent();
        }

        private void Button_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            eventAggregator.GetEvent<WindowStateChangedEvent>().Publish(true);
        }
    }
}
