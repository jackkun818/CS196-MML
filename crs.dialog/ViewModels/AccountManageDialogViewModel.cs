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
                ErrorMessage = "请输入登录密码";
                return;
            }
            if (NewPassword1.Equals(""))
            {
                ErrorMessage = "请输入新密码";
                return;
            }
            if (NewPassword2.Equals(""))
            {
                ErrorMessage = "请再次输入新密码";
                return;
            }
            if (NewPassword1.All(char.IsDigit) == false || !(NewPassword1.Length >= 8 && NewPassword1.Length <= 16))
            {
                ErrorMessage = "新密码只能是8-16位的数字";
                return;
            }

            var oldPasswordHash = GetPasswordHash(OldPassword);
            if (oldPasswordHash.Equals(PasswordHash) == false)
            {
                ErrorMessage = "登录密码不正确";
                return;
            }
            if (NewPassword1.Equals(NewPassword2) == false)
            {
                ErrorMessage = "两次输入的新密码不相同";
                return;
            }
            var newPassword1Hash = GetPasswordHash(NewPassword1);
            var newPassword2Hash = GetPasswordHash(NewPassword2);

            var (status, msg) = await Crs_DialogEx.ProgressShow().GetProgressResultAsync<(bool, string)>(async exception =>
            {
                exception.Exception = async ex =>
                {
                    exception.Message = "修改账号密码错误";
                    return (false, $"{exception.Message},{ex.Message}");
                };

                var user = doctor;
                if (user != null)
                {
                    user = db.Users.AsNoTracking().FirstOrDefault(m => m.PhoneNumber == doctor.PhoneNumber);
                    user.PasswordHash = GetPasswordHash(NewPassword1);
                    db.Users.Update(user);
                    db.SaveChanges();

                    return (true, "修改账号密码成功");
                }
                else
                {
                    return (true, "修改账号密码成功");
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

            // 将输入字符串转换为字节数组并计算哈希
            var bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(password));

            // 将字节数组转换为十六进制字符串
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
