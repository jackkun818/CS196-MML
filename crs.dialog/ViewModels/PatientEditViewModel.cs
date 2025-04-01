using crs.core;
using crs.core.DbModels;
using crs.extension;
using crs.extension.Models;
using crs.theme.Extensions;
using HandyControl.Tools.Extension;
using Microsoft.EntityFrameworkCore;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Interop;
using static Azure.Core.HttpHeader;

namespace crs.dialog.ViewModels
{
    public class PatientEditViewModel : BindableBase, IDialogResultable<PatientItem>, IDialogCommon<PatientItem, User>
    {
        readonly Crs_Db2Context db;

        User doctor;

        public PatientEditViewModel() { }

        public PatientEditViewModel(Crs_Db2Context db)
        {
            this.db = db;
        }

        #region Property
        private PatientItem patientItem;
        public PatientItem PatientItem
        {
            get { return patientItem; }
            set { SetProperty(ref patientItem, value); }
        }

        private string name;
        public string Name
        {
            get { return name; }
            set { SetProperty(ref name, value); }
        }

        private bool sexMan;
        public bool SexMan
        {
            get { return sexMan; }
            set { SetProperty(ref sexMan, value); }
        }

        private bool sexWoman;
        public bool SexWoman
        {
            get { return sexWoman; }
            set { SetProperty(ref sexWoman, value); }
        }

        private string age;
        public string Age
        {
            get { return age; }
            set { SetProperty(ref age, value); }
        }

        private string province;
        public string Province
        {
            get { return province; }
            set { SetProperty(ref province, value); }
        }

        private string career;
        public string Career
        {
            get { return career; }
            set { SetProperty(ref career, value); }
        }

        private string phoneNumber;
        public string PhoneNumber
        {
            get { return phoneNumber; }
            set { SetProperty(ref phoneNumber, value); }
        }

        private DateTime? createOn;
        public DateTime? CreateOn
        {
            get { return createOn; }
            set { SetProperty(ref createOn, value); }
        }

        private DateTime? lastModifiedOn;
        public DateTime? LastModifiedOn
        {
            get { return lastModifiedOn; }
            set { SetProperty(ref lastModifiedOn, value); }
        }

        private string notes;
        public string Notes
        {
            get { return notes; }
            set { SetProperty(ref notes, value); }
        }
        #endregion

        private DelegateCommand confirmCommand;
        public DelegateCommand ConfirmCommand =>
            confirmCommand ?? (confirmCommand = new DelegateCommand(ExecuteConfirmCommand));

        async void ExecuteConfirmCommand()
        {
            if (string.IsNullOrWhiteSpace(Name))
            {
                await Crs_DialogEx.MessageBoxShow().GetMessageBoxResultAsync("Please enter a name");
                return;
            }

            if (!SexMan && !SexWoman)
            {
                await Crs_DialogEx.MessageBoxShow().GetMessageBoxResultAsync("Please choose gender");
                return;
            }

            if (string.IsNullOrWhiteSpace(Age))
            {
                await Crs_DialogEx.MessageBoxShow().GetMessageBoxResultAsync("Please enter your age");
                return;
            }

            if (!int.TryParse(Age, out int AgeValue))
            {
                await Crs_DialogEx.MessageBoxShow().GetMessageBoxResultAsync("Age format error");
                return;
            }

            if (!string.IsNullOrWhiteSpace(PhoneNumber) && !Int64.TryParse(PhoneNumber, out Int64 PhoneNumberValue))
            {
                await Crs_DialogEx.MessageBoxShow().GetMessageBoxResultAsync("Mobile phone number format error");
                return;
            }

            var (status, msg) = await Crs_DialogEx.ProgressShow().GetProgressResultAsync<(bool, string)>(async exception =>
            {
                exception.Exception = async ex =>
                {
                    exception.Message = "Error editing user information";
                    return (false, $"{exception.Message},{ex.Message}");
                };

                var patient = PatientItem?.Data;
                if (patient != null)
                {
                    patient = await db.OrganizationPatients.FirstOrDefaultAsync(m => m.Id == patient.Id);
                    patient.FullName = Name;
                    patient.Sex = SexMan ? 0 : SexWoman ? 1 : null;

                    patient.Province = Province;
                    patient.Career = Career;
                    patient.Age = AgeValue;
                    patient.PhoneNumber = PhoneNumber;
                    patient.CreatedOn = CreateOn;
                    patient.LastModifiedOn = LastModifiedOn;
                    patient.Notes = Notes;

                    db.OrganizationPatients.Update(patient);
                    db.SaveChanges();

                    return (true, "User information was modified successfully");
                }
                else
                {
                    patient = new OrganizationPatient();
                    patient.DoctorId = doctor?.Id;
                    patient.FullName = Name;
                    patient.Sex = SexMan ? 0 : SexWoman ? 1 : null;

                    patient.Province = Province;
                    patient.Career = Career;
                    patient.Age = AgeValue;
                    patient.PhoneNumber = PhoneNumber;
                    patient.CreatedOn = CreateOn;
                    patient.LastModifiedOn = LastModifiedOn;
                    patient.Notes = Notes;

                    db.OrganizationPatients.Add(patient);
                    db.SaveChanges();

                    return (true, "Create user information successfully");
                }
            });

            await Crs_DialogEx.MessageBoxShow().GetMessageBoxResultAsync(msg);
            if (!status)
            {
                return;
            }

            Result = new PatientItem();
            CloseAction?.Invoke();
        }

        private DelegateCommand cancelCommand;
        public DelegateCommand CancelCommand =>
            cancelCommand ?? (cancelCommand = new DelegateCommand(ExecuteCancelCommand));

        void ExecuteCancelCommand()
        {
            Result = null;
            CloseAction?.Invoke();
        }

        public void Execute(PatientItem patientItem, User doctor)
        {
            this.doctor = doctor;
            PatientItem = patientItem;

            if (patientItem != null)
            {
                var patient = patientItem.Data;
                Name = patient.FullName;

                SexMan = patient.Sex != null && patient.Sex == 0;
                SexWoman = patient.Sex != null && patient.Sex == 1;

                Province = patient.Province;
                Career = patient.Career;
                Age = patient.Age?.ToString();
                PhoneNumber = patient.PhoneNumber;
                CreateOn = patient.CreatedOn;
                LastModifiedOn = patient.LastModifiedOn;
                Notes = patient.Notes;
            }
        }

        public PatientItem Result { get; set; }
        public Action CloseAction { get; set; }
    }
}
