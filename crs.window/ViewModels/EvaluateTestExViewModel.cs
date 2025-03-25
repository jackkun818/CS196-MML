using crs.core;
using crs.core.DbModels;
using crs.extension;
using crs.extension.Models;
using crs.theme.Extensions;
using crs.window.Views;
using HandyControl.Controls;
using HandyControl.Tools.Extension;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client.NativeInterop;
using Prism.Commands;
using Prism.Events;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using static crs.extension.Crs_Enum;
using static crs.extension.Crs_EventAggregator;
using static SkiaSharp.HarfBuzz.SKShaper;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using Module = crs.core.DbModels.Module;
using Window = System.Windows.Window;

namespace crs.window.ViewModels
{
    public class EvaluateTestExViewModel : Crs_BindableBase
    {
        readonly IRegionManager regionManager;
        readonly IContainerProvider containerProvider;
        readonly IEventAggregator eventAggregator;
        readonly Crs_Db2Context db;

        User doctor;
        OrganizationPatient patient;
        int? programId;

        public EvaluateTestExViewModel() { }
        public EvaluateTestExViewModel(IRegionManager regionManager, IContainerProvider containerProvider, IEventAggregator eventAggregator, Crs_Db2Context db)
        {
            this.regionManager = regionManager;
            this.containerProvider = containerProvider;
            this.eventAggregator = eventAggregator;
            this.db = db;
        }

        #region Property

        private ObservableCollection<EvaluateTestItem> evaluateModuleItems;
        public ObservableCollection<EvaluateTestItem> EvaluateModuleItems
        {
            get { return evaluateModuleItems; }
            set { SetProperty(ref evaluateModuleItems, value); }
        }

        private EvaluateTestItem evaluateModuleSelectedItem;
        public EvaluateTestItem EvaluateModuleSelectedItem
        {
            get { return evaluateModuleSelectedItem; }
            set { SetProperty(ref evaluateModuleSelectedItem, value); }
        }

        private ObservableCollection<ProgramItem<EvaluateTestMode, EvaluateTestItem>> evaluateProgramItems;
        public ObservableCollection<ProgramItem<EvaluateTestMode, EvaluateTestItem>> EvaluateProgramItems
        {
            get { return evaluateProgramItems; }
            set { SetProperty(ref evaluateProgramItems, value); }
        }

        private ProgramItem<EvaluateTestMode, EvaluateTestItem> evaluateProgramSelectedItem;
        public ProgramItem<EvaluateTestMode, EvaluateTestItem> EvaluateProgramSelectedItem
        {
            get { return evaluateProgramSelectedItem; }
            set { SetProperty(ref evaluateProgramSelectedItem, value); }
        }

        private ObservableCollection<ProgramItem<EvaluateTestMode, EvaluateTestItem>> evaluateHistoryProgramItems;
        public ObservableCollection<ProgramItem<EvaluateTestMode, EvaluateTestItem>> EvaluateHistoryProgramItems
        {
            get { return evaluateHistoryProgramItems; }
            set { SetProperty(ref evaluateHistoryProgramItems, value); }
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

        private DelegateCommand<ProgramItem<EvaluateTestMode, EvaluateTestItem>> importCommand;
        public DelegateCommand<ProgramItem<EvaluateTestMode, EvaluateTestItem>> ImportCommand =>
            importCommand ?? (importCommand = new DelegateCommand<ProgramItem<EvaluateTestMode, EvaluateTestItem>>(ExecuteImportCommand));

        async void ExecuteImportCommand(ProgramItem<EvaluateTestMode, EvaluateTestItem> parameter)
        {
            if (parameter == null)
            {
                return;
            }

            var selectedItem = parameter.Item;

            if (EvaluateProgramItems != null && EvaluateProgramItems.FirstOrDefault(m => m.Item.Mode == selectedItem.Mode) != null)
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

            var item = selectedItem.ClassCopyAndMapper<EvaluateTestItem, EvaluateTestItem>();

            EvaluateProgramItems ??= new ObservableCollection<ProgramItem<EvaluateTestMode, EvaluateTestItem>>();
            EvaluateProgramItems.Add(new ProgramItem<EvaluateTestMode, EvaluateTestItem>
            {
                Mode = selectedItem.Mode,
                Item = item
            });
            EvaluateProgramSelectedItem = EvaluateProgramItems.LastOrDefault();
        }

        private DelegateCommand<ProgramItem<EvaluateTestMode, EvaluateTestItem>> deleteCommand;
        public DelegateCommand<ProgramItem<EvaluateTestMode, EvaluateTestItem>> DeleteCommand =>
            deleteCommand ?? (deleteCommand = new DelegateCommand<ProgramItem<EvaluateTestMode, EvaluateTestItem>>(ExecuteDeleteCommand));

        async void ExecuteDeleteCommand(ProgramItem<EvaluateTestMode, EvaluateTestItem> parameter)
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
            EvaluateHistoryProgramItems.Remove(parameter);

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

            EvaluateHistoryProgramItems = null;
            var (status, msg, multiItems) = await Crs_DialogEx.ProgressShow().GetProgressResultAsync<(bool, string, List<(Program program, List<ProgramModule> programModules)>)>(async exception =>
            {
                exception.Exception = async ex =>
                {
                    exception.Message = "获取历史信息错误";
                    return (false, $"{exception.Message},{ex.Message}", null);
                };

                var patientId = patient.Id;

                var multiItems = await (from program in db.Programs.AsNoTracking().Where(m => m.PatientId == patientId && m.Eval == true)
                                        join programModule in db.ProgramModules.AsNoTracking().Include(m => m.Module) on program.ProgramId equals programModule.ProgramId into programModules
                                        from programModule in programModules.DefaultIfEmpty()
                                        where programModule != null
                                        select new { program, programModule })
                                        .OrderByDescending(m => m.program.CreateTime)
                                        .ToListAsync();

                var _multiItems = multiItems
                    .GroupBy(m => m.program.ProgramId)
                    .Select(m =>
                    {
                        var firstItem = m.FirstOrDefault();
                        return (firstItem.program, m.Select(m => m.programModule).ToList());
                    })
                    .ToList();

                return (true, null, _multiItems);
            });

            if (!status)
            {
                await Crs_DialogEx.MessageBoxShow().GetMessageBoxResultAsync(msg);
                return;
            }

            var items = new List<ProgramItem<EvaluateTestMode, EvaluateTestItem>>();
            foreach (var item in multiItems)
            {
                var program = item.program;
                var modules = item.programModules;

                if (modules.FirstOrDefault(m => m.Module?.Name?.Trim() == "MoCA") != null && modules.FirstOrDefault(m => m.Module?.Name?.Trim() == "MMSE") != null)
                {
                    var modeResult = EvaluateTestMode.标准评估;
                    var evaluateItem = new EvaluateTestItem { Mode = modeResult };

                    items.Add(new ProgramItem<EvaluateTestMode, EvaluateTestItem>
                    {
                        Mode = modeResult,
                        Item = evaluateItem,
                        CreateTime = program.CreateTime
                    }.Update(program));
                    continue;
                }

                foreach (var _item in modules)
                {
                    var module = _item.Module;
                    if (!Enum.TryParse<EvaluateTestMode>(module.Name?.Trim(), out var modeResult))
                    {
                        continue;
                    }

                    var evaluateItem = new EvaluateTestItem { Mode = modeResult }.Update(module);
                    items.Add(new ProgramItem<EvaluateTestMode, EvaluateTestItem>
                    {
                        Mode = modeResult,
                        Item = evaluateItem,
                        CreateTime = program.CreateTime
                    }.Update(program));
                }
            }

            EvaluateHistoryProgramItems = new ObservableCollection<ProgramItem<EvaluateTestMode, EvaluateTestItem>>(items);
            HistoryIsOpen = true;
        }

        private DelegateCommand<ProgramItem<EvaluateTestMode, EvaluateTestItem>> deleteProgramCommand;
        public DelegateCommand<ProgramItem<EvaluateTestMode, EvaluateTestItem>> DeleteProgramCommand =>
            deleteProgramCommand ?? (deleteProgramCommand = new DelegateCommand<ProgramItem<EvaluateTestMode, EvaluateTestItem>>(ExecuteDeleteProgramCommand));

        void ExecuteDeleteProgramCommand(ProgramItem<EvaluateTestMode, EvaluateTestItem> parameter)
        {
            EvaluateProgramItems.Remove(parameter);
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

            var selectedItem = EvaluateModuleSelectedItem;
            if (selectedItem == null)
            {
                await Crs_DialogEx.MessageBoxShow().GetMessageBoxResultAsync("请选择");
                return;
            }

            if (EvaluateProgramItems != null && EvaluateProgramItems.FirstOrDefault(m => m.Item.Mode == selectedItem.Mode) != null)
            {
                await Crs_DialogEx.MessageBoxShow().GetMessageBoxResultAsync($"模式“{selectedItem.Mode}”已添加");
                return;
            }

            var item = selectedItem.ClassCopyAndMapper<EvaluateTestItem, EvaluateTestItem>();

            EvaluateProgramItems ??= new ObservableCollection<ProgramItem<EvaluateTestMode, EvaluateTestItem>>();
            EvaluateProgramItems.Add(new ProgramItem<EvaluateTestMode, EvaluateTestItem>
            {
                Mode = selectedItem.Mode,
                Item = item
            });
            EvaluateProgramSelectedItem = EvaluateProgramItems.LastOrDefault();
        }

        private DelegateCommand startCommand;
        public DelegateCommand StartCommand =>
            startCommand ?? (startCommand = new DelegateCommand(ExecuteStartCommand));

        async void ExecuteStartCommand()
        {
            var items = EvaluateProgramItems;
            if (items == null || items.Count == 0)
            {
                await Crs_DialogEx.MessageBoxShow().GetMessageBoxResultAsync("请先添加方案");
                return;
            }

            if (items.FirstOrDefault(m => m.Mode == EvaluateTestMode.标准评估) != null && items.FirstOrDefault(m => m.Mode != EvaluateTestMode.标准评估) != null)
            {
                await Crs_DialogEx.MessageBoxShow().GetMessageBoxResultAsync($"模式“标准评估”不应跟其他模式混合");
                return;
            }

            if (items.FirstOrDefault(m => m.Mode == EvaluateTestMode.标准评估) != null)
            {
                var programId = items.FirstOrDefault().Data?.ProgramId ?? -1;
                var (status, msg, program) = await SaveProgramInfo(items.ToList(), programId);

                if (!status)
                {
                    await Crs_DialogEx.MessageBoxShow().GetMessageBoxResultAsync(msg);
                    return;
                }

                programId = program.ProgramId;
                var result = await Crs_DialogEx.Show(Crs_Dialog.EvaluateStandardPanel, Crs_DialogToken.TopContent)
                    .UseConfig_ContentStretch()
                    .Initialize<IDialogCommon<int?>>(vm => vm.Execute(programId))
                    .GetResultAsync<bool>();

                /*LJN
                20241115新加需求：在训练或者评估模块点击左上角中断返回后，需要自动删除右边已经选择的测试方案。
                由于之前写的是评估完成了，有结果了，result不为false才清楚原本选择的
                解决方案：
                去掉根据result的判断，每次开始一个评估方案的时候就手动清除 EvaluateProgramItems?.Clear();
                */
                //if (result)
                //{
                //    EvaluateProgramItems?.Clear();
                //}
                EvaluateProgramItems?.Clear();
                return;
            }

            {
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
                var result = await Crs_DialogEx.Show(Crs_Dialog.EvaluateGamePanel, Crs_DialogToken.TopContent)
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
                //    EvaluateProgramItems?.Clear();
                //}
                EvaluateProgramItems?.Clear();
            }
        }

        private DelegateCommand saveCommand;
        public DelegateCommand SaveCommand =>
            saveCommand ?? (saveCommand = new DelegateCommand(ExecuteSaveCommand));

        async void ExecuteSaveCommand()
        {
            var items = EvaluateProgramItems;
            if (items == null || items.Count == 0)
            {
                await Crs_DialogEx.MessageBoxShow().GetMessageBoxResultAsync("请先添加方案");
                return;
            }

            if (items.FirstOrDefault(m => m.Mode == EvaluateTestMode.标准评估) != null && items.FirstOrDefault(m => m.Mode != EvaluateTestMode.标准评估) != null)
            {
                await Crs_DialogEx.MessageBoxShow().GetMessageBoxResultAsync($"模式“标准评估”不应跟其他模式混合");
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


        async Task<(bool status, string msg, Program program)> SaveProgramInfo(List<ProgramItem<EvaluateTestMode, EvaluateTestItem>> items, int programId)
        {
            return await Crs_DialogEx.ProgressShow().GetProgressResultAsync<(bool, string, Program)>(async exception =>
            {
                exception.Exception = async ex =>
                {
                    exception.Message = "保存方案错误";
                    return (false, $"{exception.Message},{ex.Message}", null);
                };

                (bool status, List<Module> modules) result_MoCA_MMSE = default;

                if (items.FirstOrDefault(m => m.Mode == EvaluateTestMode.标准评估) != null)
                {
                    var modules = await db.Modules.AsNoTracking().Where(m => m.Type == "eval").ToListAsync();
                    if (modules.FirstOrDefault(m => m.Name?.Trim() == "MoCA") == null)
                    {
                        return (true, "保存方案失败,数据库缺失“MoCA”模块", null);
                    }

                    if (modules.FirstOrDefault(m => m.Name?.Trim() == "MMSE") == null)
                    {
                        return (true, "保存方案失败,数据库缺失“MMSE”模块", null);
                    }

                    result_MoCA_MMSE = (true, modules);
                }

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
                            Eval = true,
                            PatientId = patientId,
                        };

                        db.Programs.Add(program);
                        await db.SaveChangesAsync();
                        db.Entry(program);
                    }
                    else
                    {
                        program.CreateTime ??= DateTime.Now;
                        program.Eval = true;
                        program.PatientId = patientId;

                        db.Programs.Update(program);
                    }

                    programId = program.ProgramId;

                    var oldProgramModules = await db.ProgramModules.Where(m => m.ProgramId == programId).ToListAsync();
                    db.ProgramModules.RemoveRange(oldProgramModules);

                    var oldProgramModulePars = await db.ProgramModulePars.Where(m => m.ProgramId == programId).ToListAsync();
                    db.ProgramModulePars.RemoveRange(oldProgramModulePars);

                    if (result_MoCA_MMSE.status)
                    {
                        var modules = result_MoCA_MMSE.modules;

                        var newProgramModules = modules.Select(m =>
                        {
                            return new ProgramModule
                            {
                                ProgramId = programId,
                                ModuleId = m.ModuleId
                            };
                        }).ToList();
                        db.ProgramModules.AddRange(newProgramModules);
                    }
                    else
                    {
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
                    }

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

        public async override void OnNavigatedTo(NavigationContext navigationContext)
        {
            var parameters = navigationContext.Parameters;

            this.doctor = parameters.GetValue<User>("crs_doctor");
            this.patient = parameters.GetValue<OrganizationPatient>("crs_patient");
            this.programId = parameters.GetValue<int?>("crs_programId");

            var (status, msg, modules, multiItems) = await Crs_DialogEx.ProgressShow().GetProgressResultAsync<(bool, string, List<Module>, List<ProgramModule>)>(async exception =>
            {
                exception.Exception = async ex =>
                {
                    exception.Message = "获取评估测试模块信息错误";
                    return (false, $"{exception.Message},{ex.Message}", null, null);
                };

                var modules = await db.Modules.AsNoTracking()
                    .Where(m => m.Type == "eval").ToListAsync();

                if (programId == null)
                {
                    return (true, null, modules, null);
                }

                var multiItems = await db.ProgramModules.AsNoTracking().Include(m => m.Module).Where(m => m.ProgramId == programId).ToListAsync();

                return (true, null, modules, multiItems);
            });

            if (!status)
            {
                await Crs_DialogEx.MessageBoxShow().GetMessageBoxResultAsync(msg);
                return;
            }

            {
                var items = modules.Select(m =>
                {
                    if (Enum.TryParse<EvaluateTestMode>(m.Name?.Trim(), out var result))
                    {
                        return new EvaluateTestItem { Mode = result }.Update(m);
                    }
                    return null;
                }).Where(m => m != null).ToList();

                items.Insert(0, new EvaluateTestItem { Mode = EvaluateTestMode.标准评估 });
                EvaluateModuleItems = new ObservableCollection<EvaluateTestItem>(items);
            }

            if (multiItems != null)
            {
                var items = multiItems.Select(m =>
                {
                    var module = m.Module;

                    if (!Enum.TryParse<EvaluateTestMode>(module.Name?.Trim(), out var modeResult))
                    {
                        return null;
                    }

                    var evaluateTestItem = new EvaluateTestItem { Mode = modeResult }.Update(module);
                    return evaluateTestItem;
                }).Where(m => m != null).ToList();

                EvaluateProgramItems ??= new ObservableCollection<ProgramItem<EvaluateTestMode, EvaluateTestItem>>();
                EvaluateProgramItems.AddRange(items.Select(m => new ProgramItem<EvaluateTestMode, EvaluateTestItem>
                {
                    Mode = m.Mode,
                    Item = m
                }));

                DiagnosisInfo = string.Join("、", EvaluateModuleItems.Select(m => m.Mode));
                IsFromSchedule = true;
            }

            EvaluateModuleSelectedItem = EvaluateModuleItems.FirstOrDefault();
        }
    }
}
