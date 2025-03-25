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
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media.Imaging;
using static crs.extension.Crs_Enum;

namespace crs.dialog.ViewModels
{
    public class DigitalHumanSelectedViewModel : BindableBase, IDialogResultable<DigitalHumanItem>, IDialogCommon
    {
        readonly IRegionManager regionManager;
        readonly IContainerProvider containerProvider;
        readonly IEventAggregator eventAggregator;
        readonly Crs_Db2Context db;

        public DigitalHumanSelectedViewModel() { }
        public DigitalHumanSelectedViewModel(IRegionManager regionManager, IContainerProvider containerProvider, IEventAggregator eventAggregator, Crs_Db2Context db)
        {
            this.regionManager = regionManager;
            this.containerProvider = containerProvider;
            this.eventAggregator = eventAggregator;
            this.db = db;
        }

        #region Property
        private ObservableCollection<DigitalHumanItem> digitalHumanItems;
        public ObservableCollection<DigitalHumanItem> DigitalHumanItems
        {
            get { return digitalHumanItems; }
            set { SetProperty(ref digitalHumanItems, value); }
        }

        private DigitalHumanItem digitalHumanSelectedItem;
        public DigitalHumanItem DigitalHumanSelectedItem
        {
            get { return digitalHumanSelectedItem; }
            set { SetProperty(ref digitalHumanSelectedItem, value); }
        }

        private bool confirmCommandCanExecute = false;
        public bool ConfirmCommandCanExecute
        {
            get { return confirmCommandCanExecute; }
            set { SetProperty(ref confirmCommandCanExecute, value); }
        }
        #endregion

        private DelegateCommand digitalHumanSelectedChangedCommand;
        public DelegateCommand DigitalHumanSelectedChangedCommand =>
            digitalHumanSelectedChangedCommand ?? (digitalHumanSelectedChangedCommand = new DelegateCommand(ExecuteDigitalHumanSelectedChangedCommand));

        void ExecuteDigitalHumanSelectedChangedCommand()
        {
            ConfirmCommandCanExecute = DigitalHumanSelectedItem != null;
        }

        private DelegateCommand confirmCommand;
        public DelegateCommand ConfirmCommand =>
            confirmCommand ?? (confirmCommand = new DelegateCommand(ExecuteConfirmCommand).ObservesCanExecute(() => ConfirmCommandCanExecute));

        void ExecuteConfirmCommand()
        {
            Result = DigitalHumanSelectedItem;
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

        public async void Execute()
        {
            var (status, msg, megaHumans) = await Crs_DialogEx.ProgressShow().GetProgressResultAsync<(bool, string, List<(MegaHuman megaHuman, Bitmap bitmap)>)>(async exception =>
            {
                exception.Exception = async ex =>
                {
                    exception.Message = "获取数字人信息错误";
                    return (false, $"{exception.Message},{ex.Message}", null);
                };

                var megaHumans = await db.MegaHumans.AsNoTracking().ToListAsync();

                Bitmap bitmap = null;

                var _megaHumans = megaHumans.Select(m => (m, GetBitmapFromResource($"crs_{m.Name}"))).ToList();

                return (true, null, _megaHumans);
            });

            if (!status)
            {
                await Crs_DialogEx.MessageBoxShow().GetMessageBoxResultAsync(msg);
                return;
            }

            DigitalHumanItems = new ObservableCollection<DigitalHumanItem>(megaHumans.Select(m => new DigitalHumanItem().Update(m.megaHuman, m.bitmap)));
            DigitalHumanSelectedItem = DigitalHumanItems.FirstOrDefault();
            DigitalHumanSelectedChangedCommand?.Execute();
        }

        public Bitmap GetBitmapFromResource(string resourceKey)
        {
            // 获取WPF资源
            object resource = Application.Current.FindResource(resourceKey);

            if (resource is BitmapImage bitmapImage)
            {
                // 将BitmapImage转换为System.Drawing.Bitmap
                PngBitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(bitmapImage));

                using (MemoryStream stream = new MemoryStream())
                {
                    encoder.Save(stream);
                    stream.Position = 0;

                    return new Bitmap(stream);
                }
            }
            else
            {
                throw new InvalidOperationException($"Resource with key '{resourceKey}' is not a BitmapImage.");
            }
        }

        public DigitalHumanItem Result { get; set; }
        public Action CloseAction { get; set; }
    }
}
