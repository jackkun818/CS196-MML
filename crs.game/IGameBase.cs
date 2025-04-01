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
    /// parameter
    /// </summary>
    public class GameBaseParameter
    {
        /// <summary>
        /// Global database context（Lazy loading）
        /// </summary>
        readonly static Crs_Db2Context _db = new Lazy<Crs_Db2Context>(() => new Crs_Db2Context()).Value;

        public GameBaseParameter(int programId, int moduleId, Crs_Db2Context db = null)
        {
            ProgramId = programId;
            ModuleId = moduleId;
            Db = db ?? _db;
        }

        /// <summary>
        /// planID
        /// </summary>
        public int ProgramId { get; private set; }

        /// <summary>
        /// ModuleID
        /// </summary>
        public int ModuleId { get; private set; }

        /// <summary>
        /// Database context, instance is passed from the client. If the client does not pass in, it instantiates one itself.
        /// </summary>
        public Crs_Db2Context Db { get; private set; }

        /// <summary>
        /// ScheduleID（Not necessarily）
        /// </summary>
        public int? ScheduleId { get; set; }

        /// <summary>
        /// Module parameter list（Not necessarily）
        /// </summary>
        public List<ProgramModulePar> ProgramModulePars { get; set; }

        /// <summary>
        /// Start time
        /// </summary>
        public DateTime? BeginTime { get; set; }

        /// <summary>
        /// End time
        /// </summary>
        public DateTime? EndTime { get; set; }
    }

    public interface IGameBase
    {
        /// <summary>
        /// Pass in parameters
        /// </summary>
        //public GameBaseParameter BaseParameter { get; set; }

        /// <summary>
        /// Voice prompt, call it when appropriate, and display the information to the client interface
        /// </summary>
        Action<string> VoiceTipAction { get; set; }

        /// <summary>
        /// Question description: call it when appropriate and display the information to the client interface
        /// </summary>
        Action<string> SynopsisAction { get; set; }

        /// <summary>
        /// The question rules are called when appropriate, and the information is reversed to the client interface
        /// </summary>
        Action<string> RuleAction { get; set; }

        /// <summary>
        /// Level statistics（Current number, maximum number）, call it when appropriate, and display the information to the client interface
        /// </summary>
        Action<int, int> LevelStatisticsAction { get; set; }

        /// <summary>
        /// Correct statistics（Current number, maximum number）, call it when appropriate, and display the information to the client interface
        /// </summary>
        Action<int, int> RightStatisticsAction { get; set; }

        /// <summary>
        /// Error statistics（Current number, maximum number）, call it when appropriate, and display the information to the client interface
        /// </summary>
        Action<int, int> WrongStatisticsAction { get; set; }

        /// <summary>
        /// Time-consuming statistics（Total time, current time）, call it when appropriate, and display the information to the client interface
        /// </summary>
        Action<int?, int?> TimeStatisticsAction { get; set; }

        /// <summary>
        /// The game begins（Usually called after the explanation）
        /// </summary>
        Action GameBeginAction { get; set; }

        /// <summary>
        /// game over
        /// </summary>
        Action GameEndAction { get; set; }

        /// <summary>
        /// Voice playback
        /// </summary>
        Func<string, Task> VoicePlayFunc { get; set; }

        /// <summary>
        /// initialization（According to the incoming parameter configuration module parameter information, you can call delegate to return“Level statistics”、“Correct statistics”、“Error statistics”etc., among which the“Current number”Default is 0,“Maximum number”Configure by parameters）
        /// </summary>
        Task InitAsync(int programId, int moduleId, Crs_Db2Context db = null);

        /// <summary>
        /// start（Start the game, you can call the delegate to return“Voice prompts”、“Title description”Wait for information）
        /// </summary>
        Task StartAsync();

        /// <summary>
        /// Finish（End the game, record information, release resources, etc.（Timer））
        /// </summary>
        Task StopAsync();

        /// <summary>
        /// pause（Pause the game）
        /// </summary>
        Task PauseAsync();

        /// <summary>
        /// Next question（Switch to the next question）
        /// </summary>
        Task NextAsync();

        /// <summary>
        /// Record information
        /// </summary>
        Task ReportAsync();

        /// <summary>
        /// Get an explanation example
        /// </summary>
        /// <returns></returns>
        IGameBase GetExplanationExample();

        //LJN
        /// <summary>
        /// Show questions rules
        /// </summary>
        /// <returns></returns>
        public Action<bool> SetTitleVisibleAction { get; set; }
    }
}
