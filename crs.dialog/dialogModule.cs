using crs.dialog.Views;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;
using crs.extension;
using crs.dialog.ViewModels;

namespace crs.dialog
{
    public class dialogModule : IModule
    {
        public void OnInitialized(IContainerProvider containerProvider)
        {

        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            // 注册视图
            containerRegistry.RegisterForNavigation<Loading>(Crs_Dialog.Loading);
            containerRegistry.RegisterForNavigation<MessageBox>(Crs_Dialog.MessageBox);
            containerRegistry.RegisterForNavigation<PatientEdit>(Crs_Dialog.PatientEdit);
            containerRegistry.RegisterForNavigation<DigitalHumanSelected>(Crs_Dialog.DigitalHumanSelected);
            containerRegistry.RegisterForNavigation<DigitalHumanEdit>(Crs_Dialog.DigitalHumanEdit);
            containerRegistry.RegisterForNavigation<EvaluateGamePanel, EvaluateGamePanelViewModel>(Crs_Dialog.EvaluateGamePanel);
            containerRegistry.RegisterForNavigation<TrainGamePanel, TrainGamePanelViewModel>(Crs_Dialog.TrainGamePanel);
            containerRegistry.RegisterForNavigation<SubGamePanel>(Crs_Dialog.SubGamePanel);
            containerRegistry.RegisterForNavigation<EvaluateReport>(Crs_Dialog.EvaluateReport);
            containerRegistry.RegisterForNavigation<TrainReport>(Crs_Dialog.TrainReport);
            containerRegistry.RegisterForNavigation<MocaReport>(Crs_Dialog.MocaReport);
            containerRegistry.RegisterForNavigation<MmseReport>(Crs_Dialog.MmseReport);
            containerRegistry.RegisterForNavigation<EvaluateStandardPanel>(Crs_Dialog.EvaluateStandardPanel);
            containerRegistry.RegisterForNavigation<SubEvaluateStandardPanel>(Crs_Dialog.SubEvaluateStandardPanel);
            containerRegistry.RegisterForNavigation<SettingPanel>(Crs_Dialog.SettingPanel);
            containerRegistry.RegisterForNavigation<AboutUsDialog>(Crs_Dialog.AboutUsDialog);
            containerRegistry.RegisterForNavigation<AccountManageDialog>(Crs_Dialog.AccountManageDialog);
            containerRegistry.RegisterForNavigation<ReportSettingDialog>(Crs_Dialog.ReportSettingDialog);
        }
    }
}