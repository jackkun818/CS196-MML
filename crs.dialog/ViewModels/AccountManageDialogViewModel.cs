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
using System.Xml.Linq;
using System.Text;
using System.Security.Cryptography;
using static Azure.Core.HttpHeader;

namespace crs.dialog.ViewModels
{
    public class AccountManageDialogViewModel : BindableBase, IDialogResultable<object>, IDialogCommon<User>
    {
        readonly Crs_Db2Context db;

        User doctor;
        public AccountManageDialogViewModel() { }
        public AccountManageDialogViewModel(Crs_Db2Context db)
        {
            this.db = db;
        }

        #region Property
        private string errorMessage;
        public string ErrorMessage
        {
            get { return errorMessage; }
            set { SetProperty(ref errorMessage, value); }
        }
        private string account;
        public string Account
        {
            get { return account; }
            set { SetProperty(ref account, value); }
        }

        private string passwordHash;
        public string PasswordHash
        {
            get { return passwordHash; }
            set { SetProperty(ref passwordHash, value); }
        }

        private string oldPassword;
        public string OldPassword
        {
            get { return oldPassword; }
            set { SetProperty(ref oldPassword, value); }
        }

        private string newPassword1;
        public string NewPassword1
        {
            get { return newPassword1; }
            set { SetProperty(ref newPassword1, value); }
        }

        private string newPassword2;
        public string NewPassword2
        {
            get { return newPassword2; }
            set { SetProperty(ref newPassword2, value); }
        }

        #endregion


        public async void Execute(User _doctor)
        {
            doctor = _doctor;
            Account = doctor.PhoneNumber;
            PasswordHash = doctor.PasswordHash;
        }

        private DelegateCommand confirmCommand;
        public DelegateCommand ConfirmCommand =>
            confirmCommand ?? (confirmCommand = new DelegateCommand(ExecuteConfirmCommand));
        async void ExecuteConfirmCommand()
        {
            if (OldPassword.Equals(""))
            {
                ErrorMessage = "Please enter your login password";
                return;
            }
            if (NewPassword1.Equals(""))
            {
                ErrorMessage = "Please enter a new password";
                return;
            }
            if (NewPassword2.Equals(""))
            {
                ErrorMessage = "Please enter the new password again";
                return;
            }
            if (NewPassword1.All(char.IsDigit) == false || !(NewPassword1.Length >= 8 && NewPassword1.Length <= 16))
            {
                ErrorMessage = "The new password can only be 8-16-digit number";
                return;
            }

            var oldPasswordHash = GetPasswordHash(OldPassword);
            if (oldPasswordHash.Equals(PasswordHash) == false)
            {
                ErrorMessage = "Login password is incorrect";
                return;
            }
            if (NewPassword1.Equals(NewPassword2) == false)
            {
                ErrorMessage = "The new password entered twice is different";
                return;
            }
            var newPassword1Hash = GetPasswordHash(NewPassword1);
            var newPassword2Hash = GetPasswordHash(NewPassword2);

            var (status, msg) = await Crs_DialogEx.ProgressShow().GetProgressResultAsync<(bool, string)>(async exception =>
            {
                exception.Exception = async ex =>
                {
                    exception.Message = "Incorrect password modification";
                    return (false, $"{exception.Message},{ex.Message}");
                };

                var user = doctor;
                if (user != null)
                {
                    user = db.Users.AsNoTracking().FirstOrDefault(m => m.PhoneNumber == doctor.PhoneNumber);
                    user.PasswordHash = GetPasswordHash(NewPassword1);
                    db.Users.Update(user);
                    db.SaveChanges();

                    return (true, "Change the account password successfully");
                }
                else
                {
                    return (true, "Change the account password successfully");
                }
            });
            await Crs_DialogEx.MessageBoxShow().GetMessageBoxResultAsync(msg);
        }

        private DelegateCommand cancelCommand;
        public DelegateCommand CancelCommand =>
            cancelCommand ?? (cancelCommand = new DelegateCommand(ExecuteCancelCommand));
        void ExecuteCancelCommand()
        {
            CloseAction?.Invoke();
        }



        static string GetPasswordHash(string password)
        {
            using var sha256Hash = SHA256.Create();

            // Convert input string to byte array and calculate hash
            var bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(password));

            // Convert a byte array to a hexadecimal string
            var builder = new StringBuilder();
            for (int i = 0; i < bytes.Length; i++)
            {
                builder.Append(bytes[i].ToString("x2"));
            }
            return builder.ToString();
        }


        public object Result { get; set; }
        public Action CloseAction { get; set; }
    }
}
