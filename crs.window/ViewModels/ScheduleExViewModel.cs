using crs.core;
using crs.core.DbModels;
using crs.extension;
using crs.extension.Models;
using Prism.Commands;
using Prism.Events;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using static crs.extension.Crs_Enum;
using crs.theme.Extensions;
using HandyControl.Controls;
using System.Globalization;
using System.Windows.Controls.Primitives;
using Microsoft.EntityFrameworkCore;
using crs.window.Views;
using System.Windows.Interop;
using static crs.extension.Crs_EventAggregator;
using HandyControl.Tools.Extension;

namespace crs.window.ViewModels
{
    public class ScheduleExViewModel : Crs_BindableBase
    {
        readonly static List<(DateTime beginTime, DateTime endTime)> timeScales = new List<(DateTime beginTime, DateTime endTime)>();

        static ScheduleExViewModel()
        {
            var beginTime = DateTime.Parse("8:30");
            var endTime = DateTime.Parse("20:30");

            while (beginTime < endTime)
            {
                var lastTime = beginTime;
                beginTime = beginTime.AddMinutes(30);

                timeScales.Add((lastTime, beginTime));
            }
        }

        readonly IRegionManager regionManager;
        readonly IContainerProvider containerProvider;
        readonly IEventAggregator eventAggregator;
        readonly Crs_Db2Context db;

        User doctor;

        bool init = false;
        int? calendarPage;

        public ScheduleExViewModel() { }

        public ScheduleExViewModel(IRegionManager regionManager, IContainerProvider containerProvider, IEventAggregator eventAggregator, Crs_Db2Context db)
        {
            this.regionManager = regionManager;
            this.containerProvider = containerProvider;
            this.eventAggregator = eventAggregator;
            this.db = db;
        }

        #region Property

        private ObservableCollection<KeyValueItem<string, ScheduleType>> scheduleTypeItems;
        public ObservableCollection<KeyValueItem<string, ScheduleType>> ScheduleTypeItems
        {
            get { return scheduleTypeItems; }
            set { SetProperty(ref scheduleTypeItems, value); }
        }

        private KeyValueItem<string, ScheduleType> scheduleTypeSelectedItem;
        public KeyValueItem<string, ScheduleType> ScheduleTypeSelectedItem
        {
            get { return scheduleTypeSelectedItem; }
            set { SetProperty(ref scheduleTypeSelectedItem, value); }
        }

        private ObservableCollection<ScheduleItem> todayScheduleItems;
        public ObservableCollection<ScheduleItem> TodayScheduleItems
        {
            get { return todayScheduleItems; }
            set { SetProperty(ref todayScheduleItems, value); }
        }

        private ObservableCollection<MultiItem<string, int>> calendarItems;
        public ObservableCollection<MultiItem<string, int>> CalendarItems
        {
            get { return calendarItems; }
            set { SetProperty(ref calendarItems, value); }
        }

        private ObservableCollection<List<ScheduleItem>> everydayScheduleItems;
        public ObservableCollection<List<ScheduleItem>> EverydayScheduleItems
        {
            get { return everydayScheduleItems; }
            set { SetProperty(ref everydayScheduleItems, value); }
        }
        #endregion

        private DelegateCommand<ScheduleItem> programSettingCommand;
        public DelegateCommand<ScheduleItem> ProgramSettingCommand =>
            programSettingCommand ?? (programSettingCommand = new DelegateCommand<ScheduleItem>(ExecuteProgramSettingCommand));

        async void ExecuteProgramSettingCommand(ScheduleItem parameter)
        {
            if (parameter == null)
            {
                return;
            }

            if (parameter.PatientItem == null)
            {
                await Crs_DialogEx.MessageBoxShow().GetMessageBoxResultAsync("患者数据异常");
                return;
            }

            if (parameter.ProgramType == null)
            {
                await Crs_DialogEx.MessageBoxShow().GetMessageBoxResultAsync("方案数据异常");
                return;
            }

            var programId = parameter.Data?.schedule?.ProgramId;
            if (programId == null)
            {
                await Crs_DialogEx.MessageBoxShow().GetMessageBoxResultAsync("方案ID为空");
                return;
            }

            var patientSelectedItem = parameter.PatientItem;
            eventAggregator.GetEvent<PatientSelectedChangedEvent>().Publish(patientSelectedItem);

            var patient = parameter.PatientItem.Data;
            var parameters = new NavigationParameters
            {
                {"crs_doctor",doctor },
                {"crs_patient",patient },
                {"crs_programId",programId }
            };

            var menuType = parameter.ProgramType switch
            {
                ProgramType.评估测试 => (MenuType?)MenuType.EvaluateTest,
                ProgramType.康复训练 => (MenuType?)MenuType.Train,
                _ => throw new NotImplementedException()
            };

            eventAggregator.GetEvent<MenuSelectedChangedEvent>().Publish((menuType.Value, false));
            regionManager.RequestNavigate(Crs_Region.Menu, menuType.Value.ToString(), navigationParameters: parameters);
        }

        private DelegateCommand<ScheduleItem> programStartCommand;
        public DelegateCommand<ScheduleItem> ProgramStartCommand =>
            programStartCommand ?? (programStartCommand = new DelegateCommand<ScheduleItem>(ExecuteProgramStartCommand));

        async void ExecuteProgramStartCommand(ScheduleItem parameter)
        {
            if (parameter == null)
            {
                return;
            }

            if (parameter.PatientItem == null)
            {
                await Crs_DialogEx.MessageBoxShow().GetMessageBoxResultAsync("患者数据异常");
                return;
            }

            if (parameter.ProgramType == null)
            {
                await Crs_DialogEx.MessageBoxShow().GetMessageBoxResultAsync("方案数据异常");
                return;
            }

            if (parameter.ProgramContent == null)
            {
                await Crs_DialogEx.MessageBoxShow().GetMessageBoxResultAsync("未设置方案");
                return;
            }

            var programId = parameter.Data?.schedule?.ProgramId;
            if (programId == null)
            {
                await Crs_DialogEx.MessageBoxShow().GetMessageBoxResultAsync("方案ID为空");
                return;
            }

            if (parameter.ProgramContent == EvaluateTestMode.标准评估.ToString())
            {
                await Crs_DialogEx.Show(Crs_Dialog.EvaluateStandardPanel, Crs_DialogToken.TopContent)
                    .UseConfig_ContentStretch()
                    .Initialize<IDialogCommon<int?>>(vm => vm.Execute(programId))
                    .GetResultAsync<object>();
                return;
            }

            var selectedItem = await Crs_DialogEx.Show(Crs_Dialog.DigitalHumanSelected)
                .Initialize<IDialogCommon>(vm => vm.Execute()).GetResultAsync<DigitalHumanItem>();

            if (selectedItem == null)
            {
                return;
            }

            var viewName = parameter.ProgramType switch
            {
                ProgramType.评估测试 => Crs_Dialog.EvaluateGamePanel,
                ProgramType.康复训练 => Crs_Dialog.TrainGamePanel,
                _ => throw new NotImplementedException()
            };

            await Crs_DialogEx.Show(viewName, Crs_DialogToken.TopContent)
                .UseConfig_ContentStretch()
                .Initialize<IDialogCommon<int, DigitalHumanItem>>(vm => vm.Execute((int)programId, selectedItem))
                .GetResultAsync<object>();
        }

        private DelegateCommand<ScheduleItem> scheduleStatusSelectedItemChangedCommand;
        public DelegateCommand<ScheduleItem> ScheduleStatusSelectedItemChangedCommand =>
            scheduleStatusSelectedItemChangedCommand ?? (scheduleStatusSelectedItemChangedCommand = new DelegateCommand<ScheduleItem>(ExecuteScheduleStatusSelectedItemChangedCommand));

        async void ExecuteScheduleStatusSelectedItemChangedCommand(ScheduleItem parameter)
        {
            if (parameter == null || parameter.StatusSelectedItem == null || !init)
            {
                return;
            }

            var (status, msg) = await Crs_DialogEx.ProgressShow().GetProgressResultAsync<(bool, string)>(async exception =>
            {
                exception.Exception = async ex =>
                {
                    exception.Message = "修改今日排班状态错误";
                    return (false, $"{exception.Message},{ex.Message}");
                };

                var scheduleId = parameter.Data.Value.schedule.ScheduleId;
                var schedule = await db.Schedules.FirstOrDefaultAsync(m => m.ScheduleId == scheduleId);
                if (schedule == null)
                {
                    return (false, "修改今日排班状态失败,查找不到记录");
                }

                schedule.Status = parameter.StatusSelectedItem.Item1.ToString();
                db.Schedules.Update(schedule);
                db.SaveChanges();

                return (true, null);
            });

            if (!status)
            {
                await Crs_DialogEx.MessageBoxShow().GetMessageBoxResultAsync(msg);
                return;
            }
        }

        private DelegateCommand lastCalendarPageCommand;
        public DelegateCommand LastCalendarPageCommand =>
            lastCalendarPageCommand ?? (lastCalendarPageCommand = new DelegateCommand(ExecuteLastCalendarPageCommand));

        void ExecuteLastCalendarPageCommand()
        {
            calendarPage ??= 0;
            calendarPage--;
            ExecuteScheduleTypeSelectedItemChangedCommand();
        }

        private DelegateCommand nextCalendarPageCommand;
        public DelegateCommand NextCalendarPageCommand =>
            nextCalendarPageCommand ?? (nextCalendarPageCommand = new DelegateCommand(ExecuteNextCalendarPageCommand));

        void ExecuteNextCalendarPageCommand()
        {
            calendarPage ??= 0;
            calendarPage++;
            ExecuteScheduleTypeSelectedItemChangedCommand();
        }

        private DelegateCommand scheduleTypeSelectedItemChangedCommand;
        public DelegateCommand ScheduleTypeSelectedItemChangedCommand =>
            scheduleTypeSelectedItemChangedCommand ?? (scheduleTypeSelectedItemChangedCommand = new DelegateCommand(ExecuteScheduleTypeSelectedItemChangedCommand));

        async void ExecuteScheduleTypeSelectedItemChangedCommand()
        {
            var selectedItem = ScheduleTypeSelectedItem;
            if (selectedItem == null || !init)
            {
                return;
            }

            TodayScheduleItems = null;
            CalendarItems = null;
            EverydayScheduleItems = null;

            // 查询数据库
            switch (scheduleTypeSelectedItem.Value)
            {
                case ScheduleType.今日排班:
                    {
                        calendarPage = null;

                        var (status, msg, multiItems) = await Crs_DialogEx.ProgressShow().GetProgressResultAsync<(bool, string, List<(Schedule schedule, OrganizationPatient patient, List<ProgramModule> programModules)>)>(async exception =>
                        {
                            exception.Exception = async ex =>
                            {
                                exception.Message = "获取今日排班信息错误";
                                return (false, $"{exception.Message},{ex.Message}", null);
                            };

                            var today = DateTime.Today.Date;

                            // 查询今日排班列表

                            var multiItems = await (from schedule in db.Schedules.AsNoTracking().Include(m => m.Program).Where(m => m.CreateTime.Date == today)
                                                    join patient in db.OrganizationPatients.AsNoTracking() on schedule.PatientId equals patient.Id into patients
                                                    from patient in patients.DefaultIfEmpty()
                                                    join programModule in db.ProgramModules.AsNoTracking().Include(m => m.Module) on schedule.ProgramId equals programModule.ProgramId into programModules
                                                    from programModule in programModules.DefaultIfEmpty()
                                                    where patient != null
                                                    select new { schedule, patient, programModule })
                                                    .ToListAsync();

                            var _multiItems = multiItems
                                .GroupBy(m => m.schedule.ScheduleId)
                                .Select(m =>
                                {
                                    var firstItem = m.FirstOrDefault();
                                    return (firstItem?.schedule, firstItem?.patient, m.Select(n => n.programModule).ToList());
                                })
                                .ToList();

                            return (true, null, _multiItems);
                        });

                        if (!status)
                        {
                            await Crs_DialogEx.MessageBoxShow().GetMessageBoxResultAsync(msg);
                            return;
                        }

                        var items = timeScales.Select(m => new ScheduleItem
                        {
                            BeginTime = m.beginTime,
                            EndTime = m.endTime,
                            StatusItems = new ObservableCollection<MultiItem<ScheduleStatus>>(Enum.GetValues<ScheduleStatus>().Cast<ScheduleStatus>().Select(m => new MultiItem<ScheduleStatus> { Item1 = m }))
                        }).ToList();

                        foreach (var item in multiItems)
                        {
                            var schedule = item.schedule;
                            var patient = item.patient;

                            if (!schedule.StartTime.HasValue || !schedule.EndTime.HasValue)
                            {
                                continue;
                            }

                            var startTime = schedule.StartTime.Value;
                            var endTime = schedule.EndTime.Value;

                            var _item = items.FirstOrDefault(m => startTime.TimeOfDay >= m.BeginTime.TimeOfDay && endTime.TimeOfDay <= m.EndTime.TimeOfDay);
                            if (_item != null)
                            {
                                _item.Update(item);
                            }
                        }

                        TodayScheduleItems = new ObservableCollection<ScheduleItem>(items);
                    }
                    break;
                case ScheduleType.每日排班:
                    {
                        calendarPage ??= 0;

                        var weeks = new string[] { "一", "二", "三", "四", "五", "六", "日" };
                        var today = DateTime.Today.AddDays((int)calendarPage * 7);
                        var weekIndex = (int)today.DayOfWeek - 1;
                        weekIndex = weekIndex >= 0 ? weekIndex : 6;

                        var calendarItems = new List<MultiItem<string, int, DateTime>>(weeks.Select(m => new MultiItem<string, int, DateTime> { Item1 = m }));
                        for (int index = 0; index < calendarItems.Count; index++)
                        {
                            var _today = today.AddDays(index - weekIndex);
                            calendarItems[index].Item2 = _today.Day;
                            calendarItems[index].Item3 = _today;
                        }

                        var _calendarItems = calendarItems.Select(m => new MultiItem<string, int> { Item1 = m.Item1, Item2 = m.Item2 });
                        CalendarItems = new ObservableCollection<MultiItem<string, int>>(_calendarItems);

                        var (status, msg, multiItems) = await Crs_DialogEx.ProgressShow().GetProgressResultAsync<(bool, string, List<(Schedule schedule, OrganizationPatient patient)>)>(async exception =>
                        {
                            exception.Exception = async ex =>
                            {
                                exception.Message = "获取每日排班信息错误";
                                return (false, $"{exception.Message},{ex.Message}", null);
                            };

                            var beginTime = calendarItems.FirstOrDefault().Item3;
                            var endTime = calendarItems.LastOrDefault().Item3.AddDays(1).AddTicks(-1);

                            // 查询每日排班列表

                            var multiItems = await (from schedule in db.Schedules.AsNoTracking().Include(m => m.Program).Where(m => m.CreateTime >= beginTime && m.CreateTime <= endTime)
                                                    join patient in db.OrganizationPatients.AsNoTracking() on schedule.PatientId equals patient.Id into patients
                                                    from patient in patients.DefaultIfEmpty()
                                                    where patient != null
                                                    select new { schedule, patient })
                                                   .ToListAsync();

                            var _multiItems = multiItems
                                .Select(m => (m.schedule, m.patient))
                                .ToList();

                            return (true, null, _multiItems);
                        });

                        if (!status)
                        {
                            await Crs_DialogEx.MessageBoxShow().GetMessageBoxResultAsync(msg);
                            return;
                        }

                        var items = timeScales.Select(m => new List<ScheduleItem>(_calendarItems.Select(n => new ScheduleItem
                        {
                            BeginTime = m.beginTime,
                            EndTime = m.endTime,
                            Day = n.Item2
                        }))).ToList();

                        var _items = (from item in items let __items = item from _item in __items select _item).ToList();

                        foreach (var item in multiItems)
                        {
                            var schedule = item.schedule;
                            var patient = item.patient;

                            if (!schedule.StartTime.HasValue || !schedule.EndTime.HasValue)
                            {
                                continue;
                            }

                            var createTime = schedule.CreateTime;
                            var startTime = schedule.StartTime.Value;
                            var endTime = schedule.EndTime.Value;

                            var __items = _items.Where(m => m.Day == createTime.Day && startTime.TimeOfDay >= m.BeginTime.TimeOfDay && endTime.TimeOfDay <= m.EndTime.TimeOfDay).ToList();
                            foreach (var _item in __items)
                            {
                                _item.Update((item.schedule, item.patient, null));
                            }
                        }

                        EverydayScheduleItems = new ObservableCollection<List<ScheduleItem>>(items);
                    }
                    break;
            }
        }

        public override void OnNavigatedTo(NavigationContext navigationContext)
        {
            var parameters = navigationContext.Parameters;

            this.doctor = parameters.GetValue<User>("crs_doctor");

            var items = Enum.GetValues(typeof(ScheduleType)).Cast<ScheduleType>().Select(m => new KeyValueItem<string, ScheduleType>(m.ToString(), m));
            ScheduleTypeItems = new ObservableCollection<KeyValueItem<string, ScheduleType>>(items);
            ScheduleTypeSelectedItem = ScheduleTypeItems.FirstOrDefault();

            init = true;
            ScheduleTypeSelectedItemChangedCommand?.Execute();
        }
    }
}
