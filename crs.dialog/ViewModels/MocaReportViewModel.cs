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
using LiveChartsCore;
using System.Collections.ObjectModel;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using LiveChartsCore.SkiaSharpView.WPF;
using HandyControl.Controls;
using LiveChartsCore.Measure;
using System.Windows;
using System.Windows.Controls;
using Newtonsoft.Json;
using static crs.extension.Crs_Enum;
using System.Text.RegularExpressions;
using Microsoft.Expression.Drawing.Core;
using System.Xaml;

namespace crs.dialog.ViewModels
{
    public class MocaReportViewModel : BindableBase, IDialogResultable<object>, IDialogCommon<int?, int?, int?, string, string>
    {
        readonly IRegionManager regionManager;
        readonly IContainerProvider containerProvider;
        readonly IEventAggregator eventAggregator;
        readonly Crs_Db2Context db;

        public MocaReportViewModel() { }
        public MocaReportViewModel(IRegionManager regionManager, IContainerProvider containerProvider, IEventAggregator eventAggregator, Crs_Db2Context db)
        {
            this.regionManager = regionManager;
            this.containerProvider = containerProvider;
            this.eventAggregator = eventAggregator;
            this.db = db;

            var items = Enumerable.Range(1, 11).ToList();
            subjectItems = new ObservableCollection<SubjectItem>(items.Select(index => new SubjectItem { Name = $"topic{index}" ,StandardType=EvaluateStandardType.MoCAScale}));
        }

        #region Property
        private PatientItem patienttem;
        public PatientItem PatientItem
        {
            get { return patienttem; }
            set { SetProperty(ref patienttem, value); }
        }

        private string dateTime;
        public string DateTime
        {
            get { return dateTime; }
            set { SetProperty(ref dateTime, value); }
        }

        private string durationTime;
        public string DurationTime
        {
            get { return durationTime; }
            set { SetProperty(ref durationTime, value); }
        }

        private ModuleItem moduleItem;
        public ModuleItem ModuleItem
        {
            get { return moduleItem; }
            set { SetProperty(ref moduleItem, value); }
        }

        private CartesianChartItem cartesianChartItem;
        public CartesianChartItem CartesianChartItem
        {
            get { return cartesianChartItem; }
            set { SetProperty(ref cartesianChartItem, value); }
        }

        private ObservableCollection<SubjectItem> subjectItems;
        public ObservableCollection<SubjectItem> SubjectItems
        {
            get { return subjectItems; }
            set { SetProperty(ref subjectItems, value); }
        }

        private string allFraction;
        public string AllFraction
        {
            get { return allFraction; }
            set { SetProperty(ref allFraction, value); }
        }

        private string problem;
        public string Problem
        {
            get { return problem; }
            set { SetProperty(ref problem, value); }
        }

        private string conclusion;
        public string Conclusion
        {
            get { return conclusion; }
            set { SetProperty(ref conclusion, value); }
        }
        #endregion

        private DelegateCommand cancelCommand;
        public DelegateCommand CancelCommand =>
            cancelCommand ?? (cancelCommand = new DelegateCommand(ExecuteCancelCommand));

        void ExecuteCancelCommand()
        {
            CloseAction?.Invoke();
        }

        public async void Execute(int? patientId, int? moduleId, int? resultId, string _dateTime, string _durationTime)
        {
            var (status, msg, multiItem) = await Crs_DialogEx.ProgressShow().GetProgressResultAsync<(bool, string, (OrganizationPatient patient, Module module, List<Result> results, List<EvaluateResult> evaluateResults))>(async exception =>
            {
                exception.Exception = async ex =>
                {
                    exception.Message = "Get report information error";
                    return (false, $"{exception.Message},{ex.Message}", default);
                };

                var patient = await db.OrganizationPatients.AsNoTracking().FirstOrDefaultAsync(m => m.Id == patientId);
                var module = await db.Modules.AsNoTracking().FirstOrDefaultAsync(m => m.ModuleId == moduleId);

                var results = await db.Results.AsNoTracking().Include(m => m.ResultDetails).Where(m => m.ResultId == resultId).ToListAsync();

                var evaluateIds = (from result in results from detail in result.ResultDetails where detail.EvaluateId != null select detail.EvaluateId.Value).Distinct().ToList();
                var evaluateResults = await db.EvaluateResults.Where(m => evaluateIds.Contains(m.EvaluateId)).ToListAsync();

                return (true, null, (patient, module, results, evaluateResults));
            });

            if (!status)
            {
                await Crs_DialogEx.MessageBoxShow().GetMessageBoxResultAsync(msg);
                return;
            }

            if (multiItem.patient != null)
            {
                PatientItem = new PatientItem().Update(multiItem.patient);
            }

            if (multiItem.module != null)
            {
                ModuleItem = new ModuleItem().Update(multiItem.module);
            }

            if (!string.IsNullOrEmpty(_dateTime))
            {
                DateTime = _dateTime;
            }

            if (!string.IsNullOrEmpty(_durationTime))
            {
                DurationTime = _durationTime;
            }

            var result = multiItem.results.FirstOrDefault();
            if (result == null)
            {
                return;
            }

            var pattern = @"^topic\d+\.\d+\.\d+$";
            var resultDetails = result.ResultDetails.Where(m => Regex.IsMatch(m.ValueName, pattern)).ToList();
            var resultDetailGroups = resultDetails.GroupBy(m => m.ValueName.Split('.')[0]).ToList();
            var evaluateResults = multiItem.evaluateResults;
            var subjectItems = SubjectItems;

            foreach (var item in resultDetailGroups)
            {
                var subjectItem = subjectItems.FirstOrDefault(m => m.Name == item.Key);

                if (subjectItem != null)
                {
                    var childrenItems = item.GroupBy(m => m.ValueName.Split('.')[1]).ToList();

                    foreach (var childrenItem in childrenItems)
                    {
                        var childrenIndex = int.Parse(childrenItem.Key) - 1;
                        var subjectChildrenItem = subjectItem.ChildrenItems[childrenIndex];

                        foreach (var detail in childrenItem)
                        {
                            var detailIndex = int.Parse(detail.ValueName.Split(".")[2]) - 1;
                            var answerItem = subjectChildrenItem.AnswerItems[detailIndex];

                            answerItem.IsRight = (int)detail.Value == 1;
                            answerItem.IsWrong = (int)detail.Value == 0;
                            answerItem.Remark = detail.Remark;

                            if (detail.EvaluateId != null)
                            {
                                var evaluateResult = evaluateResults.FirstOrDefault(m => m.EvaluateId == detail.EvaluateId.Value);
                                if (evaluateResult != null)
                                {
                                    switch (evaluateResult.Type)
                                    {
                                        case "IMAGE":
                                            subjectChildrenItem.BitmapData = evaluateResult.Data;
                                            break;
                                        case "AUDIO":
                                            subjectChildrenItem.AudioData = evaluateResult.Data;
                                            break;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            var allAnswerCount = subjectItems.Sum(m => m.AllAnswerCount);
            var rightAnswerCount = subjectItems.Sum(m => m.RightAnswerCount);
            var wrongAnswerCount = subjectItems.Sum(m => m.WrongAnswerCount);

            AllFraction = $"{rightAnswerCount}/{allAnswerCount}";

            var problem = $"MoCAActual scores of the scale{AllFraction}";
            var conclusion = "none";

            var severity = wrongAnswerCount switch
            {
                > 20 => "Heavy",
                > 10 => "Moderate",
                > 0 => "Mild",
                _ => null
            };

            if (severity != null)
            {
                problem = $"{problem}, judged as{severity}Cognitive impairment";

                var problemItems = subjectItems.Where(m => !string.IsNullOrWhiteSpace(m.OriginName)).Where(m => m.WrongAnswerCount > 0).Select(m => m.OriginName).Distinct().ToList();

                problem = $"{problem}, mainly manifested as:{string.Join("、", problemItems)}The functional domain is damaged.";
                conclusion = $"Further evaluation{string.Join("、", problemItems)}";
            }

            Problem = problem;
            Conclusion = conclusion;

            var subjectGroupItems = subjectItems.Where(m => !string.IsNullOrWhiteSpace(m.OriginName)).GroupBy(m => m.OriginName).ToList();

            var cartesianData = new List<(string columnName, double allAnswerCount, double rightAnswerCount)>
            {
                ("Total score", allAnswerCount, rightAnswerCount)
            };
            cartesianData.AddRange(subjectGroupItems.Select(m => (m.Key, (double)m.Sum(n => n.AllAnswerCount), (double)m.Sum(n => n.RightAnswerCount))));

            ICartesianAxis[] xAxes =
                [
                    new Axis
                    {
                        Labels = cartesianData.Select(m=>m.columnName).ToList(),
                        LabelsRotation = 0,
                        MinStep = 1
                    }
                ];

            ICartesianAxis[] yAxes =
                [
                    new Axis
                    {
                        MinLimit = 0,
                        MaxLimit = allAnswerCount + 5
                    }
                ];

            ISeries[] series =
                [
                    new ColumnSeries<double>
                    {
                        Name = "Full marks",
                        Values = cartesianData.Select(m=>m.allAnswerCount).ToList(),
                        Stroke = null,
                        Fill = new SolidColorPaint(SKColors.CornflowerBlue),
                        DataLabelsPaint = new SolidColorPaint(SKColors.CornflowerBlue),
                        DataLabelsPosition = DataLabelsPosition.End
                    },
                    new ColumnSeries<double>
                    {
                        Name = "Actual score",
                        Values = cartesianData.Select(m=>m.rightAnswerCount).ToList(),
                        Stroke = null,
                        Fill = new SolidColorPaint(SKColors.Orange),
                        DataLabelsPaint = new SolidColorPaint(SKColors.Orange),
                        DataLabelsPosition = DataLabelsPosition.End
                    }
                ];

            CartesianChartItem = new CartesianChartItem
            {
                XAxes = xAxes,
                YAxes = yAxes,
                Series = series,
            };
        }

        public object Result { get; set; }
        public Action CloseAction { get; set; }
    }
}
