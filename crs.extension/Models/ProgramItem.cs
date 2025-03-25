using crs.core.DbModels;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using static crs.extension.Crs_Enum;

namespace crs.extension.Models
{
    public class ProgramItem<TEnum, TItem> : BindableBase where TEnum : Enum where TItem : class
    {
        private TEnum mode;
        public TEnum Mode
        {
            get { return mode; }
            set { SetProperty(ref mode, value); }
        }

        private TItem item;
        public TItem Item
        {
            get { return item; }
            set { SetProperty(ref item, value); }
        }

        private DateTime? createTime;
        public DateTime? CreateTime
        {
            get { return createTime; }
            set { SetProperty(ref createTime, value); }
        }

        public Program Data { get; private set; }

        public ProgramItem<TEnum, TItem> Update(Program data)
        {
            Data = data;

            return this;
        }
    }
}
