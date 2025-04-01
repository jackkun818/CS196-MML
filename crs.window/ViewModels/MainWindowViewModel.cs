using crs.extension;
using Prism.Commands;
using Prism.Events;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Regions;
using System.Reflection.Metadata;

namespace crs.window.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        readonly IRegionManager regionManager;
        readonly IContainerProvider containerProvider;
        readonly IEventAggregator eventAggregator;

        public MainWindowViewModel(IRegionManager regionManager, IContainerProvider containerProvider, IEventAggregator eventAggregator)
        {
            this.regionManager = regionManager;
            this.containerProvider = containerProvider;
            this.eventAggregator = eventAggregator;
        }

        private DelegateCommand loadedCommand;
        public DelegateCommand LoadedCommand =>
            loadedCommand ?? (loadedCommand = new DelegateCommand(ExecuteLoadedCommand));

        void ExecuteLoadedCommand()
        {
            // Jump to login page
            regionManager.RequestNavigate(Crs_Region.SubWindow, Crs_View.SubNull);
        }
    }
}
