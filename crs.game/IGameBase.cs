using crs.core;
using crs.core.DbModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace crs.game
{
    /// <summary>
    /// 参数
    /// </summary>
    public class GameBaseParameter
    {
        /// <summary>
        /// 全局数据库上下文（懒加载）
        /// </summary>
        readonly static Crs_Db2Context _db = new Lazy<Crs_Db2Context>(() => new Crs_Db2Context()).Value;

        public GameBaseParameter(int programId, int moduleId, Crs_Db2Context db = null)
        {
            ProgramId = programId;
            ModuleId = moduleId;
            Db = db ?? _db;
        }

        /// <summary>
        /// 方案ID
        /// </summary>
        public int ProgramId { get; private set; }

        /// <summary>
        /// 模块ID
        /// </summary>
        public int ModuleId { get; private set; }

        /// <summary>
        /// 数据库上下文，由客户端传入实例，若客户端没有传入实例，则自身实例化一个
        /// </summary>
        public Crs_Db2Context Db { get; private set; }

        /// <summary>
        /// 排班ID（不一定有）
        /// </summary>
        public int? ScheduleId { get; set; }

        /// <summary>
        /// 模块参数列表（不一定有）
        /// </summary>
        public List<ProgramModulePar> ProgramModulePars { get; set; }

        /// <summary>
        /// 开始时间
        /// </summary>
        public DateTime? BeginTime { get; set; }

        /// <summary>
        /// 结束时间
        /// </summary>
        public DateTime? EndTime { get; set; }
    }

    public interface IGameBase
    {
        /// <summary>
        /// 传入参数
        /// </summary>
        //public GameBaseParameter BaseParameter { get; set; }

        /// <summary>
        /// 语音提示，在合适的时候调用，反显信息到客户端界面
        /// </summary>
        Action<string> VoiceTipAction { get; set; }

        /// <summary>
        /// 题目说明，在合适的时候调用，反显信息到客户端界面
        /// </summary>
        Action<string> SynopsisAction { get; set; }

        /// <summary>
        /// 题目规则，在合适的时候调用，反显信息到客户端界面
        /// </summary>
        Action<string> RuleAction { get; set; }

        /// <summary>
        /// 等级统计（当前数、最大数），在合适的时候调用，反显信息到客户端界面
        /// </summary>
        Action<int, int> LevelStatisticsAction { get; set; }

        /// <summary>
        /// 正确统计（当前数、最大数），在合适的时候调用，反显信息到客户端界面
        /// </summary>
        Action<int, int> RightStatisticsAction { get; set; }

        /// <summary>
        /// 错误统计（当前数、最大数），在合适的时候调用，反显信息到客户端界面
        /// </summary>
        Action<int, int> WrongStatisticsAction { get; set; }

        /// <summary>
        /// 耗时统计（总耗时、当前耗时），在合适的时候调用，反显信息到客户端界面
        /// </summary>
        Action<int?, int?> TimeStatisticsAction { get; set; }

        /// <summary>
        /// 游戏开始（一般在讲解结束后调用）
        /// </summary>
        Action GameBeginAction { get; set; }

        /// <summary>
        /// 游戏结束
        /// </summary>
        Action GameEndAction { get; set; }

        /// <summary>
        /// 语音播放
        /// </summary>
        Func<string, Task> VoicePlayFunc { get; set; }

        /// <summary>
        /// 初始化（根据传入参数配置模块参数信息，可以调用委托返回“等级统计”、“正确统计”、“错误统计”等信息，其中委托中的“当前数”默认为0，“最大数”由参数配置）
        /// </summary>
        Task InitAsync(int programId, int moduleId, Crs_Db2Context db = null);

        /// <summary>
        /// 开始（开始游戏，可以调用委托返回“语音提示”、“题目说明”等信息）
        /// </summary>
        Task StartAsync();

        /// <summary>
        /// 结束（结束游戏，记录信息，释放资源等（定时器））
        /// </summary>
        Task StopAsync();

        /// <summary>
        /// 暂停（暂停游戏）
        /// </summary>
        Task PauseAsync();

        /// <summary>
        /// 下一题（切换到下一题）
        /// </summary>
        Task NextAsync();

        /// <summary>
        /// 记录信息
        /// </summary>
        Task ReportAsync();

        /// <summary>
        /// 获取讲解示例
        /// </summary>
        /// <returns></returns>
        IGameBase GetExplanationExample();

        //LJN
        /// <summary>
        /// 显示题目规则
        /// </summary>
        /// <returns></returns>
        public Action<bool> SetTitleVisibleAction { get; set; }
    }
}
