using crs.core;
using crs.extension;
using crs.theme.Extensions;
using HandyControl.Tools.Extension;
using Prism.Commands;
using Prism.Events;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using crs.core.DbModels;
using crs.extension.Models;
using Microsoft.EntityFrameworkCore;
using LiveChartsCore.Kernel.Sketches;
using static SkiaSharp.HarfBuzz.SKShaper;
using Result = crs.core.DbModels.Result;
using System.Collections.ObjectModel;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore;
using SkiaSharp;
using System.Windows.Media;
using LiveChartsCore.Defaults;
using System.Numerics;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.IO;
using System.Windows.Xps.Packaging;
using System.Windows.Xps;
using System.Windows;
using System.Windows.Interop;

namespace crs.dialog.ViewModels
{
    public class SettingPanelViewModel : BindableBase, IDialogResultable<object>, IDialogCommon<User>
    {

        readonly IRegionManager regionManager;
        readonly IContainerProvider containerProvider;
        readonly IEventAggregator eventAggregator;
        readonly Crs_Db2Context db;

        User doctor;
        public SettingPanelViewModel() { }

        public SettingPanelViewModel(IRegionManager regionManager, IContainerProvider containerProvider, IEventAggregator eventAggregator, Crs_Db2Context db)
        {
            this.regionManager = regionManager;
            this.containerProvider = containerProvider;
            this.eventAggregator = eventAggregator;
            this.db = db;
        }


        #region Property
        //private bool _canClose;
        //public bool CanClose
        //{
        //    get { return _canClose; }
        //    set
        //    {
        //        _canClose = value;
        //        OnPropertyChanged(nameof(CanClose));
        //    }
        //}

        #endregion




        public async void Execute(User _doctor)
        {
            doctor = _doctor;


        }

        private DelegateCommand cancelCommand;
        public DelegateCommand CancelCommand =>
            cancelCommand ?? (cancelCommand = new DelegateCommand(ExecuteCancelCommand));

        void ExecuteCancelCommand()
        {
            CloseAction?.Invoke();
        }

        private DelegateCommand aboutUsDialogCommand;
        public DelegateCommand AboutUsDialogCommand =>
            aboutUsDialogCommand ?? (aboutUsDialogCommand = new DelegateCommand(ExecuteAboutUsDialogCommand));

        async void ExecuteAboutUsDialogCommand()
        {
            var item = await Crs_DialogEx.Show(Crs_Dialog.AboutUsDialog, Crs_DialogToken.TopMessageBox)
                .Initialize<IDialogCommon>(vm => vm.Execute())
                .GetResultAsync<object>();
        }

        private DelegateCommand accountManageDialogCommand;
        public DelegateCommand AccountManageDialogCommand =>
            accountManageDialogCommand ?? (accountManageDialogCommand = new DelegateCommand(ExecuteAccountManageDialogCommand));

        async void ExecuteAccountManageDialogCommand()
        {
            var item = await Crs_DialogEx.Show(Crs_Dialog.AccountManageDialog, Crs_DialogToken.TopMessageBox)
                .Initialize<IDialogCommon<User>>(vm => vm.Execute(doctor))
                .GetResultAsync<object>();
        }

        private DelegateCommand reportSettingDialogCommand;
        public DelegateCommand ReportSettingDialogCommand =>
            reportSettingDialogCommand ?? (reportSettingDialogCommand = new DelegateCommand(ExecuteReportSettingDialogCommand));

        async void ExecuteReportSettingDialogCommand()
        {
            var item = await Crs_DialogEx.Show(Crs_Dialog.ReportSettingDialog, Crs_DialogToken.TopContent)
                .UseConfig_ContentStretch()
                .Initialize<IDialogCommon<User>>(vm => vm.Execute(doctor))
                .GetResultAsync<object>();
        }





        public object Result { get; set; }
        public Action CloseAction { get; set; }

    }
}
