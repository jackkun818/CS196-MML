using HandyControl.Tools.Extension;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;

namespace crs.dialog.ViewModels
{
    public class LoadingViewModel : BindableBase, IDialogResultable<object>
    {
        public LoadingViewModel()
        {

        }

        public object Result { get; set; }
        public Action CloseAction { get; set; }
    }
}
