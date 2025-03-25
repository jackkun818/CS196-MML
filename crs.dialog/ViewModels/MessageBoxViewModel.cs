using crs.theme.Extensions;
using HandyControl.Tools.Extension;
using Microsoft.VisualBasic;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;

namespace crs.dialog.ViewModels
{
    public class MessageBoxViewModel : BindableBase, IDialogResultable<bool?>, IMessageBox
    {
        public MessageBoxViewModel()
        {

        }

        #region Property
        private MessageBoxButton button = MessageBoxButton.OK;
        public MessageBoxButton Button
        {
            get { return button; }
            set { SetProperty(ref button, value); }
        }

        private string message;
        public string Message
        {
            get { return message; }
            set { SetProperty(ref message, value); }
        }
        #endregion

        private DelegateCommand okCommand;
        public DelegateCommand OkCommand =>
            okCommand ?? (okCommand = new DelegateCommand(ExecuteOkCommand));

        void ExecuteOkCommand()
        {
            Result = true;
            CloseAction?.Invoke();
        }

        private DelegateCommand noCommand;
        public DelegateCommand NoCommand =>
            noCommand ?? (noCommand = new DelegateCommand(ExecuteNoCommand));

        void ExecuteNoCommand()
        {
            Result = false;
            CloseAction?.Invoke();
        }

        private DelegateCommand cancelCommand;
        public DelegateCommand CancelCommand =>
            cancelCommand ?? (cancelCommand = new DelegateCommand(ExecuteCancelCommand));

        void ExecuteCancelCommand()
        {
            Result = null;
            CloseAction?.Invoke();
        }

        public bool? Result { get; set; }

        public Action CloseAction { get; set; }
    }
}
