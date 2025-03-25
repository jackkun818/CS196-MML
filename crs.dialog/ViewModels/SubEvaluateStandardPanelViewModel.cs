using HandyControl.Tools.Extension;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using static crs.extension.Crs_Interface;

namespace crs.dialog.ViewModels
{
    public class SubEvaluateStandardPanelViewModel : BindableBase, IDialogResultable<object>, IEvaluateStandardHost
    {
        public SubEvaluateStandardPanelViewModel()
        {

        }

        #region Property
        private EvaluateStandardPanelViewModel dataContext;
        public EvaluateStandardPanelViewModel DataContext
        {
            get { return dataContext; }
            set { SetProperty(ref dataContext, value); }
        }
        #endregion

        public void Init(object dataContext)
        {
            DataContext = dataContext as EvaluateStandardPanelViewModel;
        }

        public void Close()
        {
            DataContext = null;
            CloseAction?.Invoke();
        }

        public object Result { get; set; }
        public Action CloseAction { get; set; }
    }
}
