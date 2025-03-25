using crs.extension.Models;
using crs.theme.Extensions;
using HandyControl.Tools.Extension;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace crs.dialog.ViewModels
{
    public class AboutUsDialogViewModel : BindableBase, IDialogResultable<object>, IDialogCommon
    {
        public AboutUsDialogViewModel() { }


        private DelegateCommand cancelCommand;
        public DelegateCommand CancelCommand =>
            cancelCommand ?? (cancelCommand = new DelegateCommand(ExecuteCancelCommand));
        void ExecuteCancelCommand()
        {
            CloseAction?.Invoke();
        }

        public async void Execute()
        {


        }

        public object Result { get; set; }
        public Action CloseAction { get; set; }
    }
}
