using crs.core.DbModels;
using crs.extension;
using crs.extension.Models;
using Prism.Commands;
using Prism.Events;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using static crs.extension.Crs_Enum;
using crs.theme.Extensions;
using Microsoft.EntityFrameworkCore;
using crs.window.Views;
using HandyControl.Tools.Extension;
using System.Windows;
using crs.core;
using Newtonsoft.Json;
using System.Windows.Interop;
using static crs.extension.Crs_EventAggregator;
using System.Reflection.Metadata;
using System.Threading.Tasks;

namespace crs.window.ViewModels
{
    public class TrainViewModel : Crs_BindableBase
    {
        readonly IRegionManager regionManager;
        readonly IContainerProvider containerProvider;
        readonly IEventAggregator eventAggregator;
        readonly Crs_Db2Context db;

        User doctor;
        OrganizationPatient patient;
        int? programId;

        bool init = false;

        public TrainViewModel() { }
        public TrainViewModel(IRegionManager regionManager, IContainerProvider containerProvider, IEventAggregator eventAggregator, Crs_Db2Context db)
        {
            this.regionManager = regionManager;
            this.containerProvider = containerProvider;
            this.eventAggregator = eventAggregator;
            this.db = db;
        }

        #region Property

        private ObservableCollection<MultiItem<TrainType, List<Module>>> trainTypeItems;
        public ObservableCollection<MultiItem<TrainType, List<Module>>> TrainTypeItems
        {
            get { return trainTypeItems; }
            set { SetProperty(ref trainTypeItems, value); }
        }

        private MultiItem<TrainType, List<Module>> trainTypeSelectedItem;
        public MultiItem<TrainType, List<Module>> TrainTypeSelectedItem
        {
            get { return trainTypeSelectedItem; }
            set { SetProperty(ref trainTypeSelectedItem, value); }
        }

        private ObservableCollection<TrainItem> trainModeItems;
        public ObservableCollection<TrainItem> TrainModeItems
        {
            get { return trainModeItems; }
            set { SetProperty(ref trainModeItems, value); }
        }

        private TrainItem trainModeSelectedItem;
        public TrainItem TrainModeSelectedItem
        {
            get { return trainModeSelectedItem; }
            set { SetProperty(ref trainModeSelectedItem, value); }
        }

        private ObservableCollection<ProgramItem<TrainMode, TrainItem>> trainProgramItems;
        public ObservableCollection<ProgramItem<TrainMode, TrainItem>> TrainProgramItems
        {
            get { return trainProgramItems; }
            set { SetProperty(ref trainProgramItems, value); }
        }

        private ProgramItem<TrainMode, TrainItem> trainProgramSelectedItem;
        public ProgramItem<TrainMode, TrainItem> TrainProgramSelectedItem
        {
            get { return trainProgramSelectedItem; }
            set { SetProperty(ref trainProgramSelectedItem, value); }
        }

        private ObservableCollection<ProgramItem<TrainMode, TrainItem>> trainHistoryProgramItems;
        public ObservableCollection<ProgramItem<TrainMode, TrainItem>> TrainHistoryProgramItems
        {
            get { return trainHistoryProgramItems; }
            set { SetProperty(ref trainHistoryProgramItems, value); }
        }

        private bool historyIsOpen;
        public bool HistoryIsOpen
        {
            get { return historyIsOpen; }
            set { SetProperty(ref historyIsOpen, value); }
        }

        private bool historyStaysOpen;
        public bool HistoryStaysOpen
        {
            get { return historyStaysOpen; }
            set { SetProperty(ref historyStaysOpen, value); }
        }

        private bool isFromSchedule;
        public bool IsFromSchedule
        {
            get { return isFromSchedule; }
            set { SetProperty(ref isFromSchedule, value); }
        }

        private string diagnosisInfo;
        public string DiagnosisInfo
        {
            get { return diagnosisInfo; }
            set { SetProperty(ref diagnosisInfo, value); }
        }
        #endregion

        private DelegateCommand<ProgramItem<TrainMode, TrainItem>> importCommand;
        public DelegateCommand<ProgramItem<TrainMode, TrainItem>> ImportCommand =>
            importCommand ?? (importCommand = new DelegateCommand<ProgramItem<TrainMode, TrainItem>>(ExecuteImportCommand));

        async void ExecuteImportCommand(ProgramItem<TrainMode, TrainItem> parameter)
        {
            if (parameter == null)
            {
                return;
            }

            var selectedItem = parameter.Item;

            if (TrainProgramItems != null && TrainProgramItems.FirstOrDefault(m => m.Item.Mode == selectedItem.Mode) != null)
            {
                try
                {
                    HistoryStaysOpen = true;
                    await Crs_DialogEx.MessageBoxShow().GetMessageBoxResultAsync($"模式“{selectedItem.Mode}”已添加");
                }
                finally
                {
                    HistoryStaysOpen = false;
                }
                return;
            }

            var item = selectedItem.ClassCopyAndMapper<TrainItem, TrainItem>();

            TrainProgramItems ??= new ObservableCollection<ProgramItem<TrainMode, TrainItem>>();
            TrainProgramItems.Add(new ProgramItem<TrainMode, TrainItem>
            {
                Mode = selectedItem.Mode,
                Item = item.UpdateMessageInfo()
            });
            TrainProgramSelectedItem = TrainProgramItems.LastOrDefault();
        }


        private DelegateCommand<ProgramItem<TrainMode, TrainItem>> deleteCommand;
        public DelegateCommand<ProgramItem<TrainMode, TrainItem>> DeleteCommand =>
            deleteCommand ?? (deleteCommand = new DelegateCommand<ProgramItem<TrainMode, TrainItem>>(ExecuteDeleteCommand));

        async void ExecuteDeleteCommand(ProgramItem<TrainMode, TrainItem> parameter)
        {
            if (parameter == null)
            {
                return;
            }
            var _report = db.Results.FirstOrDefault(m => m.ProgramId == parameter.Data.ProgramId);
            if (_report != null)
            {
                var _ResultDetailsList = db.ResultDetails.Where(m => m.ResultId == _report.ResultId).ToList();
                foreach (var resultDetail in _ResultDetailsList)
                {
                    db.ResultDetails.Remove(resultDetail);
                }
                await db.SaveChangesAsync();

                db.Results.Remove(_report);
                await db.SaveChangesAsync();
            }


            var programItem = db.Programs.FirstOrDefault(m => m.ProgramId == parameter.Data.ProgramId);
            if (programItem != null)
            {
                db.Programs.Remove(programItem);
                await db.SaveChangesAsync();
            }

            var selectedItem = parameter.Item;
            TrainHistoryProgramItems.Remove(parameter);

            return;
        }


        private DelegateCommand historyProgramCommand;
        public DelegateCommand HistoryProgramCommand =>
            historyProgramCommand ?? (historyProgramCommand = new DelegateCommand(ExecuteHistoryProgramCommand));

        async void ExecuteHistoryProgramCommand()
        {
            if (patient == null)
            {
                await Crs_DialogEx.MessageBoxShow().GetMessageBoxResultAsync("当前患者信息为空");
                return;
            }

            TrainHistoryProgramItems = null;
            var (status, msg, multiItems) = await Crs_DialogEx.ProgressShow().GetProgressResultAsync<(bool, string, List<(Program program, ProgramModule programModule, List<ProgramModulePar> programModulePars)>)>(async exception =>
            {
                exception.Exception = async ex =>
                {
                    exception.Message = "获取历史信息错误";
                    return (false, $"{exception.Message},{ex.Message}", null);
                };

                var patientId = patient.Id;

                var multiItems = await (from program in db.Programs.AsNoTracking().Where(m => m.PatientId == patientId && (m.Eval == null || m.Eval == false))
                                        join programModule in db.ProgramModules.AsNoTracking().Include(m => m.Module) on program.ProgramId equals programModule.ProgramId into programModules
                                        from programModule in programModules.DefaultIfEmpty()
                                        join programModulePar in db.ProgramModulePars.AsNoTracking().Include(m => m.ModulePar).ThenInclude(m => m.Module) on program.ProgramId equals programModulePar.ProgramId into ProgramModulePars
                                        from programModulePar in ProgramModulePars.DefaultIfEmpty()
                                        where programModule != null && programModulePar != null && programModule.ModuleId == programModulePar.ModulePar.ModuleId
                                        select new { program, programModule, programModulePar })
                                        .OrderByDescending(m => m.program.CreateTime)
                                        .ToListAsync();

                //var __multiItems = await (from program in db.Programs.AsNoTracking().Where(m => m.PatientId == patientId && (m.Eval == null || m.Eval == false))
                //                          join mulitModule in (from programModule in db.ProgramModules.AsNoTracking().Include(m => m.Module)
                //                                               from programModulePar in db.ProgramModulePars.AsNoTracking().Include(m => m.ModulePar)
                //                                               where programModule.ProgramId == programModulePar.ProgramId && programModule.ModuleId == programModulePar.ModulePar.ModuleId
                //                                               select new { programModule, programModulePar }) on program.ProgramId equals mulitModule.programModule.ProgramId into mulitModules
                //                          from mulitModule in mulitModules.DefaultIfEmpty()
                //                          where mulitModule.programModule != null && mulitModule.programModulePar != null
                //                          select new { program, mulitModule.programModule, mulitModule.programModulePar })
                //                          .OrderByDescending(m => m.program.CreateTime)
                //                          .ToListAsync();

                var _multiItems = multiItems
                    .GroupBy(m => (m.program.ProgramId, m.programModule.ModuleId, m.programModulePar.ModuleParId))
                    .Select(m =>
                    {
                        var firstItem = m.FirstOrDefault();
                        return new { firstItem.program, firstItem.programModule, firstItem.programModulePar };
                    })
                    .GroupBy(m => (m.program.ProgramId, m.programModule.ModuleId))
                    .Select(m =>
                    {
                        var firstItem = m.FirstOrDefault();
                        return (firstItem.program, firstItem.programModule, m.Select(m => m.programModulePar).ToList());
                    })
                    .ToList();

                return (true, null, _multiItems);
            });

            if (!status)
            {
                await Crs_DialogEx.MessageBoxShow().GetMessageBoxResultAsync(msg);
                return;
            }

            var items = multiItems.Select(m =>
            {
                var program = m.program;
                var module = m.programModule.Module;
                var multiModulePars = m.programModulePars.Select(m => (m.ModulePar, m.Value)).ToList();

                if (!Enum.TryParse<TrainType>(module.TrainType?.Trim(), out var typeResult))
                {
                    return null;
                }

                if (!Enum.TryParse<TrainMode>(module.Name?.Trim(), out var modeResult))
                {
                    return null;
                }

                var trainItem = new TrainItem { Type = typeResult, Mode = modeResult }.Update(module);

                var modulePars = multiModulePars.Where(m => m.ModulePar.FeedbackType == null).ToList();
                trainItem.ModuleParItems = new ObservableCollection<ModuleParItem>(modulePars.Select(m => new ModuleParItem().Update(m.ModulePar, defaultValue: m.Value)));

                var feedbackModulePars = multiModulePars.Where(m => m.ModulePar.FeedbackType != null).ToList();
                trainItem.FeedbackModuleParItems = new ObservableCollection<ModuleParItem>(feedbackModulePars.Select(m => new ModuleParItem().Update(m.ModulePar, isChecked: m.Value == 1)));

                return new ProgramItem<TrainMode, TrainItem>
                {
                    Mode = modeResult,
                    Item = trainItem.UpdateMessageInfo(),
                    CreateTime = program.CreateTime
                }.Update(program);
            }).Where(m => m != null).ToList();

            TrainHistoryProgramItems = new ObservableCollection<ProgramItem<TrainMode, TrainItem>>(items);
            HistoryIsOpen = true;
        }

        private DelegateCommand<ProgramItem<TrainMode, TrainItem>> deleteProgramCommand;
        public DelegateCommand<ProgramItem<TrainMode, TrainItem>> DeleteProgramCommand =>
            deleteProgramCommand ?? (deleteProgramCommand = new DelegateCommand<ProgramItem<TrainMode, TrainItem>>(ExecuteDeleteProgramCommand));

        void ExecuteDeleteProgramCommand(ProgramItem<TrainMode, TrainItem> parameter)
        {
            TrainProgramItems.Remove(parameter);
        }

        private DelegateCommand addCommand;
        public DelegateCommand AddCommand =>
            addCommand ?? (addCommand = new DelegateCommand(ExecuteAddCommand));

        async void ExecuteAddCommand()
        {
            if (patient == null)
            {
                await Crs_DialogEx.MessageBoxShow().GetMessageBoxResultAsync("当前患者信息为空");
                return;
            }

            var selectedItem = TrainModeSelectedItem;
            if (selectedItem == null)
            {
                await Crs_DialogEx.MessageBoxShow().GetMessageBoxResultAsync("请选择");
                return;
            }

            var parItems = selectedItem.ModuleParItems;
            if (parItems == null || parItems.Count == 0)
            {
                await Crs_DialogEx.MessageBoxShow().GetMessageBoxResultAsync("请设置参数");
                return;
            }

            if (TrainProgramItems != null && TrainProgramItems.FirstOrDefault(m => m.Item.Mode == selectedItem.Mode) != null)
            {
                await Crs_DialogEx.MessageBoxShow().GetMessageBoxResultAsync($"模式“{selectedItem.Mode}”已添加");
                return;
            }

            var item = selectedItem.ClassCopyAndMapper<TrainItem, TrainItem>();

            TrainProgramItems ??= new ObservableCollection<ProgramItem<TrainMode, TrainItem>>();
            TrainProgramItems.Add(new ProgramItem<TrainMode, TrainItem>
            {
                Mode = selectedItem.Mode,
                Item = item.UpdateMessageInfo()
            });
            TrainProgramSelectedItem = TrainProgramItems.LastOrDefault();

            ExecuteTrainModeSelectedItemChangedCommand();
        }

        private DelegateCommand startCommand;
        public DelegateCommand StartCommand =>
            startCommand ?? (startCommand = new DelegateCommand(ExecuteStartCommand));

        async void ExecuteStartCommand()
        {
            var items = TrainProgramItems;
            if (items == null || items.Count == 0)
            {
                await Crs_DialogEx.MessageBoxShow().GetMessageBoxResultAsync("请先添加方案");
                return;
            }

            var selectedItem = await Crs_DialogEx.Show(Crs_Dialog.DigitalHumanSelected)
                .Initialize<IDialogCommon>(vm => vm.Execute()).GetResultAsync<DigitalHumanItem>();

            if (selectedItem == null)
            {
                return;
            }

            var programId = items.FirstOrDefault().Data?.ProgramId ?? -1;
            var (status, msg, program) = await SaveProgramInfo(items.ToList(), programId);

            if (!status)
            {
                await Crs_DialogEx.MessageBoxShow().GetMessageBoxResultAsync(msg);
                return;
            }

            //foreach (var item in items)
            //{
            //    item.Update(program);
            //}

            programId = program.ProgramId;
            var result = await Crs_DialogEx.Show(Crs_Dialog.TrainGamePanel, Crs_DialogToken.TopContent)
                .UseConfig_ContentStretch()
                .Initialize<IDialogCommon<int, DigitalHumanItem>>(vm => vm.Execute(programId, selectedItem))
                .GetResultAsync<bool>();

            /*LJN
            20241115新加需求：在训练或者评估模块点击左上角中断返回后，需要自动删除右边已经选择的测试方案。
            由于之前写的是评估完成了，有结果了，result不为false才清楚原本选择的
            解决方案：
            去掉根据result的判断，每次开始一个评估方案的时候就手动清除 EvaluateProgramItems?.Clear();
            */
            //if (result)
            //{
            //    TrainProgramItems?.Clear();
            //}
            TrainProgramItems?.Clear();
        }

        private DelegateCommand saveCommand;
        public DelegateCommand SaveCommand =>
            saveCommand ?? (saveCommand = new DelegateCommand(ExecuteSaveCommand));

        async void ExecuteSaveCommand()
        {
            var items = TrainProgramItems;
            if (items == null || items.Count == 0)
            {
                await Crs_DialogEx.MessageBoxShow().GetMessageBoxResultAsync("请先添加方案");
                return;
            }

            if (programId == null)
            {
                await Crs_DialogEx.MessageBoxShow().GetMessageBoxResultAsync("方案ID为空");
                return;
            }

            var (status, msg, program) = await SaveProgramInfo(items.ToList(), (int)programId);

            if (!status)
            {
                await Crs_DialogEx.MessageBoxShow().GetMessageBoxResultAsync(msg);
                return;
            }

            var parameters = new NavigationParameters
            {
                {"crs_doctor",doctor }
            };

            eventAggregator.GetEvent<MenuSelectedChangedEvent>().Publish((MenuType.Schedule, false));
            regionManager.RequestNavigate(Crs_Region.Menu, MenuType.Schedule.ToString(), navigationParameters: parameters);
        }

        async Task<(bool status, string msg, Program program)> SaveProgramInfo(List<ProgramItem<TrainMode, TrainItem>> items, int programId)
        {
            return await Crs_DialogEx.ProgressShow().GetProgressResultAsync<(bool, string, Program)>(async exception =>
            {
                exception.Exception = async ex =>
                {
                    exception.Message = "保存方案错误";
                    return (false, $"{exception.Message},{ex.Message}", null);
                };

                var transaction = await db.Database.BeginTransactionAsync();
                try
                {
                    var patientId = patient.Id;

                    var program = await db.Programs.FirstOrDefaultAsync(m => m.ProgramId == programId);
                    if (program == null)
                    {
                        program = new Program
                        {
                            CreateTime = DateTime.Now,
                            Eval = false,
                            PatientId = patientId,
                        };

                        db.Programs.Add(program);
                        await db.SaveChangesAsync();
                        db.Entry(program);
                    }
                    else
                    {
                        program.CreateTime ??= DateTime.Now;
                        program.Eval = false;
                        program.PatientId = patientId;

                        db.Programs.Update(program);
                    }

                    programId = program.ProgramId;

                    var oldProgramModules = await db.ProgramModules.Where(m => m.ProgramId == programId).ToListAsync();
                    db.ProgramModules.RemoveRange(oldProgramModules);

                    var oldProgramModulePars = await db.ProgramModulePars.Where(m => m.ProgramId == programId).ToListAsync();
                    db.ProgramModulePars.RemoveRange(oldProgramModulePars);

                    var moduleItems = (from item in items select item.Item).ToList();

                    var newProgramModules = moduleItems.Select(m =>
                    {
                        var module = m.DataEx;
                        return new ProgramModule
                        {
                            ProgramId = programId,
                            ModuleId = module.ModuleId
                        };
                    }).ToList();
                    db.ProgramModules.AddRange(newProgramModules);

                    var newProgramModulePars = (from moduleItem in moduleItems
                                                let moduleParItems = moduleItem.ModuleParItems.Concat(moduleItem.FeedbackModuleParItems)
                                                from moduleParItem in moduleParItems
                                                select moduleParItem)
                                                .Select(m =>
                                                {
                                                    var modulePar = m.DataEx;
                                                    return new ProgramModulePar
                                                    {
                                                        ProgramId = programId,
                                                        ModuleParId = modulePar.ModuleParId,
                                                        Value = m.IsFeedbackType ? (m.IsChecked ? 1 : 0) : m.DefaultValue
                                                    };
                                                }).ToList();
                    db.ProgramModulePars.AddRange(newProgramModulePars);

                    await db.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return (true, "保存方案成功", program);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    throw new Exception(ex.Message, ex);
                }
            });
        }

        private DelegateCommand trainModeSelectedItemChangedCommand;
        public DelegateCommand TrainModeSelectedItemChangedCommand =>
            trainModeSelectedItemChangedCommand ?? (trainModeSelectedItemChangedCommand = new DelegateCommand(ExecuteTrainModeSelectedItemChangedCommand));

        void ExecuteTrainModeSelectedItemChangedCommand()
        {
            var selectedItem = TrainModeSelectedItem;
            if (selectedItem == null || !init)
            {
                return;
            }

            var modulePars = selectedItem.Data.ModulePars.Where(m => m.FeedbackType == null).OrderBy(m => m.Name).ToList();
            selectedItem.ModuleParItems = new ObservableCollection<ModuleParItem>(modulePars.Select(m => new ModuleParItem().Update(m)));

            var feedbackModulePars = selectedItem.Data.ModulePars.Where(m => m.FeedbackType != null).OrderBy(m => m.Name).ToList();
            selectedItem.FeedbackModuleParItems = new ObservableCollection<ModuleParItem>(feedbackModulePars.Select(m => new ModuleParItem().Update(m)));
        }

        private DelegateCommand trainTypeSelectedItemChangedCommand;
        public DelegateCommand TrainTypeSelectedItemChangedCommand =>
            trainTypeSelectedItemChangedCommand ?? (trainTypeSelectedItemChangedCommand = new DelegateCommand(ExecuteTrainTypeSelectedItemChangedCommand));

        void ExecuteTrainTypeSelectedItemChangedCommand()
        {
            var selectedItem = TrainTypeSelectedItem;
            if (selectedItem == null || !init)
            {
                return;
            }

            var modules = selectedItem.Item2;
            var items = modules.Select(m =>
            {
                if (Enum.TryParse<TrainMode>(m.Name?.Trim(), out var result))
                {
                    return new TrainItem { Type = selectedItem.Item1, Mode = result }.Update(m);
                }
                return null;
            }).Where(m => m != null).ToList();

            TrainModeItems = new ObservableCollection<TrainItem>(items);
            TrainModeSelectedItem = TrainModeItems.FirstOrDefault();
        }

        public async override void OnNavigatedTo(NavigationContext navigationContext)
        {
            var parameters = navigationContext.Parameters;

            this.doctor = parameters.GetValue<User>("crs_doctor");
            this.patient = parameters.GetValue<OrganizationPatient>("crs_patient");
            this.programId = parameters.GetValue<int?>("crs_programId");

            var (status, msg, moduleGroups, multiItems) = await Crs_DialogEx.ProgressShow().GetProgressResultAsync<(bool, string, List<IGrouping<string, Module>>, List<(ProgramModule programModule, List<ProgramModulePar> programModulePars)>)>(async exception =>
            {
                exception.Exception = async ex =>
                {
                    exception.Message = "获取康复训练模块信息错误";
                    return (false, $"{exception.Message},{ex.Message}", null, null);
                };

                var modules = await db.Modules.AsNoTracking()
                    .Include(m => m.ModulePars).Where(m => m.Type == "train").ToListAsync();
                var moduleGroups = modules.GroupBy(m => m.TrainType).ToList();

                if (programId == null)
                {
                    return (true, null, moduleGroups, null);
                }

                var multiItems = await (from programModule in db.ProgramModules.AsNoTracking().Include(m => m.Module).Where(m => m.ProgramId == programId)
                                        from programModulePar in db.ProgramModulePars.AsNoTracking().Include(m => m.ModulePar).Where(m => m.ProgramId == programId)
                                        where programModule.ModuleId == programModulePar.ModulePar.ModuleId
                                        select new { programModule, programModulePar })
                                        .ToListAsync();

                var _multiItems = multiItems
                    .GroupBy(m => m.programModule.ModuleId)
                    .Select(m =>
                    {
                        var firstItem = m.FirstOrDefault();
                        return (firstItem.programModule, m.Select(m => m.programModulePar).ToList());
                    })
                    .ToList();

                return (true, null, moduleGroups, _multiItems);
            });

            if (!status)
            {
                await Crs_DialogEx.MessageBoxShow().GetMessageBoxResultAsync(msg);
                return;
            }

            {
                var items = moduleGroups.Select(m =>
                {
                    if (Enum.TryParse<TrainType>(m.Key?.Trim(), out var result))
                    {
                        return new MultiItem<TrainType, List<Module>> { Item1 = result, Item2 = m.ToList() };
                    }
                    return null;
                }).Where(m => m != null).ToList();

                TrainTypeItems = new ObservableCollection<MultiItem<TrainType, List<Module>>>(items);
                TrainTypeSelectedItem = TrainTypeItems.FirstOrDefault();
            }

            if (multiItems != null)
            {
                var items = multiItems.Select(m =>
                {
                    var module = m.programModule.Module;
                    var multiModulePars = m.programModulePars.Select(m => (m.ModulePar, m.Value)).ToList();

                    if (!Enum.TryParse<TrainType>(module.TrainType?.Trim(), out var typeResult))
                    {
                        return null;
                    }

                    if (!Enum.TryParse<TrainMode>(module.Name?.Trim(), out var modeResult))
                    {
                        return null;
                    }

                    var trainItem = new TrainItem { Type = typeResult, Mode = modeResult }.Update(module);

                    var modulePars = multiModulePars.Where(m => m.ModulePar.FeedbackType == null).ToList();
                    trainItem.ModuleParItems = new ObservableCollection<ModuleParItem>(modulePars.Select(m => new ModuleParItem().Update(m.ModulePar, defaultValue: m.Value)));

                    var feedbackModulePars = multiModulePars.Where(m => m.ModulePar.FeedbackType != null).ToList();
                    trainItem.FeedbackModuleParItems = new ObservableCollection<ModuleParItem>(feedbackModulePars.Select(m => new ModuleParItem().Update(m.ModulePar, isChecked: m.Value == 1)));

                    return trainItem;
                }).Where(m => m != null).ToList();

                TrainProgramItems ??= new ObservableCollection<ProgramItem<TrainMode, TrainItem>>();
                TrainProgramItems.AddRange(items.Select(m => new ProgramItem<TrainMode, TrainItem>
                {
                    Mode = m.Mode,
                    Item = m.UpdateMessageInfo()
                }));

                DiagnosisInfo = string.Join("、", TrainTypeItems.Select(m => m.Item1));
                IsFromSchedule = true;
            }

            init = true;
            TrainTypeSelectedItemChangedCommand?.Execute();
        }
    }
}
