using crs.core.DbModels;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using static crs.extension.Crs_Enum;
using static crs.extension.Models.TrainItem;

namespace crs.extension.Models
{
    public class EvaluateTestItem : BindableBase
    {
        private EvaluateTestMode mode;
        public EvaluateTestMode Mode
        {
            get { return mode; }
            set { SetProperty(ref mode, value); }
        }

        public Module Data { get; private set; }
        public ModuleEx DataEx { get; set; }

        public EvaluateTestItem Update(Module data)
        {
            Data = data;
            DataEx = data.ClassMapper<ModuleEx, Module>();

            return this;
        }
    }
}
