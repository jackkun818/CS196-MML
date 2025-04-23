using crs.core.DbModels;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using static crs.extension.Crs_Enum;

namespace crs.extension.Models
{
    public class ScheduleItem : BindableBase
    {
        private DateTime beginTime;
        public DateTime BeginTime
        {
            get { return beginTime; }
            set { SetProperty(ref beginTime, value); }
        }

        private DateTime endTime;
        public DateTime EndTime
        {
            get { return endTime; }
            set { SetProperty(ref endTime, value); }
        }

        private int day;
        public int Day
        {
            get { return day; }
            set { SetProperty(ref day, value); }
        }

        private ObservableCollection<MultiItem<ScheduleStatus>> statusItems;
        public ObservableCollection<MultiItem<ScheduleStatus>> StatusItems
        {
            get { return statusItems; }
            set { SetProperty(ref statusItems, value); }
        }

        private MultiItem<ScheduleStatus> statusSelectedItem;
        public MultiItem<ScheduleStatus> StatusSelectedItem
        {
            get { return statusSelectedItem; }
            set { SetProperty(ref statusSelectedItem, value); }
        }

        private PatientItem patientItem;
        public PatientItem PatientItem
        {
            get { return patientItem; }
            set { SetProperty(ref patientItem, value); }
        }

        private ProgramType? programType;
        public ProgramType? ProgramType
        {
            get { return programType; }
            set { SetProperty(ref programType, value); }
        }

        private string programContent;
        public string ProgramContent
        {
            get { return programContent; }
            set { SetProperty(ref programContent, value); }
        }

        public (Schedule schedule, OrganizationPatient patient, List<ProgramModule> programModules)? Data { get; private set; }

        public ScheduleItem Update((Schedule schedule, OrganizationPatient patient, List<ProgramModule> programModules) data)
        {
            Data = data;

            var patient = data.patient;
            if (patient != null)
            {
                PatientItem = new PatientItem().Update(data.patient);
            }

            var program = data.schedule?.Program;
            if (program != null)
            {
                ProgramType = program.Eval switch
                {
                    true => Crs_Enum.ProgramType.评估测试,
                    false => Crs_Enum.ProgramType.康复训练,
                    _ => Crs_Enum.ProgramType.康复训练
                };
            }

            var programModules = data.programModules;
            if (programModules != null)
            {
                var modules = programModules.Select(m => m.Module).Where(m => m != null).ToList();
                if (modules != null && modules.Count > 0)
                {
                    if (modules.FirstOrDefault(m => m.Name?.Trim() == "MoCA") != null && modules.FirstOrDefault(m => m.Name?.Trim() == "MMSE") != null)
                    {
                        ProgramContent = EvaluateTestMode.标准评估.ToString();
                    }
                    else
                    {
                        ProgramContent = string.Join("、", modules.Select(m => m.TrainType?.Trim() ?? m.Name?.Trim()));
                    }
                }
            }

            var statusItems = StatusItems;
            if (statusItems != null)
            {
                var status = data.schedule.Status;
                StatusSelectedItem = StatusItems.FirstOrDefault(m => m.Item1.ToString() == status) ?? StatusItems.FirstOrDefault();
            }

            return this;
        }
    }
}