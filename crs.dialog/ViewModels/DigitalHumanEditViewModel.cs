using crs.core;
using crs.core.DbModels;
using crs.extension;
using crs.extension.Models;
using crs.theme.Extensions;
using HandyControl.Tools.Extension;
using Microsoft.EntityFrameworkCore;
using Prism.Commands;
using Prism.Events;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using ThoughtWorks.QRCode.Codec.Data;
using static crs.extension.Crs_Enum;

namespace crs.dialog.ViewModels
{
    public class DigitalHumanEditViewModel : BindableBase, IDialogResultable<DigitalHumanItem>, IDialogCommon<string>
    {
        readonly IRegionManager regionManager;
        readonly IContainerProvider containerProvider;
        readonly IEventAggregator eventAggregator;
        readonly Crs_Db2Context db;

        public DigitalHumanEditViewModel() { }
        public DigitalHumanEditViewModel(IRegionManager regionManager, IContainerProvider containerProvider, IEventAggregator eventAggregator, Crs_Db2Context db)
        {
            this.regionManager = regionManager;
            this.containerProvider = containerProvider;
            this.eventAggregator = eventAggregator;
            this.db = db;
        }

        #region Property
        private Bitmap qrCodeImage;
        public Bitmap QrCodeImage
        {
            get { return qrCodeImage; }
            set { SetProperty(ref qrCodeImage, value); }
        }
        #endregion

        private DelegateCommand cancelCommand;
        public DelegateCommand CancelCommand =>
            cancelCommand ?? (cancelCommand = new DelegateCommand(ExecuteCancelCommand));

        void ExecuteCancelCommand()
        {
            Result = null;
            QrCodeImage?.Dispose();
            CloseAction?.Invoke();
        }

        public async void Execute(string url)
        {
            var (status, msg, bitmap) = await Crs_DialogEx.ProgressShow().GetProgressResultAsync<(bool, string, Bitmap)>(async exception =>
            {
                exception.Exception = async ex =>
                {
                    exception.Message = "新增数字人信息错误";
                    return (false, $"{exception.Message},{ex.Message}", null);
                };

                var bitmap = Crs_QrCodeToolkit.Encode(url);
                return (true, null, bitmap);
            });

            if (!status)
            {
                await Crs_DialogEx.MessageBoxShow().GetMessageBoxResultAsync(msg);
                return;
            }

            QrCodeImage = bitmap;
        }

        public DigitalHumanItem Result { get; set; }
        public Action CloseAction { get; set; }
    }
}
