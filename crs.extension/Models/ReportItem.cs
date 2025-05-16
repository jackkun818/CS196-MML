using crs.core.DbModels;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using static crs.extension.Crs_Enum;

namespace crs.extension.Models
{
    public class ReportItem : BindableBase
    {
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

        private string type;
        public string Type
        {
            get { return type; }
            set { SetProperty(ref type, value); }
        }

        private string mode;
        public string Mode
        {
            get { return mode; }
            set { SetProperty(ref mode, value); }
        }

        private string level;
        public string Level
        {
            get { return level; }
            set { SetProperty(ref level, value); }
        }

        private bool isChecked;
        public bool IsChecked
        {
            get { return isChecked; }
            set { SetProperty(ref isChecked, value); }
        }

        public (Module module, Result result)? Data { get; private set; }

        public ReportItem Update((Module module, Result result) data)
        {
            this.Data = data;

            return this;
        }
    }
}
