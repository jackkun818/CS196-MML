using crs.core.DbModels;
using crs.extension;
using Microsoft.VisualBasic.ApplicationServices;
using Prism.Commands;
using Prism.Events;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using User = crs.core.DbModels.User;

namespace crs.window.ViewModels
{
    public class CheckViewModel : Crs_BindableBase
    {
        readonly IRegionManager regionManager;
        readonly IContainerProvider containerProvider;
        readonly IEventAggregator eventAggregator;

        User user;

        public CheckViewModel() { }
        public CheckViewModel(IRegionManager regionManager, IContainerProvider containerProvider, IEventAggregator eventAggregator)
        {
            this.regionManager = regionManager;
            this.containerProvider = containerProvider;
            this.eventAggregator = eventAggregator;
        }

        #region Property
        private bool checking;
        public bool Checking
        {
            get { return checking; }
            set { SetProperty(ref checking, value); }
        }

        private bool checkCommandCanExecute = true;
        public bool CheckCommandCanExecute
        {
            get { return checkCommandCanExecute; }
            set { SetProperty(ref checkCommandCanExecute, value); }
        }
        #endregion

        private DelegateCommand checkCommand;
        public DelegateCommand CheckCommand =>
            checkCommand ?? (checkCommand = new DelegateCommand(ExecuteCheckCommand).ObservesCanExecute(() => CheckCommandCanExecute));

        async void ExecuteCheckCommand()
        {
            try
            {
                CheckCommandCanExecute = false;

                Checking = true;

                await Task.Delay(1500);

                // Self-test successful,Bring parameters,Jump to the main menu page
                var parameters = new NavigationParameters
                {
                    {"crs_user",user }
                };
                regionManager.RequestNavigate(Crs_Region.MainWindow, Crs_View.Menu, navigationParameters: parameters);
            }
            finally
            {
                Checking = false;
                CheckCommandCanExecute = true;
            }
        }

        public override void OnNavigatedTo(NavigationContext navigationContext)
        {
            var parameters = navigationContext.Parameters;
            this.user = parameters.GetValue<User>("crs_user");

            ExecuteCheckCommand();
        }

        public override void OnNavigatedFrom(NavigationContext navigationContext)
        {

        }
    }
}