using crs.core;
using crs.core.DbModels;
using crs.extension;
using crs.extension.Controls;
using crs.extension.Models;
using crs.game;
using crs.theme.Extensions;
using HandyControl.Controls;
using HandyControl.Tools;
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
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using static crs.extension.Crs_Enum;
using static crs.extension.Crs_Interface;
using static SkiaSharp.HarfBuzz.SKShaper;
using MessageBoxButton = crs.theme.Extensions.MessageBoxButton;
using Result = crs.core.DbModels.Result;

namespace crs.dialog.ViewModels
{
    public class EvaluateStandardPanelViewModel : BindableBase, IDialogResultable<bool>, IDialogCommon<int?>
    {
        readonly IRegionManager regionManager;
        readonly IContainerProvider containerProvider;
        readonly IEventAggregator eventAggregator;
        readonly Crs_Db2Context db;

        bool init = false;
        int? programId;
        int? scheduleId;

        public EvaluateStandardPanelViewModel() { }
        public EvaluateStandardPanelViewModel(IRegionManager regionManager, IContainerProvider containerProvider, IEventAggregator eventAggregator, Crs_Db2Context db)
        {
            this.regionManager = regionManager;
            this.containerProvider = containerProvider;
            this.eventAggregator = eventAggregator;
            this.db = db;
        }

        #region Property

        private IEvaluateStandardHost standardHost;
        public IEvaluateStandardHost StandardHost
        {
            get { return standardHost; }
            set { SetProperty(ref standardHost, value); }
        }

        private ObservableCollection<EvaluateStandardItem> evaluateStandardItems;
        public ObservableCollection<EvaluateStandardItem> EvaluateStandardItems
        {
            get { return evaluateStandardItems; }
            set { SetProperty(ref evaluateStandardItems, value); }
        }

        private EvaluateStandardItem evaluateStandardSelectedItem;
        public EvaluateStandardItem EvaluateStandardSelectedItem
        {
            get { return evaluateStandardSelectedItem; }
            set { SetProperty(ref evaluateStandardSelectedItem, value); }
        }

        private ObservableCollection<SubjectItem> subjectItems;
        public ObservableCollection<SubjectItem> SubjectItems
        {
            get { return subjectItems; }
            set { SetProperty(ref subjectItems, value); }
        }

        private SubjectItem subjectSelectedItem;
        public SubjectItem SubjectSelectedItem
        {
            get { return subjectSelectedItem; }
            set { SetProperty(ref subjectSelectedItem, value); }
        }

        private int subjectSelectedIndex;
        public int SubjectSelectedIndex
        {
            get { return subjectSelectedIndex; }
            set { SetProperty(ref subjectSelectedIndex, value); }
        }


        private Visibility therapistScoringStatus;
        public Visibility TherapistScoringStatus
        {
            get { return therapistScoringStatus; }
            set { SetProperty(ref therapistScoringStatus, value); }
        }

        private Visibility nextButtonVisibilityStatus;
        public Visibility NextButtonVisibilityStatus
        {
            get { return nextButtonVisibilityStatus; }
            set { SetProperty(ref nextButtonVisibilityStatus, value); }
        }

        private bool nextButtonIsEnabledStatus;
        public bool NextButtonIsEnabledStatus
        {
            get { return nextButtonIsEnabledStatus; }
            set { SetProperty(ref nextButtonIsEnabledStatus, value); }
        }

        private SubjectItem subPanelSubjectSelectedItem;
        public SubjectItem SubPanelSubjectSelectedItem
        {
            get { return subPanelSubjectSelectedItem; }
            set { SetProperty(ref subPanelSubjectSelectedItem, value); }
        }

        private int subPanelSubjectSelectedItemIndex;
        public int SubPanelSubjectSelectedItemIndex
        {
            get { return subPanelSubjectSelectedItemIndex; }
            set { SetProperty(ref subPanelSubjectSelectedItemIndex, value); }
        }

        private Crs_Carousel subPanelCarousel;
        public Crs_Carousel SubPanelCarousel
        {
            get { return subPanelCarousel; }
            set { SetProperty(ref subPanelCarousel, value); }
        }

        #endregion

        private DelegateCommand cancelCommand;
        public DelegateCommand CancelCommand =>
            cancelCommand ?? (cancelCommand = new DelegateCommand(ExecuteCancelCommand));

        async void ExecuteCancelCommand()
        {
            if (await Crs_DialogEx.MessageBoxShow().GetMessageBoxResultAsync("评估还未完成，是否退出？", button: MessageBoxButton.OKOrCancel) == null)
            {
                return;
            }

            var program = await db.Programs.FirstOrDefaultAsync(m => m.ProgramId == programId);
            if (program != null)
            {
                program.ActEndTime = DateTime.Now;
                db.Programs.Update(program);
                db.SaveChanges();
            }

            var standardHost = StandardHost;
            standardHost?.Close();

            Result = false;
            CloseAction?.Invoke();
        }

        private DelegateCommand lastCommand;
        public DelegateCommand LastCommand =>
            lastCommand ?? (lastCommand = new DelegateCommand(ExecuteLastCommand));

        async void ExecuteLastCommand()
        {
            var selectedItem = SubjectSelectedItem;
            if (selectedItem == null || !init || selectedItem.IsFirst)
            {
                return;
            }

            //var result = SubjectSelectedItemCheck(selectedItem);
            //if (!result.status)
            //{
            //    await Crs_DialogEx.MessageBoxShow().GetMessageBoxResultAsync(result.msg);
            //    return;
            //}

            SubjectSelectedIndex--;
        }

        private DelegateCommand nextCommand;
        public DelegateCommand NextCommand =>
            nextCommand ?? (nextCommand = new DelegateCommand(ExecuteNextCommand));

        async void ExecuteNextCommand()
        {
            var selectedItem = SubjectSelectedItem;
            if (selectedItem == null || !init || selectedItem.IsLast)
            {
                return;
            }

            if (SubPanelSubjectSelectedItem != null && SubPanelSubjectSelectedItemIndex == 0)
            {
                var result = SubjectSelectedItemCheck(SubPanelSubjectSelectedItem);
                if (!result.status)
                {
                    await Crs_DialogEx.MessageBoxShow().GetMessageBoxResultAsync(result.msg);
                    return;
                }
            }
            else if (SubPanelSubjectSelectedItem != null)
            {
                var result = SubjectSelectedItemCheck(SubPanelSubjectSelectedItem, SubPanelSubjectSelectedItem.ChildrenItems[SubPanelSubjectSelectedItemIndex - 1]);
                if (!result.status)
                {
                    await Crs_DialogEx.MessageBoxShow().GetMessageBoxResultAsync(result.msg);
                    return;
                }
            }
            //var result = SubjectSelectedItemCheck(selectedItem);
            //if (!result.status)
            //{
            //    await Crs_DialogEx.MessageBoxShow().GetMessageBoxResultAsync(result.msg);
            //    return;
            //}

            TherapistScoringStatus = Visibility.Collapsed;
            NextButtonVisibilityStatus = Visibility.Collapsed;
            NextButtonIsEnabledStatus = false;

            if (SubPanelSubjectSelectedItem != null && (SubPanelSubjectSelectedItemIndex == 0 || SubPanelSubjectSelectedItem.ChildrenItems.Where(m => m.IsUse).ToList().Count == SubPanelSubjectSelectedItemIndex))
            {
                SubjectSelectedIndex++;
            }
            if (SubPanelCarousel != null)
            {
                if (SubPanelCarousel.PageIndex < SubPanelCarousel.Items.Count - 1)
                {
                    SubPanelCarousel.PageIndex = SubPanelCarousel.PageIndex + 1;
                }
            }

            //SubjectSelectedIndex++;
        }

        public (bool status, string msg) SubjectSelectedItemCheck(SubjectItem subjectItem, SubjectChildrenItem subjectChildrenItem = null)
        {
            var childrenItems = subjectItem.ChildrenItems.Where(m => m.IsUse).ToList();
            foreach (var item in childrenItems)
            {
                if (subjectChildrenItem != null && !item.Equals(subjectChildrenItem))
                {
                    continue;
                }

                string topicName = null;
                var index = childrenItems.IndexOf(item);

                if (childrenItems.Count > 1)
                {
                    topicName = $"-{index + 1}";
                }

                //if ((item.IsUseAudio && item.AudioData == null) || (item.IsUseBitmap && item.BitmapData == null))
                //{
                //    return (false, $"患者未完成“{subjectItem.Name}{topicName}”答题");
                //}

                var answerItems = (from answerItem in item.AnswerItems.Where(m => m.IsUse) select answerItem).ToList();
                var answerItemGroups = answerItems.GroupBy(m => m.GroupName).ToList();

                foreach (var groupItem in answerItemGroups)
                {
                    bool isFind = false;
                    if (groupItem.Key == null)
                    {
                        foreach (var _item in groupItem)
                        {
                            if (!_item.IsRight && !_item.IsWrong)
                            {
                                isFind = true;
                                break;
                            }
                        }
                    }
                    else
                    {
                        bool isUnFind = false;
                        foreach (var _item in groupItem)
                        {
                            if (_item.IsRight || _item.IsWrong)
                            {
                                isUnFind = true;
                                break;
                            }
                        }
                        isFind = !isUnFind;
                    }

                    if (isFind)
                    {
                        return (false, $"治疗师未完成“{subjectItem.Name}{topicName}”打分");
                    }
                }
            }
            return (true, null);
        }

        private DelegateCommand completeCommand;
        public DelegateCommand CompleteCommand =>
            completeCommand ?? (completeCommand = new DelegateCommand(ExecuteCompleteCommand));

        async void ExecuteCompleteCommand()
        {
            var selectedItem = EvaluateStandardSelectedItem;
            if (selectedItem == null)
            {
                return;
            }

            var subjectItems = selectedItem.SubjectItems;
            foreach (var subjectItem in subjectItems)
            {
                var result = SubjectSelectedItemCheck(subjectItem);
                if (!result.status)
                {
                    await Crs_DialogEx.MessageBoxShow().GetMessageBoxResultAsync(result.msg);
                    return;
                }
            }

            if (programId == null)
            {
                await Crs_DialogEx.MessageBoxShow().GetMessageBoxResultAsync("方案ID为空");
                return;
            }

            var module = selectedItem.Data;
            if (module == null)
            {
                await Crs_DialogEx.MessageBoxShow().GetMessageBoxResultAsync("模块信息为空");
                return;
            }

            bool? completeButtonResult;
            if ((completeButtonResult = await Crs_DialogEx.MessageBoxShow().GetMessageBoxResultAsync("是否结束并生成报告？", button: MessageBoxButton.CustomReport)) == null)
            {
                return;
            }

            if (!completeButtonResult.Value)
            {
                await Crs_DialogEx.MessageBoxShow().GetMessageBoxResultAsync("评估结束");

                var _standardHost = StandardHost;
                _standardHost?.Close();

                Result = true;
                CloseAction?.Invoke();
                return;
            }

            var (status, msg) = await Crs_DialogEx.ProgressShow().GetProgressResultAsync<(bool, string)>(async exception =>
            {
                exception.Exception = async ex =>
                {
                    exception.Message = "保存结果错误";
                    return (false, $"{exception.Message},{ex.Message}");
                };

                var transaction = await db.Database.BeginTransactionAsync();
                try
                {
                    var moduleName = module.Name?.Trim();
                    var result = new Result
                    {
                        ProgramId = programId.Value,
                        Report = moduleName,
                        Eval = true,
                        ScheduleId = scheduleId
                    };

                    db.Results.Add(result);
                    await db.SaveChangesAsync();
                    db.Entry(result);

                    var resultId = result.ResultId;

                    foreach (var subjectItem in subjectItems)
                    {
                        var childrenItems = subjectItem.ChildrenItems.Where(m => m.IsUse).ToList();
                        foreach (var childrenItem in childrenItems)
                        {
                            (string type, byte[] data) resource = default;
                            switch (childrenItem.IsUseBitmap, childrenItem.BitmapData, childrenItem.IsUseAudio, childrenItem.AudioData)
                            {
                                case (true, not null, _, _):
                                    resource = ("IMAGE", childrenItem.BitmapData);
                                    break;
                                case (_, _, true, not null):
                                    resource = ("AUDIO", childrenItem.AudioData);
                                    break;
                            }

                            EvaluateResult evaluateResult = null;
                            if (resource != default)
                            {
                                evaluateResult = new EvaluateResult
                                {
                                    Type = resource.type,
                                    Data = resource.data,
                                };

                                db.EvaluateResults.Add(evaluateResult);
                                await db.SaveChangesAsync();
                                db.Entry(evaluateResult);
                            }

                            var evaluateId = evaluateResult?.EvaluateId;
                            var childrenItemIndex = childrenItems.IndexOf(childrenItem) + 1;

                            var answerItems = childrenItem.AnswerItems.Where(m => m.IsUse).ToList();
                            foreach (var answerItem in answerItems)
                            {
                                var answerItemIndex = answerItems.IndexOf(answerItem) + 1;
                                var valueName = $"{subjectItem.Name}.{childrenItemIndex}.{answerItemIndex}";
                                var value = answerItem.IsRight ? 1 : answerItem.IsWrong ? 0 : -1;
                                var remark = answerItem.Remark;

                                var resultDetail = new ResultDetail
                                {
                                    ResultId = resultId,
                                    ModuleId = module.ModuleId,
                                    ValueName = valueName,
                                    Remark = remark,
                                    Value = value,
                                    EvaluateId = evaluateId,
                                };
                                db.ResultDetails.Add(resultDetail);
                            }
                        }
                    }

                    await db.SaveChangesAsync();
                    await transaction.CommitAsync();

                    selectedItem.IsComplete = true;
                    foreach (var item in subjectItems)
                    {
                        item.IsComplete = true;
                    }

                    return (true, "保存结果成功");
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    throw new Exception(ex.Message, ex);
                }
            });

            await Crs_DialogEx.MessageBoxShow().GetMessageBoxResultAsync(msg);
            if (!status)
            {
                return;
            }

            selectedItem = EvaluateStandardItems.FirstOrDefault(m => !m.IsComplete);
            if (selectedItem != null)
            {
                EvaluateStandardSelectedItem = selectedItem;
                return;
            }

            await Crs_DialogEx.MessageBoxShow().GetMessageBoxResultAsync("评估结束");

            var program = await db.Programs.FirstOrDefaultAsync(m => m.ProgramId == programId);
            if (program != null)
            {
                program.ActEndTime = DateTime.Now;
                db.Programs.Update(program);
                db.SaveChanges();
            }

            var standardHost = StandardHost;
            standardHost?.Close();

            Result = true;
            CloseAction?.Invoke();
        }

        private DelegateCommand evaluateStandardSelectedChangedCommand;
        public DelegateCommand EvaluateStandardSelectedChangedCommand =>
            evaluateStandardSelectedChangedCommand ?? (evaluateStandardSelectedChangedCommand = new DelegateCommand(ExecuteEvaluateStandardSelectedChangedCommand));

        async void ExecuteEvaluateStandardSelectedChangedCommand()
        {
            var selectedItem = EvaluateStandardSelectedItem;
            if (selectedItem == null || !init)
            {
                return;
            }

            SubjectItems = null;
            await Task.Yield();

            var _items = selectedItem.SubjectItems;
            if (_items == null)
            {
                var items = selectedItem.StandardType switch
                {
                    EvaluateStandardType.MoCA量表 => Enumerable.Range(1, 11).ToList(),
                    EvaluateStandardType.MMSE量表 => Enumerable.Range(1, 11).ToList(),
                    _ => throw new NotImplementedException()
                };

                _items = items.Select(index =>
                {
                    var isFirst = items.FirstOrDefault() == index;
                    var isLast = items.LastOrDefault() == index;

                    return new SubjectItem
                    {
                        Name = $"题目{index}",
                        StandardType = selectedItem.StandardType,
                        IsFirst = isFirst,
                        IsLast = isLast,
                    };
                }).ToList();
                selectedItem.SubjectItems = _items;
            }

            SubjectItems = new ObservableCollection<SubjectItem>(_items);
            SubjectSelectedItem = SubjectItems.FirstOrDefault();
        }

        public async void Execute(int? programId)
        {
            this.programId = programId;

            var (status, msg, multiItems) = await Crs_DialogEx.ProgressShow().GetProgressResultAsync<(bool, string, List<ProgramModule>)>(async exception =>
            {
                exception.Exception = async ex =>
                {
                    exception.Message = "获取方案信息错误";
                    return (false, $"{exception.Message},{ex.Message}", null);
                };

                var program = await db.Programs.FirstOrDefaultAsync(m => m.ProgramId == programId);
                if (program != null)
                {
                    program.ActStartTime = DateTime.Now;
                    db.Programs.Update(program);
                    db.SaveChanges();
                }

                var schedule = await db.Schedules.FirstOrDefaultAsync(m => m.ProgramId == programId);
                this.scheduleId = schedule?.ScheduleId;

                var multiItems = await db.ProgramModules.AsNoTracking().Include(m => m.Module).Where(m => m.ProgramId == programId).ToListAsync();

                return (true, null, multiItems);
            });

            if (!status)
            {
                await Crs_DialogEx.MessageBoxShow().GetMessageBoxResultAsync(msg);
                return;
            }

            var items = Enum.GetValues<EvaluateStandardType>().Cast<EvaluateStandardType>().Select(m =>
            {
                var type = m switch
                {
                    EvaluateStandardType.MoCA量表 => "MoCA",
                    EvaluateStandardType.MMSE量表 => "MMSE",
                    _ => null
                };

                var module = multiItems.FirstOrDefault(m => m.Module?.Name?.Trim() == type)?.Module;

                return new EvaluateStandardItem { StandardType = m, }.Update(module);
            });

            EvaluateStandardItems = new ObservableCollection<EvaluateStandardItem>(items);
            EvaluateStandardSelectedItem = EvaluateStandardItems.FirstOrDefault();

            IEvaluateStandardHost standardHost = null;
            _ = Crs_DialogEx.Show(Crs_Dialog.SubEvaluateStandardPanel, Crs_DialogToken.SubTopContent)
                .UseConfig_ContentStretch()
                .Initialize<IEvaluateStandardHost>(vm => standardHost = vm)
                .GetResultAsync<object>();

            standardHost.Init(this);
            StandardHost = standardHost;

            init = true;
            EvaluateStandardSelectedChangedCommand?.Execute();

            TherapistScoringStatus = Visibility.Collapsed;

            NextButtonVisibilityStatus = Visibility.Collapsed;
            NextButtonIsEnabledStatus = false;


        }

        public bool Result { get; set; }
        public Action CloseAction { get; set; }
    }
}
