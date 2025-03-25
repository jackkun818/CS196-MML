using crs.extension;
using Prism.Commands;
using Prism.Events;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Regions;
using System.Reflection.Metadata;

namespace crs.window.ViewModels
{
    public class SubWindowViewModel : BindableBase
    {
        readonly IRegionManager regionManager;
        readonly IContainerProvider containerProvider;
        readonly IEventAggregator eventAggregator;

        public SubWindowViewModel(IRegionManager regionManager, IContainerProvider containerProvider, IEventAggregator eventAggregator)
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
            regionManager.RequestNavigate(Crs_Region.MainWindow, Crs_View.Login);
        }
    }
}
