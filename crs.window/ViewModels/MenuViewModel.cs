using crs.core;
using crs.core.DbModels;
using crs.extension;
using crs.extension.Models;
using crs.theme.Extensions;
using HandyControl.Tools.Extension;
using Microsoft.Identity.Client.NativeInterop;
using Microsoft.VisualBasic.ApplicationServices;
using Prism.Commands;
using Prism.Events;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;
using static crs.extension.Crs_Enum;
using static crs.extension.Crs_EventAggregator;
using static ImTools.ImMap;
using User = crs.core.DbModels.User;

namespace crs.window.ViewModels
{
    public class MenuViewModel : Crs_BindableBase
    {
        readonly IRegionManager regionManager;
        readonly IContainerProvider containerProvider;
        readonly IEventAggregator eventAggregator;
        readonly Crs_Db2Context db;

        bool init = false;

        public MenuViewModel() { }
        public MenuViewModel(IRegionManager regionManager, IContainerProvider containerProvider, IEventAggregator eventAggregator, Crs_Db2Context db)
        {
            this.regionManager = regionManager;
            this.containerProvider = containerProvider;
            this.eventAggregator = eventAggregator;
            this.db = db;
        }

        #region Property

        private ObservableCollection<KeyValueItem<string, MenuType>> menuItems;
        public ObservableCollection<KeyValueItem<string, MenuType>> MenuItems
        {
            get { return menuItems; }
            set { SetProperty(ref menuItems, value); }
        }

        private KeyValueItem<string, MenuType> menuSelectedItem;
        public KeyValueItem<string, MenuType> MenuSelectedItem
        {
            get { return menuSelectedItem; }
            set { SetProperty(ref menuSelectedItem, value); }
        }

        // Current doctor
        private DoctorItem doctorItem;
        public DoctorItem DoctorItem
        {
            get { return doctorItem; }
            set { SetProperty(ref doctorItem, value); }
        }

        // Current patient
        private PatientItem patienttem;
        public PatientItem PatientItem
        {
            get { return patienttem; }
            set { SetProperty(ref patienttem, value); }
        }

        #endregion

        private DelegateCommand menuSelectedItemChangedCommand;
        public DelegateCommand MenuSelectedItemChangedCommand =>
            menuSelectedItemChangedCommand ?? (menuSelectedItemChangedCommand = new DelegateCommand(ExecuteMenuSelectedItemChangedCommand));

        void ExecuteMenuSelectedItemChangedCommand()
        {
            var menuSelectedItem = MenuSelectedItem;
            if (menuSelectedItem == null || !init)
            {
                return;
            }

            var doctor = DoctorItem?.Data;
            var patient = PatientItem?.Data;

            var parameters = new NavigationParameters
            {
                {"crs_doctor",doctor },
                {"crs_patient",patient }
            };

            regionManager.RequestNavigate(Crs_Region.Menu, menuSelectedItem.Value.ToString(), navigationParameters: parameters);
            return;

            //switch (menuSelectedItem.Value)
            //{
            //    case MenuType.UserManagement:
            //        regionManager.RequestNavigate(Crs_Region.Menu, Crs_View.UserManagement, navigationParameters: parameters);
            //        return;
            //    case MenuType.EvaluateTest:
            //        regionManager.RequestNavigate(Crs_Region.Menu, Crs_View.EvaluateTest, navigationParameters: parameters);
            //        return;
            //    case MenuType.Train:
            //        regionManager.RequestNavigate(Crs_Region.Menu, Crs_View.Train, navigationParameters: parameters);
            //        return;
            //    case MenuType.Schedule:
            //        regionManager.RequestNavigate(Crs_Region.Menu, Crs_View.Schedule, navigationParameters: parameters);
            //        return;
            //    case MenuType.Report:
            //        regionManager.RequestNavigate(Crs_Region.Menu, Crs_View.Report, navigationParameters: parameters);
            //        return;
            //    case MenuType.DigitalHuman:
            //        regionManager.RequestNavigate(Crs_Region.Menu, Crs_View.DigitalHuman, navigationParameters: parameters);
            //        return;
            //}
            //regionManager.RequestNavigate(Crs_Region.Menu, Crs_View.Null);
        }

        private DelegateCommand settingPanelCommand;
        public DelegateCommand SettingPanelCommand =>
            settingPanelCommand ?? (settingPanelCommand = new DelegateCommand(ExecuteSettingPanelCommand));

        async void ExecuteSettingPanelCommand()
        {
            await Crs_DialogEx.Show(Crs_Dialog.SettingPanel, Crs_DialogToken.TopContent)
                .UseConfig_ContentStretch()
                .Initialize<IDialogCommon<User>>(vm => vm.Execute(DoctorItem?.Data))
                .GetResultAsync<object>();
        }

        public override void OnNavigatedTo(NavigationContext navigationContext)
        {
            var parameters = navigationContext.Parameters;
            var user = parameters.GetValue<User>("crs_user");
            if (user != null)
            {
                DoctorItem = new DoctorItem().Update(user);
            }

            // Create feature options
            var menuItems = Enum.GetValues(typeof(MenuType)).Cast<MenuType>().Select(m =>
            {
                var attributes = m.GetType().GetField(m.ToString()).GetCustomAttributes(typeof(DescriptionAttribute), false);
                var attribute = attributes.FirstOrDefault() as DescriptionAttribute;
                return new KeyValueItem<string, MenuType>(attribute.Description, m);
            });

            MenuItems = new ObservableCollection<KeyValueItem<string, MenuType>>(menuItems);
            MenuSelectedItem = MenuItems.FirstOrDefault();

            init = true;
            // Trigger query first feature option
            MenuSelectedItemChangedCommand?.Execute();

            // Subscribe to events, current patient(Patient)Triggered when a change occurs
            eventAggregator.GetEvent<PatientSelectedChangedEvent>().Subscribe(PatientSelectedChangedEvent);

            // Subscribe to events, triggered when menu options change
            eventAggregator.GetEvent<MenuSelectedChangedEvent>().Subscribe(MenuSelectedChangedEvent);
        }

        public override void OnNavigatedFrom(NavigationContext navigationContext)
        {
            // Unsubscribe Events
            eventAggregator.GetEvent<PatientSelectedChangedEvent>().Unsubscribe(PatientSelectedChangedEvent);
            eventAggregator.GetEvent<MenuSelectedChangedEvent>().Unsubscribe(MenuSelectedChangedEvent);
        }

        // Current patient(Patient)Change events
        void PatientSelectedChangedEvent(PatientItem patientItem)
        {
            PatientItem = patientItem;
        }

        // Current patient(Patient)Change events
        void MenuSelectedChangedEvent((MenuType, bool) menuItem)
        {
            var menuType = menuItem.Item1;
            var isTrigger = menuItem.Item2;

            if (isTrigger)
            {
                MenuSelectedItem = MenuItems.FirstOrDefault(m => m.Value == menuType);
                return;
            }

            try
            {
                init = false;
                MenuSelectedItem = MenuItems.FirstOrDefault(m => m.Value == menuType);
                return;
            }
            finally
            {
                init = true;
            }
        }
    }
}
