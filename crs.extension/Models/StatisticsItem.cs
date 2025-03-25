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
    public class StatisticsItem : BindableBase
    {
        private int count;
        public int Count
        {
            get { return count; }
            set { SetProperty(ref count, value); }
        }

        private int _value;
        public int Value
        {
            get { return _value; }
            set { SetProperty(ref _value, value); }
        }
    }
}
