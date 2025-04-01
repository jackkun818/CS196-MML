using crs.core.DbModels;
using crs.extension;
using HandyControl.Data;
using Microsoft.EntityFrameworkCore;
using Prism.Commands;
using Prism.Events;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using crs.theme.Extensions;
using Microsoft.VisualBasic.ApplicationServices;
using User = crs.core.DbModels.User;
using System.Text;
using System.Security.Cryptography;
using System.Runtime.InteropServices.JavaScript;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Windows.Shapes;
using Path = System.IO.Path;
using HandyControl.Tools.Extension;
using crs.core;

namespace crs.window.ViewModels
{
    public class LoginViewModel : Crs_BindableBase
    {
        readonly static string configPath = @".\configs\connect.json";

        static LoginViewModel()
        {
            var dir = new DirectoryInfo(Path.GetDirectoryName(configPath));
            if (!dir.Exists)
            {
                dir.Create();
            }
        }

        readonly IRegionManager regionManager;
        readonly IContainerProvider containerProvider;
        readonly IEventAggregator eventAggregator;
        readonly Crs_Db2Context db;

        public LoginViewModel() { }
        public LoginViewModel(IRegionManager regionManager, IContainerProvider containerProvider, IEventAggregator eventAggregator, Crs_Db2Context db)
        {
            this.regionManager = regionManager;
            this.containerProvider = containerProvider;
            this.eventAggregator = eventAggregator;
            this.db = db;
        }

        #region Property
        private string account;
        public string Account
        {
            get { return account; }
            set { SetProperty(ref account, value); }
        }

        private string password;
        public string Password
        {
            get { return password; }
            set { SetProperty(ref password, value); }
        }

        private bool rememberPassword;
        public bool RememberPassword
        {
            get { return rememberPassword; }
            set { SetProperty(ref rememberPassword, value); }
        }

        // Control boundLoginCommandcontrol ofIsEnable
        private bool loginCommandCanExecute = true;
        public bool LoginCommandCanExecute
        {
            get { return loginCommandCanExecute; }
            set { SetProperty(ref loginCommandCanExecute, value); }
        }
        #endregion

        private DelegateCommand loginCommand;
        public DelegateCommand LoginCommand =>
            loginCommand ?? (loginCommand = new DelegateCommand(ExecuteLoginCommand).ObservesCanExecute(() => LoginCommandCanExecute));

        async void ExecuteLoginCommand()
        {
            var account = Account;
            var password = Password;

            if (string.IsNullOrWhiteSpace(account))
            {
                await Crs_DialogEx.MessageBoxShow().GetMessageBoxResultAsync("Please enter your account number");
                return;
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                await Crs_DialogEx.MessageBoxShow().GetMessageBoxResultAsync("Please enter your password");
                return;
            }

            var (status, msg, user) = await Crs_DialogEx.ProgressShow().GetProgressResultAsync<(bool, string, User)>(exception =>
            {
                exception.Exception = ex =>
                {
                    exception.Message = "Login error";
                    return (false, $"{exception.Message},{ex.Message}", null);
                };

                var user = db.Users.AsNoTracking().FirstOrDefault(m => m.PhoneNumber == account);
                if (user == null) return (false, "The user does not exist", null);

                var employee = db.Employees.AsNoTracking().FirstOrDefault(m => m.Id == user.Id);
                if (employee == null) return (false, "This user is not a doctor", null);

                var passwordHash = GetPasswordHash(password);
                if (user.PasswordHash != passwordHash) return (false, "Error password", null);

                return (true, null, user);
            });

            if (!status)
            {
                await Crs_DialogEx.MessageBoxShow().GetMessageBoxResultAsync(msg);
                return;
            }

            // Login successfully,Bring parameters,Jump to self-test page
            var parameters = new NavigationParameters
            {
                {"crs_user",user }
            };
            regionManager.RequestNavigate(Crs_Region.MainWindow, Crs_View.Check, navigationParameters: parameters);
        }

        // Entering newViewCalled on time
        public override void OnNavigatedTo(NavigationContext navigationContext)
        {
            if (File.Exists(configPath))
            {
                using var fileReader = File.OpenText(configPath);
                using var reader = new JsonTextReader(fileReader);

                var token = JToken.ReadFrom(reader);

                Account = token.Value<string>("account");
                Password = token.Value<string>("password");
                RememberPassword = token.Value<bool>("remember_password");
            }
        }

        // From the currentViewCalled when leaving
        public override void OnNavigatedFrom(NavigationContext navigationContext)
        {
            using (var fileStream = new FileStream(configPath, FileMode.Create))
            using (var streamWriter = new StreamWriter(fileStream, new UTF8Encoding(false)))
            {
                streamWriter.Write(JsonConvert.SerializeObject(new
                {
                    account = Account,
                    password = Password,
                    remember_password = RememberPassword
                }, Formatting.Indented));
                streamWriter.Flush();
            }
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
    }
}
