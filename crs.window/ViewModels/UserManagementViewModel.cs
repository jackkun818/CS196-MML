using crs.core.DbModels;
using crs.extension.Models;
using Prism.Commands;
using Prism.Events;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using static crs.extension.Crs_Enum;
using System.Windows.Controls;
using crs.extension;
using crs.theme.Extensions;
using Microsoft.EntityFrameworkCore;
using static crs.extension.Crs_EventAggregator;
using System.Windows.Interop;
using System.Threading.Tasks;
using HandyControl.Tools.Extension;
using crs.core;

namespace crs.window.ViewModels
{
    public class UserManagementViewModel : Crs_BindableBase
    {
        readonly IRegionManager regionManager;
        readonly IContainerProvider containerProvider;
        readonly IEventAggregator eventAggregator;
        readonly Crs_Db2Context db;

        User doctor;
        OrganizationPatient patient;

        public UserManagementViewModel() { }
        public UserManagementViewModel(IRegionManager regionManager, IContainerProvider containerProvider, IEventAggregator eventAggregator, Crs_Db2Context db)
        {
            this.regionManager = regionManager;
            this.containerProvider = containerProvider;
            this.eventAggregator = eventAggregator;
            this.db = db;
        }

        #region Property
        private ObservableCollection<PatientItem> patientItems;
        public ObservableCollection<PatientItem> PatientItems
        {
            get { return patientItems; }
            set { SetProperty(ref patientItems, value); }
        }

        private PatientItem patientSelectedItem;
        public PatientItem PatientSelectedItem
        {
            get { return patientSelectedItem; }
            set { SetProperty(ref patientSelectedItem, value); }
        }

        private string searchText;
        public string SearchText
        {
            get { return searchText; }
            set { SetProperty(ref searchText, value); }
        }
        #endregion

        private DelegateCommand<object> deleteCommand;
        public DelegateCommand<object> DeleteCommand =>
            deleteCommand ?? (deleteCommand = new DelegateCommand<object>(ExecuteDeleteCommand));

        async void ExecuteDeleteCommand(object parameter)
        {
            if (parameter is not PatientItem patientItem)
            {
                return;
            }

            if (await Crs_DialogEx.MessageBoxShow().GetMessageBoxResultAsync("Whether to delete the patient", button: MessageBoxButton.OKOrCancel) == null)
            {
                return;
            }

            var (status, msg) = await Crs_DialogEx.ProgressShow().GetProgressResultAsync<(bool, string)>(async exception =>
            {
                exception.Exception = async ex =>
                {
                    exception.Message = "Error deleting user information";
                    return (false, $"{exception.Message},{ex.Message}");
                };

                var patient = patientItem.Data;
                var item = await db.OrganizationPatients.FirstOrDefaultAsync(m => m.Id == patient.Id);
                if (item != null)
                {
                    item.SoftDeleted = true;
                    await db.SaveChangesAsync();
                }
                return (true, "Delete user information successfully");
            });

            await Crs_DialogEx.MessageBoxShow().GetMessageBoxResultAsync(msg);
            if (!status)
            {
                return;
            }

            await ExecuteSearchCommand();
        }

        private DelegateCommand<object> editCommand;
        public DelegateCommand<object> EditCommand =>
            editCommand ?? (editCommand = new DelegateCommand<object>(ExecuteEditCommand));

        async void ExecuteEditCommand(object parameter)
        {
            var patientItem = parameter as PatientItem;

            var item = await Crs_DialogEx.Show(Crs_Dialog.PatientEdit, Crs_DialogToken.TopContent)
                .UseConfig_ContentStretch()
                .Initialize<IDialogCommon<PatientItem, User>>(vm => vm.Execute(patientItem, doctor))
                .GetResultAsync<PatientItem>();

            if (item != null)
            {
                await ExecuteSearchCommand();
            }
        }

        private DelegateCommand searchCommand;
        public DelegateCommand SearchCommand =>
            searchCommand ?? (searchCommand = new DelegateCommand(async () => await ExecuteSearchCommand()));

        async Task<bool> ExecuteSearchCommand()
        {
            PatientItems = null;
            var searchText = SearchText;

            var (status, msg, patients) = await Crs_DialogEx.ProgressShow().GetProgressResultAsync<(bool, string, List<OrganizationPatient>)>(async exception =>
            {
                exception.Exception = async ex =>
                {
                    exception.Message = "Error in querying user information";
                    return (false, $"{exception.Message},{ex.Message}", null);
                };

                var queryable = db.OrganizationPatients.AsNoTracking().Where(m => m.DoctorId == doctor.Id && (m.SoftDeleted == null || m.SoftDeleted == false));
                if (!string.IsNullOrWhiteSpace(searchText))
                {
                    queryable = queryable.Where(m => m.FullName.Contains(searchText) || m.PhoneNumber.Contains(searchText));
                }

                var patients = await queryable.ToListAsync();
                return (true, null, patients);
            });

            if (!status)
            {
                await Crs_DialogEx.MessageBoxShow().GetMessageBoxResultAsync(msg);
                return false;
            }

            PatientItems = new ObservableCollection<PatientItem>(patients.Select(m => new PatientItem().Update(m)));
            PatientSelectedItem = PatientItems.FirstOrDefault(m => m.Data.Id == patient?.Id) ?? PatientItems.FirstOrDefault();
            return true;
        }

        private DelegateCommand patientSelectedItemChangedCommand;
        public DelegateCommand PatientSelectedItemChangedCommand =>
            patientSelectedItemChangedCommand ?? (patientSelectedItemChangedCommand = new DelegateCommand(ExecutePatientSelectedItemChangedCommand));

        void ExecutePatientSelectedItemChangedCommand()
        {
            // Notification to change the main menu page“Current patient”information
            var patientSelectedItem = PatientSelectedItem;
            patient = patientSelectedItem?.Data;
            eventAggregator.GetEvent<PatientSelectedChangedEvent>().Publish(patientSelectedItem);
        }

        public override async void OnNavigatedTo(NavigationContext navigationContext)
        {
            var parameters = navigationContext.Parameters;

            this.doctor = parameters.GetValue<User>("crs_doctor");
            this.patient = parameters.GetValue<OrganizationPatient>("crs_patient");

            await ExecuteSearchCommand();
        }

        public override void OnNavigatedFrom(NavigationContext navigationContext)
        {

        }
    }
}
