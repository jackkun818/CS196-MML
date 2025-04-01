using crs.core.DbModels;
using crs.core;
using Prism.Commands;
using Prism.Events;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Linq;
using crs.extension;
using crs.extension.Models;
using static crs.extension.Crs_Enum;
using System.Collections.ObjectModel;
using crs.theme.Extensions;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using HandyControl.Tools.Extension;
using System.Threading.Tasks;
using System.Reflection.Metadata;

namespace crs.window.ViewModels
{
    public class ReportViewModel : Crs_BindableBase
    {
        readonly IRegionManager regionManager;
        readonly IContainerProvider containerProvider;
        readonly IEventAggregator eventAggregator;
        readonly Crs_Db2Context db;

        User doctor;
        OrganizationPatient patient;

        bool init = false;

        public ReportViewModel() { }
        public ReportViewModel(IRegionManager regionManager, IContainerProvider containerProvider, IEventAggregator eventAggregator, Crs_Db2Context db)
        {
            this.regionManager = regionManager;
            this.containerProvider = containerProvider;
            this.eventAggregator = eventAggregator;
            this.db = db;
        }

        #region Property

        private ObservableCollection<MultiItem<ReportType>> reportTypeItems;
        public ObservableCollection<MultiItem<ReportType>> ReportTypeItems
        {
            get { return reportTypeItems; }
            set { SetProperty(ref reportTypeItems, value); }
        }

        private MultiItem<ReportType> reportTypeSelectedItem;
        public MultiItem<ReportType> ReportTypeSelectedItem
        {
            get { return reportTypeSelectedItem; }
            set { SetProperty(ref reportTypeSelectedItem, value); }
        }

        private ObservableCollection<ReportItem> reportItems;
        public ObservableCollection<ReportItem> ReportItems
        {
            get { return reportItems; }
            set { SetProperty(ref reportItems, value); }
        }

        #endregion

        private DelegateCommand deleteCommand;
        public DelegateCommand DeleteCommand =>
            deleteCommand ?? (deleteCommand = new DelegateCommand(ExecuteDeleteCommand));

        async void ExecuteDeleteCommand()
        {
            var reportItems = ReportItems;
            if (reportItems == null)
            {
                return;
            }

            var _reportItems = reportItems.Where(m => m.IsChecked).ToList();

            if (_reportItems.Count == 0)
            {
                await Crs_DialogEx.MessageBoxShow().GetMessageBoxResultAsync("Please select");
                return;
            }
            else
            {
                foreach (var item in _reportItems)
                {
                    var _report = db.Results.FirstOrDefault(m => m.ProgramId == item.Data.Value.result.ProgramId);

                    var _ResultDetailsList = db.ResultDetails.Where(m => m.ResultId == item.Data.Value.result.ResultId).ToList();
                    foreach (var resultDetail in _ResultDetailsList)
                    {
                        db.ResultDetails.Remove(resultDetail);
                    }
                    await db.SaveChangesAsync();

                    db.Results.Remove(_report);
                    await db.SaveChangesAsync();

                    var programItem = db.Programs.FirstOrDefault(m => m.ProgramId == item.Data.Value.result.ProgramId);
                    db.Programs.Remove(programItem);
                    await db.SaveChangesAsync();

                    reportItems.Remove(item);
                }

                return;
            }
        }

        private DelegateCommand<ReportItem> viewCommand;
        public DelegateCommand<ReportItem> ViewCommand =>
            viewCommand ?? (viewCommand = new DelegateCommand<ReportItem>(ExecuteViewCommand));

        async void ExecuteViewCommand(ReportItem parameter)
        {
            var selectedItem = ReportTypeSelectedItem;
            if (parameter == null || selectedItem == null)
            {
                return;
            }

            var viewName = (selectedItem.Item1, parameter.Mode) switch
            {
                (ReportType.Evaluation report, "MoCA") => Crs_Dialog.MocaReport,
                (ReportType.Evaluation report, "MMSE") => Crs_Dialog.MmseReport,
                (ReportType.Evaluation report, _) => Crs_Dialog.EvaluateReport,
                (ReportType.Training Report, _) => Crs_Dialog.TrainReport,
                _ => throw new NotImplementedException()
            };

            var patientId = patient.Id;
            var moduleId = parameter.Data.Value.module.ModuleId;
            var resultId = parameter.Data.Value.result.ResultId;
            var dateTime = parameter.DateTime;
            var durationTime = parameter.DurationTime;

            await Crs_DialogEx.Show(viewName, Crs_DialogToken.TopContent)
                .UseConfig_ContentStretch()
                .Initialize<IDialogCommon<int?, int?, int?, string, string>>(vm => vm.Execute(patientId, moduleId, resultId, dateTime, durationTime))
                .GetResultAsync<object>();
        }

        private DelegateCommand reportTypeSelectedItemChangedCommand;
        public DelegateCommand ReportTypeSelectedItemChangedCommand =>
            reportTypeSelectedItemChangedCommand ?? (reportTypeSelectedItemChangedCommand = new DelegateCommand(ExecuteReportTypeSelectedItemChangedCommand));

        async void ExecuteReportTypeSelectedItemChangedCommand()
        {
            var selectedItem = ReportTypeSelectedItem;
            if (selectedItem == null || !init)
            {
                return;
            }

            if (patient == null)
            {
                await Crs_DialogEx.MessageBoxShow().GetMessageBoxResultAsync("The current patient information is empty");
                return;
            }

            ReportItems = null;
            var (status, msg, results) = await Crs_DialogEx.ProgressShow().GetProgressResultAsync<(bool, string, List<(Program program, Result result, Module module, List<ResultDetail> resultDetails)>)>(async exception =>
            {
                exception.Exception = async ex =>
                {
                    exception.Message = "Obtain data report information error";
                    return (false, $"{exception.Message},{ex.Message}", null);
                };


                var patientId = patient.Id;

                // Query the database

                var programQueryable = selectedItem.Item1 switch
                {
                    ReportType.Evaluation report => db.Programs.AsNoTracking().Where(m => m.PatientId == patientId && m.Eval == true),
                    ReportType.Training Report => db.Programs.AsNoTracking().Where(m => m.PatientId == patientId && (m.Eval == null || m.Eval == false)),
                    _ => throw new NotImplementedException()
                };

                var _results = await (from program in programQueryable
                                      join result in db.Results.AsNoTracking() on program.ProgramId equals result.ProgramId into results
                                      from result in results.DefaultIfEmpty()
                                      join resultDetail in db.ResultDetails.AsNoTracking() on result.ResultId equals resultDetail.ResultId into resultDetails
                                      from resultDetail in resultDetails.DefaultIfEmpty()
                                      join module in db.Modules.AsNoTracking() on resultDetail.ModuleId equals module.ModuleId into modules
                                      from module in modules.DefaultIfEmpty()
                                      where result != null && resultDetail != null && module != null
                                      select new { program, result, resultDetail, module })
                                      .OrderByDescending(m => m.program.CreateTime)
                                      .ToListAsync();

                var _results_ = _results
                    .GroupBy(m => (m.program.ProgramId, m.result.ResultId, m.module.ModuleId))
                    .Select(m =>
                    {
                        var firstItem = m.FirstOrDefault();
                        return (firstItem.program, firstItem.result, firstItem.module, m.Select(m => m.resultDetail).ToList());
                    })
                    .ToList();

                return (true, null, _results_);
            });

            if (!status)
            {
                await Crs_DialogEx.MessageBoxShow().GetMessageBoxResultAsync(msg);
                return;
            }

            var items = results.Select(m =>
            {
                /*
                 Only the start time is displayed on the report, but no end time is required.
                 */
                var dateTime = $"{m.program.ActStartTime?.ToString("yyyy-MM-dd HH:mm")}";
                //var dateTime = $"{(m.program.ActStartTime?.ToString("yyyy-MM-dd HH:mm") ?? "--")}~{(m.program.ActEndTime?.ToString("HH:mm") ?? "--")}";
                string durationTime = null;
                if (m.program.ActStartTime != null && m.program.ActEndTime != null)
                {
                    durationTime = $"{(m.program.ActEndTime.Value - m.program.ActStartTime.Value).ToString(@"hh\:mm\:ss")}";
                }

                return new ReportItem
                {
                    DateTime = dateTime,
                    DurationTime = durationTime,
                    Type = m.module.TrainType?.Trim(),
                    Mode = m.module.Name?.Trim(),
                    Level = m.result.Lv?.ToString()
                }.Update((m.module, m.result));
            }).ToList();

            ReportItems = new ObservableCollection<ReportItem>(items);
        }

        public override void OnNavigatedTo(NavigationContext navigationContext)
        {
            var parameters = navigationContext.Parameters;

            this.doctor = parameters.GetValue<User>("crs_doctor");
            this.patient = parameters.GetValue<OrganizationPatient>("crs_patient");

            var items = Enum.GetValues<ReportType>().Cast<ReportType>().Select(m => new MultiItem<ReportType> { Item1 = m });

            ReportTypeItems = new ObservableCollection<MultiItem<ReportType>>(items);
            ReportTypeSelectedItem = ReportTypeItems.FirstOrDefault();

            init = true;
            ReportTypeSelectedItemChangedCommand?.Execute();
        }
    }
}
