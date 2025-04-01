using crs.core;
using crs.core.DbModels;
using crs.extension;
using crs.extension.Models;
using crs.theme.Extensions;
using HandyControl.Tools.Extension;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client.NativeInterop;
using Newtonsoft.Json;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Interop;
using static Azure.Core.HttpHeader;

namespace crs.dialog.ViewModels
{
    public class ReportSettingDialogViewModel : BindableBase, IDialogResultable<object>, IDialogCommon<User>
    {
        readonly Crs_Db2Context db;

        readonly static string reportTitlePrefixPath = @".\configs\reportTitlePrefix.json";

        User doctor;
        public ReportSettingDialogViewModel() { }
        public ReportSettingDialogViewModel(Crs_Db2Context db)
        {
            this.db = db;
        }

        #region Property
        private string evaluateReportTitlePrefix;
        public string EvaluateReportTitlePrefix
        {
            get { return evaluateReportTitlePrefix; }
            set { SetProperty(ref evaluateReportTitlePrefix, value); }
        }

        private string trainReportTitlePrefix;
        public string TrainReportTitlePrefix
        {
            get { return trainReportTitlePrefix; }
            set { SetProperty(ref trainReportTitlePrefix, value); }
        }

        #endregion


        public async void Execute(User _doctor)
        {
            doctor = _doctor;
        }

        private DelegateCommand confirmCommand;
        public DelegateCommand ConfirmCommand =>
            confirmCommand ?? (confirmCommand = new DelegateCommand(ExecuteConfirmCommand));
        async void ExecuteConfirmCommand()
        {
            using (var fileStream = new FileStream(reportTitlePrefixPath, FileMode.Create))
            using (var streamWriter = new StreamWriter(fileStream, new UTF8Encoding(false)))
            {
                streamWriter.Write(JsonConvert.SerializeObject(new
                {
                    evaluateReportTitlePrefix = EvaluateReportTitlePrefix,
                    trainReportTitlePrefix = TrainReportTitlePrefix
                }, Formatting.Indented));
                streamWriter.Flush();
            }
            await Crs_DialogEx.MessageBoxShow().GetMessageBoxResultAsync("Save the result successfully");

            await Crs_DialogEx.Show(Crs_Dialog.SettingPanel, Crs_DialogToken.TopContent)
                .UseConfig_ContentStretch()
                .Initialize<IDialogCommon<User>>(vm => vm.Execute(doctor))
                .GetResultAsync<object>();
        }

        private DelegateCommand cancelCommand;
        public DelegateCommand CancelCommand =>
            cancelCommand ?? (cancelCommand = new DelegateCommand(ExecuteCancelCommand));
        async void ExecuteCancelCommand()
        {
            //CloseAction?.Invoke();
            await Crs_DialogEx.Show(Crs_Dialog.SettingPanel, Crs_DialogToken.TopContent)
                .UseConfig_ContentStretch()
                .Initialize<IDialogCommon<User>>(vm => vm.Execute(doctor))
                .GetResultAsync<object>();
        }
        public object Result { get; set; }
        public Action CloseAction { get; set; }
    }
}
