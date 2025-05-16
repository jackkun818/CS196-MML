using crs.core.DbModels;
using Microsoft.Identity.Client.NativeInterop;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json.Nodes;

namespace crs.extension.Models
{
    public class ModuleParItem : BindableBase
    {
        private string name;
        public string Name
        {
            get { return name; }
            set { SetProperty(ref name, value); }
        }

        private double minValue;
        public double MinValue
        {
            get { return minValue; }
            set { SetProperty(ref minValue, value); }
        }

        private double maxValue;
        public double MaxValue
        {
            get { return maxValue; }
            set { SetProperty(ref maxValue, value); }
        }

        private double defaultValue;
        public double DefaultValue
        {
            get { return defaultValue; }
            set { SetProperty(ref defaultValue, value); }
        }

        private double stepSize;
        public double StepSize
        {
            get { return stepSize; }
            set { SetProperty(ref stepSize, value); }
        }

        private string unit;
        public string Unit
        {
            get { return unit; }
            set { SetProperty(ref unit, value); }
        }

        private bool isChecked;
        public bool IsChecked
        {
            get { return isChecked; }
            set { SetProperty(ref isChecked, value); }
        }

        public ModulePar Data { get; private set; }
        public ModuleParEx DataEx { get; set; }
        public bool IsFeedbackType { get; set; }

        public ModuleParItem Update(ModulePar data, double? defaultValue = null, bool? isChecked = null)
        {
            Data = data;
            DataEx = data.ClassMapper<ModuleParEx, ModulePar>();

            Name = data.Name?.Trim();
            DefaultValue = defaultValue ?? data.DefaultValue;

            MinValue = Math.Max(data.MinValue ?? 0d, 0d);
            MaxValue = Math.Max(data.MaxValue ?? 5000d, 0d);
            StepSize = Math.Max(data.Interval ?? 1d, 0d);
            Unit = data.Unit?.Trim();

            if (data?.FeedbackType != null)
            {
                IsFeedbackType = true;
                IsChecked = isChecked ?? data.DefaultValue == 1d;
            }

            return this;
        }
    }
}
