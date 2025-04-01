using crs.core.DbModels;
using crs.extension;
using crs.extension.Models;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using crs.theme.Extensions;
using crs.core;
using Prism.Events;
using Prism.Ioc;
using Microsoft.EntityFrameworkCore;
using System.Windows.Interop;
using System.Threading.Tasks;
using HandyControl.Tools.Extension;
using System.Drawing;
using System.IO;

namespace crs.window.ViewModels
{
    public class DigitalHumanViewModel : Crs_BindableBase
    {
        readonly IRegionManager regionManager;
        readonly IContainerProvider containerProvider;
        readonly IEventAggregator eventAggregator;
        readonly Crs_Db2Context db;

        public DigitalHumanViewModel() { }
        public DigitalHumanViewModel(IRegionManager regionManager, IContainerProvider containerProvider, IEventAggregator eventAggregator, Crs_Db2Context db)
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
        #endregion

        private DelegateCommand addCommand;
        public DelegateCommand AddCommand =>
            addCommand ?? (addCommand = new DelegateCommand(ExecuteAddCommand));

        async void ExecuteAddCommand()
        {
            var url = "https://www.baidu.com";
            var item = await Crs_DialogEx.Show(Crs_Dialog.DigitalHumanEdit, Crs_DialogToken.TopContent)
                .Initialize<IDialogCommon<string>>(vm => vm.Execute(url))
                .GetResultAsync<DigitalHumanItem>();
        }

        private DelegateCommand deleteCommand;
        public DelegateCommand DeleteCommand =>
            deleteCommand ?? (deleteCommand = new DelegateCommand(ExecuteDeleteCommand));

        async void ExecuteDeleteCommand()
        {
            var selectedItem = DigitalHumanSelectedItem;
            if (selectedItem == null)
            {
                await Crs_DialogEx.MessageBoxShow().GetMessageBoxResultAsync("Please select");
                return;
            }

            if (await Crs_DialogEx.MessageBoxShow().GetMessageBoxResultAsync("Whether to delete", button: MessageBoxButton.OKOrCancel) == null)
            {
                return;
            }

            var (status, msg) = await Crs_DialogEx.ProgressShow().GetProgressResultAsync<(bool, string)>(async exception =>
            {
                exception.Exception = async ex =>
                {
                    exception.Message = "Error deleting digital information";
                    return (false, $"{exception.Message},{ex.Message}");
                };

                var megaHuman = selectedItem.Data;
                var item = await db.MegaHumans.FirstOrDefaultAsync(m => m.MegaHumanId == megaHuman.MegaHumanId);
                if (item != null)
                {
                    db.MegaHumans.Remove(item);
                    await db.SaveChangesAsync();
                }
                return (true, null);
            });

            if (!status)
            {
                await Crs_DialogEx.MessageBoxShow().GetMessageBoxResultAsync(msg);
                return;
            }

            await ExecuteSearchCommand();
        }

        async Task<bool> ExecuteSearchCommand()
        {
            DigitalHumanItems = null;
            DigitalHumanSelectedItem = null;

            var (status, msg, megaHumans) = await Crs_DialogEx.ProgressShow().GetProgressResultAsync<(bool, string, List<(MegaHuman megaHuman, Bitmap bitmap)>)>(async exception =>
            {
                exception.Exception = async ex =>
                {
                    exception.Message = "Error in obtaining digital person information";
                    return (false, $"{exception.Message},{ex.Message}", null);
                };

                var megaHumans = await db.MegaHumans.AsNoTracking().ToListAsync();

                Bitmap bitmap = null;

                var _megaHumans = megaHumans.Select(m => (m, bitmap)).ToList();

                return (true, null, _megaHumans);
            });

            if (!status)
            {
                await Crs_DialogEx.MessageBoxShow().GetMessageBoxResultAsync(msg);
                return false;
            }

            DigitalHumanItems = new ObservableCollection<DigitalHumanItem>(megaHumans.Select(m => new DigitalHumanItem().Update(m.megaHuman, m.bitmap)));
            return true;
        }

        public override async void OnNavigatedTo(NavigationContext navigationContext)
        {
            await ExecuteSearchCommand();
        }

        public override void OnNavigatedFrom(NavigationContext navigationContext)
        {

        }
    }
}
