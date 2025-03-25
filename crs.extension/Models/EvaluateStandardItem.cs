using crs.core.DbModels;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using static crs.extension.Crs_Enum;

namespace crs.extension.Models
{
    public class EvaluateStandardItem : BindableBase
    {
        private EvaluateStandardType standardType;
        public EvaluateStandardType StandardType
        {
            get { return standardType; }
            set { SetProperty(ref standardType, value); }
        }

        private bool isComplete;
        public bool IsComplete
        {
            get { return isComplete; }
            set { SetProperty(ref isComplete, value); }
        }

        public List<SubjectItem> SubjectItems { get; set; }
        public Module Data { get; private set; }

        public EvaluateStandardItem Update(Module data)
        {
            Data = data;

            return this;
        }
    }
}
