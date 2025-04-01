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
using crs.extension.Controls;

namespace crs.dialog.ViewModels
{
    public class EvaluateReportViewModel : BindableBase, IDialogResultable<object>, IDialogCommon<int?, int?, int?, string, string>
    {
        readonly IRegionManager regionManager;
        readonly IContainerProvider containerProvider;
        readonly IEventAggregator eventAggregator;
        readonly Crs_Db2Context db;

        public EvaluateReportViewModel() { }
        public EvaluateReportViewModel(IRegionManager regionManager, IContainerProvider containerProvider, IEventAggregator eventAggregator, Crs_Db2Context db)
        {
            this.regionManager = regionManager;
            this.containerProvider = containerProvider;
            this.eventAggregator = eventAggregator;
            this.db = db;
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

        private DataTable reportDataTable;
        public DataTable ReportDataTable
        {
            get { return reportDataTable; }
            set { SetProperty(ref reportDataTable, value); }
        }

        private ObservableCollection<KeyValueItem<string, CartesianChartItem>> cartesianChartItems;
        public ObservableCollection<KeyValueItem<string, CartesianChartItem>> CartesianChartItems
        {
            get { return cartesianChartItems; }
            set { SetProperty(ref cartesianChartItems, value); }
        }

        private ObservableCollection<KeyValueItem<string, double>> zValueChartItems;

        public ObservableCollection<KeyValueItem<string, double>> ZValueChartItems
        {
            get { return zValueChartItems; ; }
            set { SetProperty(ref zValueChartItems, value); }
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
            var (status, msg, multiItem) = await Crs_DialogEx.ProgressShow().GetProgressResultAsync<(bool, string, (OrganizationPatient patient, Module module, List<Result> results))>(async exception =>
            {
                exception.Exception = async ex =>
                {
                    exception.Message = "Get report information error";
                    return (false, $"{exception.Message},{ex.Message}", default);
                };

                var patient = await db.OrganizationPatients.AsNoTracking().FirstOrDefaultAsync(m => m.Id == patientId);
                var module = await db.Modules.AsNoTracking().FirstOrDefaultAsync(m => m.ModuleId == moduleId);

                var results = await db.Results.AsNoTracking().Include(m => m.ResultDetails).Where(m => m.ResultId == resultId).ToListAsync();

                return (true, null, (patient, module, results));
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

            if (multiItem.results.Count > 0)
            {
                var table = new DataTable();

                var columns = (from item in multiItem.results from _item in item.ResultDetails select _item)
                    .Where(m => string.IsNullOrWhiteSpace(m.Charttype))
                    .OrderBy(m => m.Order)
                    .Select(m => m.ValueName?.Trim())
                    .ToHashSet().ToList();
                table.Columns.AddRange(columns.Select(m => new DataColumn(m)).ToArray());

                var chartColumns = new List<(string column, List<ResultDetail> rows)>();


                /*DKY
                2025.1.18 New requirements: Table modification of the field of vision module
                Solution:
                Data processing of the field of view module separately
                */
                if (ModuleItem.Name.Equals("Vision"))
                {
                    var table1 = new DataTable();
                    table1.Columns.Add("-");
                    table1.Columns.AddRange(columns.Select(m => m.Split(",")[0])
                        .Distinct()
                        .Select(m => new DataColumn(m)).ToArray());

                    var groups = (from item in multiItem.results from _item in item.ResultDetails select _item)
                    .OrderBy(m => m.Order).GroupBy(m => m.Order / 10);

                    foreach (var item in groups)
                    {
                        //Chart data and non-chart data are processed separately
                        if (string.IsNullOrWhiteSpace(item.FirstOrDefault(m => m.ValueName != null).Charttype))
                        {
                            var newRow = table1.NewRow();

                            newRow["-"] = item.FirstOrDefault(m => m.ValueName != null).ValueName?.Trim().Split(",")[1];
                            foreach (var _item in item)
                            {
                                var valueName = _item.ValueName?.Trim().Split(",")[0];
                                var value = _item.Value.ToString();
                                newRow[valueName] = value;
                            }
                            table1.Rows.Add(newRow);
                        }
                        else
                        {
                            foreach (var _item in item)
                            {
                                var valueName = _item.ValueName?.Trim().Split(",")[0];
                                if (!string.IsNullOrWhiteSpace(_item.Charttype))
                                {
                                    var selectItem = chartColumns.FirstOrDefault(m => m.column == valueName);
                                    if (selectItem == default)
                                    {
                                        selectItem = (valueName, new List<ResultDetail>());
                                        chartColumns.Add(selectItem);
                                    }
                                    selectItem.rows.Add(_item);
                                }
                            }
                        }

                    }

                    ReportDataTable = table1;
                }
                else
                {
                    var groups = (from item in multiItem.results from _item in item.ResultDetails select _item)
                    .OrderBy(m => m.Lv).GroupBy(m => m.Lv);

                    foreach (var item in groups)
                    {
                        var newRow = table.NewRow();
                        foreach (var _item in item)
                        {
                            var valueName = _item.ValueName?.Trim();
                            var value = _item.Value.ToString();
                            if (string.IsNullOrWhiteSpace(_item.Charttype))
                            {
                                newRow[valueName] = value;
                            }

                            if (!string.IsNullOrWhiteSpace(_item.Charttype))
                            {
                                var selectItem = chartColumns.FirstOrDefault(m => m.column == valueName);
                                if (selectItem == default)
                                {
                                    selectItem = (valueName, new List<ResultDetail>());
                                    chartColumns.Add(selectItem);
                                }
                                selectItem.rows.Add(_item);
                            }
                        }
                        table.Rows.Add(newRow);
                    }

                    ReportDataTable = table;

                }

                var cartesianCharts = chartColumns.Select(m =>
                {
                    var firstItem = m.rows.FirstOrDefault();

                    ICartesianAxis[] xAxes = [new Axis
                    {
                        Name = firstItem.ValueName.Split(',')[0],
                        MinLimit = 0,
                        MaxLimit = m.rows.Count,
                        MinStep = 1,

                    }];

                    ICartesianAxis[] yAxes = [new Axis
                    {
                        Name = firstItem.ValueName.Split(',').Length > 1 ? firstItem.ValueName.Split(',')[1] : null,
                        MinLimit = firstItem.Minvalue,
                        MaxLimit = firstItem.Maxvalue
                    }];

                    ISeries[] series = firstItem.Charttype switch
                    {
                        "Bar chart" => [new ColumnSeries<ObservablePoint>
                        {
                            Name = firstItem.ValueName.Split(',').Length > 1 ? firstItem.ValueName.Split(',')[1] : null,
                            Values = m.rows.OrderBy(m=>m.Abscissa).Select(m=>new ObservablePoint{X=m.Abscissa,Y=m.Value}).ToList(),
                            Stroke = null,
                            Fill = new SolidColorPaint(SKColors.CornflowerBlue),

                        }],
                        "Line chart" => [new LineSeries<ObservablePoint>
                        {
                            Name = firstItem.ValueName.Split(',').Length > 1 ? firstItem.ValueName.Split(',')[1] : null,
                            Values = m.rows.OrderBy(m=>m.Abscissa).Select(m=>new ObservablePoint{X=m.Abscissa,Y=m.Value}).ToList(),
                            Stroke = new SolidColorPaint(SKColors.Blue, 2) ,
                            Fill = null,
                            LineSmoothness = 0,
                        }],
                        _ => [new ColumnSeries<double>
                        {
                            Name = m.column,
                            Values = m.rows.Select(m=>m.Value).ToList(),
                            Stroke = null,
                            Fill = new SolidColorPaint(SKColors.CornflowerBlue),
                        }]
                    };

                    return new KeyValueItem<string, CartesianChartItem>
                    {
                        Key = m.column,
                        Value = new CartesianChartItem
                        {
                            XAxes = xAxes,
                            YAxes = yAxes,
                            Series = series,

                        }
                    };
                }).ToList();
                CartesianChartItems = new ObservableCollection<KeyValueItem<string, CartesianChartItem>>(cartesianCharts);

                ZValueChartItems = new ObservableCollection<KeyValueItem<string, double>>(
                    (from item in multiItem.results from _item in item.ResultDetails select _item)
                    .Where(m => m.ValueName.Contains("Zvalue"))
                    .Select(m => new KeyValueItem<string, double>(m.ValueName, Math.Round(m.Value, 2))).ToList());

                string conclusion = "";
                foreach (var item in ZValueChartItems)
                {
                    if (item.Value >= 0)
                    {
                        continue; // Value greater than 0 When performing no operation, just skip
                    }

                    string severity = item.Value < -1 ? "Very bad" : "Poor"; // Less than -1 for“Very bad”，-1 arrive 0 Between“Poor”

                    conclusion += item.Key switch
                    {
                        "ZValue working speed" => $"Working speed{severity}, recommended to use“Space search capability”Carry out training.",
                        "No warning soundZvalue" => $"Response speed{severity}, recommended to use“Response training”Carry out training.",
                        "There is a warning soundZvalue" => $"Response speed{severity}, recommended to use“Alert training”Carry out training.",
                        "ZValue reaction control value" => $"Response ability{severity}, recommended to use“Response behavior”Carry out training.",
                        "ZValue Response Speed ​​Value" => $"Response speed{severity}, recommended to use“Response ability”Carry out training.",
                        "ZNumber of correct answers" => $"Logical ability{severity}, recommended to use“Logical reasoning ability”Carry out training.",
                        "ZValue language learning ability" => $"Word memory ability{severity}, recommended to use“Word memory ability”Carry out training.",
                        "ZValue memory breadth" => $"Memory breadth ability{severity}, recommended to use“Topological memory capability”Carry out training.",
                        _ => "",
                    };
                }

                if (ModuleItem.Name.Equals("Vision") == true)
                {
                    conclusion += "Recommended to use“Visual space training”Carry out training";
                }
                if (conclusion.Equals("") == true)
                {
                    conclusion += "No recommended training program.";
                }

                Conclusion = "suggestion:" + conclusion;
            }
        }

        public object Result { get; set; }
        public Action CloseAction { get; set; }
    }
}
