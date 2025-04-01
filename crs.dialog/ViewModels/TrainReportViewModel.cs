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

namespace crs.dialog.ViewModels
{
    public class TrainReportViewModel : BindableBase, IDialogResultable<object>, IDialogCommon<int?, int?, int?, string, string>
    {
        readonly IRegionManager regionManager;
        readonly IContainerProvider containerProvider;
        readonly IEventAggregator eventAggregator;
        readonly Crs_Db2Context db;

        public TrainReportViewModel() { }
        public TrainReportViewModel(IRegionManager regionManager, IContainerProvider containerProvider, IEventAggregator eventAggregator, Crs_Db2Context db)
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

                var columns = (from item in multiItem.results from _item in item.ResultDetails select _item).OrderBy(m => m.Order).Select(m => m.ValueName?.Trim()).ToHashSet().ToList();
                table.Columns.AddRange(columns.Select(m => new DataColumn(m)).ToArray());

                var chartColumns = new List<(string column, List<ResultDetail> rows)>();

                var groups = (from item in multiItem.results from _item in item.ResultDetails select _item).OrderBy(m => m.Lv).GroupBy(m => m.Lv);
                foreach (var item in groups)
                {
                    var newRow = table.NewRow();
                    foreach (var _item in item)
                    {
                        var valueName = _item.ValueName?.Trim();
                        var value = _item.Value.ToString();
                        newRow[valueName] = value;

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

                var cartesianCharts = chartColumns.Select(m =>
                {
                    var firstItem = m.rows.FirstOrDefault();

                    ICartesianAxis[] xAxes = [new Axis
                    {
                        Labels = [m.column],
                        MinLimit = -(m.rows.Count),
                        MaxLimit = m.rows.Count,
                        MinStep = 1
                    }];

                    ICartesianAxis[] yAxes = [new Axis
                    {
                        MinLimit = firstItem.Minvalue,
                        MaxLimit = firstItem.Maxvalue
                    }];

                    ISeries[] series = firstItem.Charttype switch
                    {
                        "Bar chart" => [new ColumnSeries<double>
                        {
                            Name = m.column,
                            Values = m.rows.Select(m=>m.Value).ToList(),
                            Stroke = null,
                            Fill = new SolidColorPaint(SKColors.CornflowerBlue),
                        }],
                        "Line chart" => [new LineSeries<double>
                        {
                            Name = m.column,
                            Values = m.rows.Select(m=>m.Value).ToList(),
                            Stroke = null,
                            Fill = new SolidColorPaint(SKColors.CornflowerBlue),
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
            }
        }

        public object Result { get; set; }
        public Action CloseAction { get; set; }
    }
}
