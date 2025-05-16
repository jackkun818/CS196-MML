using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using static crs.extension.Crs_Enum;

namespace crs.extension.Models
{
    public class AnswerItem : BindableBase
    {
        private bool isRight;
        public bool IsRight
        {
            get { return isRight; }
            set { SetProperty(ref isRight, value); }
        }

        private bool isWrong;
        public bool IsWrong
        {
            get { return isWrong; }
            set { SetProperty(ref isWrong, value); }
        }

        private bool isUse;
        public bool IsUse
        {
            get { return isUse; }
            set { SetProperty(ref isUse, value); }
        }

        private bool ignore;
        public bool Ignore
        {
            get { return ignore; }
            set { SetProperty(ref ignore, value); }
        }

        private string groupName;
        public string GroupName
        {
            get { return groupName; }
            set { SetProperty(ref groupName, value); }
        }

        private string remark;
        public string Remark
        {
            get { return remark; }
            set { SetProperty(ref remark, value); }
        }
    }
}
