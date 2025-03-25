using crs.core;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace crs.game
{
    public partial class BaseUserControl : UserControl, IGameBase
    {
        Window[] hostWindows;

        public BaseUserControl()
        {
            this.Loaded += BaseUserControl_Loaded;
            this.Unloaded += BaseUserControl_Unloaded;
        }

        private void BaseUserControl_Unloaded(object sender, System.Windows.RoutedEventArgs e)
        {
            if (hostWindows != null)
            {
                foreach (var window in hostWindows)
                {
                    window.KeyDown -= HostWindow_KeyDown;
                }
            }
        }

        private void BaseUserControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            hostWindows ??= App.Current.Windows.Cast<Window>().Where(m => m.Tag?.ToString() == "HostWindow").ToArray();
            if (hostWindows != null)
            {
                foreach (var window in hostWindows)
                {
                    window.KeyDown += HostWindow_KeyDown;
                }
            }
        }

        private void HostWindow_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (!this.IsEnabled)
            {
                return;
            }

            OnHostWindow_KeyDown(sender, e);
        }

        /// <summary>
        /// HostWindow_KeyDown的虚方法，应在子类重载该方法
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void OnHostWindow_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {

        }
    }

    public partial class BaseUserControl : UserControl, IGameBase
    {
        public GameBaseParameter BaseParameter { get; private set; }
        public Action<string> VoiceTipAction { get; set; }
        public Action<string> SynopsisAction { get; set; }
        public Action<string> RuleAction { get; set; }
        public Action<int, int> LevelStatisticsAction { get; set; }
        public Action<int, int> RightStatisticsAction { get; set; }
        public Action<int, int> WrongStatisticsAction { get; set; }
        public Action<int?, int?> TimeStatisticsAction { get; set; }
        Action IGameBase.GameBeginAction { get; set; }
        Action IGameBase.GameEndAction { get; set; }
        Func<string, Task> IGameBase.VoicePlayFunc { get; set; }

        //LJN
        public Action<bool> SetTitleVisibleAction { get; set; }
        //LJN

        /// <summary>
        /// 通知客户端开始游戏
        /// </summary>
        protected void OnGameBegin()
        {
            var action = ((IGameBase)this)?.GameBeginAction;
            action?.Invoke();
        }

        /// <summary>
        /// 通知客户端游戏结束
        /// </summary>
        protected void OnGameEnd()
        {
            // 记录结束时间
            var baseParameter = BaseParameter;
            if (baseParameter != null)
            {
                baseParameter.EndTime ??= DateTime.Now;
            }

            var action = ((IGameBase)this)?.GameEndAction;
            action?.Invoke();
        }

        /// <summary>
        /// 语音播放
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        protected async Task OnVoicePlayAsync(string message)
        {
            var action = ((IGameBase)this)?.VoicePlayFunc;
            await (action?.Invoke(message) ?? Task.CompletedTask);
        }

        async Task IGameBase.InitAsync(int programId, int moduleId, Crs_Db2Context db)
        {
            // 实例化参数类
            var baseParameter = new GameBaseParameter(programId, moduleId, db);

            db = baseParameter.Db;

            // 查询模块参数信息
            baseParameter.ScheduleId = db.Schedules.AsNoTracking().FirstOrDefault(m => m.ProgramId == programId)?.ScheduleId;
            baseParameter.ProgramModulePars = db.ProgramModulePars.AsNoTracking()
                .Include(m => m.ModulePar)
                .ThenInclude(m => m.Module)
                .Where(m => m.ProgramId == programId && m.ModulePar.ModuleId == moduleId).ToList();

            BaseParameter = baseParameter;

            await OnInitAsync();
        }

        /// <summary>
        /// InitAsync(初始化)的虚方法，应在子类重载该方法
        /// </summary>
        /// <returns></returns>
        protected virtual async Task OnInitAsync()
        {

        }

        async Task IGameBase.StartAsync()
        {
            // 记录开始时间
            var baseParameter = BaseParameter;
            if (baseParameter != null)
            {
                baseParameter.BeginTime ??= DateTime.Now;
            }

            await OnStartAsync();
        }

        /// <summary>
        /// StartAsync(开始)的虚方法，应在子类重载该方法
        /// </summary>
        /// <returns></returns>
        protected virtual async Task OnStartAsync()
        {

        }

        async Task IGameBase.StopAsync()
        {
            // 记录结束时间
            var baseParameter = BaseParameter;
            if (baseParameter != null)
            {
                baseParameter.EndTime ??= DateTime.Now;
            }

            await OnStopAsync();
        }

        /// <summary>
        /// StopAsync(结束)的虚方法，应在子类重载该方法
        /// </summary>
        /// <returns></returns>
        protected virtual async Task OnStopAsync()
        {

        }

        async Task IGameBase.PauseAsync()
        {
            await OnPauseAsync();
        }

        /// <summary>
        /// PauseAsync(暂停)的虚方法，应在子类重载该方法
        /// </summary>
        /// <returns></returns>
        protected virtual async Task OnPauseAsync()
        {

        }

        async Task IGameBase.NextAsync()
        {
            await OnNextAsync();
        }

        /// <summary>
        /// NextAsync(下一题)的虚方法，应在子类重载该方法
        /// </summary>
        /// <returns></returns>
        protected virtual async Task OnNextAsync()
        {

        }

        async Task IGameBase.ReportAsync()
        {
            await OnReportAsync();
        }

        /// <summary>
        /// ReportAsync(记录信息)的虚方法，应在子类重载该方法
        /// </summary>
        /// <returns></returns>
        protected virtual async Task OnReportAsync()
        {

        }

        IGameBase IGameBase.GetExplanationExample()
        {
            return OnGetExplanationExample();
        }

        /// <summary>
        /// GetExplanationExample(获取讲解示例)的虚方法，应在子类重载该方法
        /// </summary>
        /// <returns></returns>
        protected virtual IGameBase OnGetExplanationExample()
        {
            return null;
        }
    }
}
