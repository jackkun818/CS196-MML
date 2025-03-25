using crs.core;
using crs.core.DbModels;
using crs.extension;
using crs.extension.Models;
using crs.game;
using crs.game.Games;
using crs.theme.Extensions;
using Flurl.Http.Content;
using HandyControl.Controls;
using HandyControl.Tools.Extension;
using Microsoft.EntityFrameworkCore;
using Prism.Commands;
using Prism.Events;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using static crs.extension.Crs_Enum;
using static crs.extension.Crs_Interface;
using MessageBoxButton = crs.theme.Extensions.MessageBoxButton;

namespace crs.dialog.ViewModels
{
    public class EvaluateGamePanelViewModel : BindableBase, IDialogResultable<bool>, IDialogCommon<int, DigitalHumanItem>
    {
        readonly IRegionManager regionManager;
        readonly IContainerProvider containerProvider;
        readonly IEventAggregator eventAggregator;
        readonly Crs_Db2Context db;

        int? programId;

        public EvaluateGamePanelViewModel() { }
        public EvaluateGamePanelViewModel(IRegionManager regionManager, IContainerProvider containerProvider, IEventAggregator eventAggregator, Crs_Db2Context db)
        {
            this.regionManager = regionManager;
            this.containerProvider = containerProvider;
            this.eventAggregator = eventAggregator;
            this.db = db;
        }

        #region Property
        private IGameHost gameHost;
        public IGameHost GameHost
        {
            get { return gameHost; }
            set { SetProperty(ref gameHost, value); }
        }

        private IGameBase gameBase;
        public IGameBase GameBase
        {
            get { return gameBase; }
            set { SetProperty(ref gameBase, value); }
        }

        private IGameBase gameExample;
        public IGameBase GameExample
        {
            get { return gameExample; }
            set { SetProperty(ref gameExample, value); }
        }

        private string voiceTipContent;
        public string VoiceTipContent
        {
            get { return voiceTipContent; }
            set { SetProperty(ref voiceTipContent, value); }
        }

        private string synopsisContent;
        public string SynopsisContent
        {
            get { return synopsisContent; }
            set { SetProperty(ref synopsisContent, value); }
        }

        private string ruleContent;
        public string RuleContent
        {
            get { return ruleContent; }
            set
            {
                SetProperty(ref ruleContent, value);

                var gameHost = GameHost;
                gameHost?.SetRuleContent(value);
            }
        }

        //LJN
        private bool isTitleVisible = false;
        public bool IsTitleVisible
        {
            get { return isTitleVisible; }
            set
            {
                SetProperty(ref isTitleVisible, value);
                var gameHost = GameHost;
                gameHost?.SetTitleVisible(value);
            }
        }
        //LJN

        private DateTime? totalCountdownTime;
        public DateTime? TotalCountdownTime
        {
            get { return totalCountdownTime; }
            set { SetProperty(ref totalCountdownTime, value); }
        }

        private DateTime? currentCountdownTime;
        public DateTime? CurrentCountdownTime
        {
            get { return currentCountdownTime; }
            set { SetProperty(ref currentCountdownTime, value); }
        }

        private ObservableCollection<ProgramItem<EvaluateTestMode, EvaluateTestItem>> programItems;
        public ObservableCollection<ProgramItem<EvaluateTestMode, EvaluateTestItem>> ProgramItems
        {
            get { return programItems; }
            set { SetProperty(ref programItems, value); }
        }

        private ProgramItem<EvaluateTestMode, EvaluateTestItem> programSelectedItem;
        public ProgramItem<EvaluateTestMode, EvaluateTestItem> ProgramSelectedItem
        {
            get { return programSelectedItem; }
            set { SetProperty(ref programSelectedItem, value); }
        }

        private DigitalHumanItem digitalHumanItem;
        public DigitalHumanItem DigitalHumanItem
        {
            get { return digitalHumanItem; }
            set { SetProperty(ref digitalHumanItem, value); }
        }

        private PatientItem patientItem;
        public PatientItem PatientItem
        {
            get { return patientItem; }
            set { SetProperty(ref patientItem, value); }
        }

        private bool gameStatus;
        public bool GameStatus
        {
            get { return gameStatus; }
            set { SetProperty(ref gameStatus, value); }
        }


       
        #endregion

        private DelegateCommand audioPlayClickCommand;
        public DelegateCommand AudioPlayClickCommand =>
            audioPlayClickCommand ?? (audioPlayClickCommand = new DelegateCommand(ExecuteAudioPlayClickCommand));

        async void ExecuteAudioPlayClickCommand()
        {
            var gameHost = GameHost;
            gameHost?.VoicePlayAsync(VoiceTipContent);
        }


        private DelegateCommand cancelCommand;
        public DelegateCommand CancelCommand =>
            cancelCommand ?? (cancelCommand = new DelegateCommand(ExecuteCancelCommand));

        async void ExecuteCancelCommand()
        {
            if (await Crs_DialogEx.MessageBoxShow().GetMessageBoxResultAsync("评估还未完成，是否退出？", button: MessageBoxButton.OKOrCancel) == null)
            {
                return;
            }

            var program = await db.Programs.FirstOrDefaultAsync(m => m.ProgramId == programId);
            if (program != null)
            {
                program.ActEndTime = DateTime.Now;
                db.Programs.Update(program);
                db.SaveChanges();
            }

            var gameBase = GameBase;
            await (gameBase?.StopAsync() ?? Task.CompletedTask);

            var gameHost = GameHost;
            gameHost?.Remove(gameBase);
            gameHost?.Close();

            GameClear();

            Result = false;
            CloseAction?.Invoke();
            GameStatus = false;
        }

        private DelegateCommand exampleCommand;
        public DelegateCommand ExampleCommand =>
            exampleCommand ?? (exampleCommand = new DelegateCommand(ExecuteExampleCommand));

        async void ExecuteExampleCommand()
        {
            var gameBase = GameBase;
            var explanationExample = gameBase?.GetExplanationExample();
            var control = gameBase as UserControl;
            if (control != null) control.IsEnabled = true;


            if (explanationExample == null)
            {
                await Crs_DialogEx.MessageBoxShow().GetMessageBoxResultAsync("讲解示例未实现");
                return;
            }

            var gameHost = GameHost;
            gameHost?.VoicePlayAsync(null);
            gameHost?.ShowDemoInfo(null, null);

            explanationExample.GameBeginAction = ExecuteStartCommand;
            explanationExample.VoicePlayFunc = async message => await (gameHost?.VoicePlayAsync(message) ?? Task.CompletedTask);
            //LJN,在讲解模块开始的时候就需要把RuleAction赋值，不然无法调用对应的委托
            explanationExample.RuleAction = rule => gameHost?.SetRuleContent(rule);
            explanationExample.SetTitleVisibleAction = Visible => gameHost?.SetTitleVisible(Visible);
            //LJN
            GameExample = explanationExample;

            var selectedItem = ProgramSelectedItem;
            gameHost?.ShowDemoInfo(explanationExample as FrameworkElement, selectedItem?.Mode.ToString());
        }

        private DelegateCommand exampleIgnoreCommand;
        public DelegateCommand ExampleIgnoreCommand =>
            exampleIgnoreCommand ?? (exampleIgnoreCommand = new DelegateCommand(ExecuteExampleIgnoreCommand));

        void ExecuteExampleIgnoreCommand()
        {
            ExecuteStartCommand();
        }

        private DelegateCommand startCommand;
        public DelegateCommand StartCommand =>
            startCommand ?? (startCommand = new DelegateCommand(_ExecuteStartCommand));

        void _ExecuteStartCommand()
        {
            var gameExample = GameExample;
            if (gameExample == null)
            {
                ExecuteExampleCommand();
                return;
            }

            ExecuteStartCommand();
        }

        async void ExecuteStartCommand()
        {
            var gameExample = GameExample;
            if (gameExample != null)
            {
                gameExample.GameBeginAction = null;
                gameExample.VoicePlayFunc = null;
            }

            var gameHost = GameHost;
            gameHost?.VoicePlayAsync(null);
            gameHost?.ShowDemoInfo(null, null);

            var gameBase = GameBase;
            var control = gameBase as UserControl;
            if (control != null) control.IsEnabled = true;

            gameHost?.Show(gameBase);
            await (gameBase?.StartAsync() ?? Task.CompletedTask);
            GameStatus = true;
        }

        private DelegateCommand stopCommand;
        public DelegateCommand StopCommand =>
            stopCommand ?? (stopCommand = new DelegateCommand(ExecuteStopCommand));

        async void ExecuteStopCommand()
        {
            bool? result;
            if ((result = await Crs_DialogEx.MessageBoxShow().GetMessageBoxResultAsync("评估还未完成，是否中断评估并生成报告？", button: MessageBoxButton.CustomReport)) == null)
            {
                return;
            }

            var gameBase = GameBase;
            var gameHost = GameHost;

            if (result.Value)
            {
                await (gameBase?.StopAsync() ?? Task.CompletedTask);
                await (gameBase?.ReportAsync() ?? Task.CompletedTask);
                gameHost?.Remove(gameBase);
                GameStatus = false;
                /*
                 LJN
                20241106新需求：点击 医生端的“结束评估”后，选择“生成报告”会有两个弹窗
                原因是在选择生成报告后程序会多一个窗口出来，因此修改此部分显示逻辑
                解决方案：多加IfGenerate标志位，来修改弹窗显示的内容
                 */
                //这个弹窗合并到TryProgramSelectedChanged()中
                //await Crs_DialogEx.MessageBoxShow().GetMessageBoxResultAsync("中断并生成报告成功");
            }
            else
            {
                await (gameBase?.StopAsync() ?? Task.CompletedTask);
                gameHost?.Remove(gameBase);
                GameStatus = false;
            }

            await TryProgramSelectedChanged(result.Value);
        }

        private DelegateCommand pauseCommand;
        public DelegateCommand PauseCommand =>
            pauseCommand ?? (pauseCommand = new DelegateCommand(ExecutePauseCommand));

        async void ExecutePauseCommand()
        {
            var gameBase = GameBase;
            var control = gameBase as UserControl;
            if (control != null) control.IsEnabled = false;

            await (gameBase?.PauseAsync() ?? Task.CompletedTask);
            GameStatus = false;
        }

        private DelegateCommand resetCommand;
        public DelegateCommand ResetCommand =>
            resetCommand ?? (resetCommand = new DelegateCommand(ExecuteResetCommand));

        async void ExecuteResetCommand()
        {
            var gameBase = GameBase;
            await (gameBase?.StopAsync() ?? Task.CompletedTask);

            var gameHost = GameHost;
            gameHost?.Remove(gameBase);
            GameStatus = false;

            GameClear(false);
            await Task.Yield();

            await GameInit();
            ExecuteStartCommand();
        }

        private DelegateCommand nextCommand;
        public DelegateCommand NextCommand =>
            nextCommand ?? (nextCommand = new DelegateCommand(ExecuteNextCommand));

        async void ExecuteNextCommand()
        {
            var gameBase = GameBase;
            await (gameBase?.NextAsync() ?? Task.CompletedTask);
        }

        async Task TryProgramSelectedChanged(bool IfGenerate = false, bool IfEnd = false)
        {
            do
            {
                GameClear();
                await Task.Yield();

                var selectedItem = await ProgramSelectedChanged();
                if (selectedItem == null)
                {
                    var program = await db.Programs.FirstOrDefaultAsync(m => m.ProgramId == programId);
                    if (program != null)
                    {
                        program.ActEndTime = DateTime.Now;
                        db.Programs.Update(program);
                        db.SaveChanges();
                    }
                    /*
                     LJN
                    20241106新需求：点击 医生端的“结束评估”后，选择“生成报告”会有两个弹窗
                    原因是在选择生成报告后程序会多一个窗口出来，因此修改此部分显示逻辑
                    解决方案：多加IfGenerate标志位，来修改弹窗显示的内容
                     */
                    //await Crs_DialogEx.MessageBoxShow().GetMessageBoxResultAsync("评估结束");
                    /*
                     IfGenerate = true, IfEnd=false,中断并要生成报告
                    IfGenerate = false, IfEnd=false,中断但不要生成报告
                    IfGenerate = false, IfEnd=true,正常结束，同时生成报告
                    IfGenerate = true, IfEnd=true,正常结束，同时生成报告
                     */
                    if (IfEnd == false)
                    {//是中断的时候才会进入这个逻辑，正常结束的弹窗不在这个函数的逻辑内处理，而是在gameBase.GameEndAction中
                        if (IfGenerate)
                        {//如果是要生成报告的
                            await Crs_DialogEx.MessageBoxShow().GetMessageBoxResultAsync("评估结束，报告已保存至数据中心");
                        }
                        else
                        {//如果是不要生成报告的
                            await Crs_DialogEx.MessageBoxShow().GetMessageBoxResultAsync("评估结束");
                        }
                    }



                    var gameHost = GameHost;
                    gameHost?.Close();

                    Result = true;
                    CloseAction?.Invoke();
                    GameStatus = false;
                    return;
                }
            } while (!await GameInit());

            async Task<ProgramItem<EvaluateTestMode, EvaluateTestItem>> ProgramSelectedChanged()
            {
                var items = ProgramItems;
                if (items == null)
                {
                    return null;
                }

                var selectedItem = ProgramSelectedItem;
                ProgramSelectedItem = null;

                await Task.Yield();

                var _items = items.ToList();
                if (selectedItem == null)
                {
                    selectedItem = _items.FirstOrDefault();
                    ProgramSelectedItem = selectedItem;
                }
                else
                {
                    if (selectedItem == _items.LastOrDefault())
                    {
                        selectedItem = null;
                        ProgramSelectedItem = null;
                    }
                    else
                    {
                        for (int index = 0; index < _items.Count; index++)
                        {
                            var item = _items[index];
                            if (item == selectedItem)
                            {
                                selectedItem = _items[index + 1];
                                ProgramSelectedItem = selectedItem;
                                break;
                            }
                        }
                    }
                }

                return selectedItem;
            }
        }

        async Task<bool> GameInit()
        {
            var selectedItem = ProgramSelectedItem;
            if (selectedItem == null)
            {
                return false;
            }

            var assembly = typeof(IGameBase).Assembly;
            var type = assembly?.GetType($"crs.game.Games.{selectedItem.Mode}");
            if (type == null)
            {
                await Crs_DialogEx.MessageBoxShow().GetMessageBoxResultAsync($"模块“{selectedItem.Mode}”未实现");
                return false;
            }

            if (type.BaseType != typeof(BaseUserControl) && type.BaseType != typeof(UserControl))
            {
                await Crs_DialogEx.MessageBoxShow().GetMessageBoxResultAsync($"模块“{selectedItem.Mode}”不是“BaseUserControl类型”或“UserControl类型”");
                return false;
            }

            var constructorInfo = type.GetConstructor(Type.EmptyTypes);
            if (constructorInfo == null)
            {
                await Crs_DialogEx.MessageBoxShow().GetMessageBoxResultAsync($"模块“{selectedItem.Mode}”没有定义“无参构造函数”");
                return false;
            }

            var instance = Activator.CreateInstance(type) as UserControl;
            var gameBase = instance as IGameBase;
            if (gameBase == null)
            {
                await Crs_DialogEx.MessageBoxShow().GetMessageBoxResultAsync($"模块“{selectedItem.Mode}”未实现“IGameBase接口”");
                return false;
            }

            //LJN
            gameBase.SetTitleVisibleAction = content => isTitleVisible = content;
            //LJN
            gameBase.VoiceTipAction = content => VoiceTipContent = content;
            gameBase.SynopsisAction = content => SynopsisContent = content;
            gameBase.RuleAction = content => RuleContent = content;
            gameBase.LevelStatisticsAction = (value, count) => { };
            gameBase.RightStatisticsAction = (value, count) => { };
            gameBase.WrongStatisticsAction = (value, count) => { };
            gameBase.TimeStatisticsAction = (totalCountdownTime, currentCountdownTime) =>
            {
                var gameHost = GameHost;
                gameHost?.ShowTime(totalCountdownTime, currentCountdownTime);

                TotalCountdownTime = DateTime.MinValue.AddSeconds(Math.Max(totalCountdownTime ?? 0, 0));
                CurrentCountdownTime = DateTime.MinValue.AddSeconds(Math.Max(currentCountdownTime ?? 0, 0));
            };
            gameBase.GameEndAction = () => App.Current.Dispatcher.InvokeAsync(async () =>
            {
                await (gameBase?.StopAsync() ?? Task.CompletedTask);
                await (gameBase?.ReportAsync() ?? Task.CompletedTask);

                var gameHost = GameHost;
                gameHost?.Remove(gameBase);
                GameStatus = false;

                GameClear();
                /*
                 20241125，正常结束，只需要弹出一个窗口
                 */
                await Crs_DialogEx.MessageBoxShow().GetMessageBoxResultAsync("评估已完成，报告已保存至数据中心");

                await TryProgramSelectedChanged(IfEnd: true);
            });
            gameBase.VoicePlayFunc = async message =>
            {
                var gameHost = GameHost;
                await (gameHost?.VoicePlayAsync(message) ?? Task.CompletedTask);
            };

            var moduleId = selectedItem.Item.Data.ModuleId;
            await (gameBase?.InitAsync(programId.Value, moduleId, db) ?? Task.CompletedTask);
            GameBase = gameBase;

            var digitalHumanItem = DigitalHumanItem;
            var patientItem = PatientItem;

            var gameHost = GameHost;
            gameHost.Init(digitalHumanItem, patientItem, selectedItem.Mode);

            //ExecuteExampleCommand();
            return true;
        }

        void GameClear(bool processExample = true)
        {
            var gameBase = GameBase;
            if (gameBase != null)
            {
                gameBase.VoiceTipAction = null;
                gameBase.SynopsisAction = null;
                gameBase.RuleAction = null;
                gameBase.LevelStatisticsAction = null;
                gameBase.RightStatisticsAction = null;
                gameBase.WrongStatisticsAction = null;
                gameBase.TimeStatisticsAction = null;
                gameBase.GameEndAction = null;
                gameBase.VoicePlayFunc = null;
                GameBase = null;
            }

            var gameExample = GameExample;
            if (gameExample != null && processExample)
            {
                gameExample.GameBeginAction = null;
                gameExample.VoicePlayFunc = null;
                GameExample = null;
            }

            var gameHost = GameHost;
            gameHost?.VoicePlayAsync(null);
            gameHost?.ShowTime(null, null);
            gameHost?.SetRuleContent(null);

            VoiceTipContent = null;
            SynopsisContent = null;
            RuleContent = null;
            TotalCountdownTime = null;
            CurrentCountdownTime = null;
        }

        public async void Execute(int programId, DigitalHumanItem digitalHumanItem)
        {
            this.programId = programId;
            DigitalHumanItem = digitalHumanItem;

            var (status, msg, multiItems) = await Crs_DialogEx.ProgressShow().GetProgressResultAsync<(bool, string, List<ProgramModule>)>(async exception =>
            {
                exception.Exception = async ex =>
                {
                    exception.Message = "获取方案信息错误";
                    return (false, $"{exception.Message},{ex.Message}", null);
                };

                var program = await db.Programs.Include(m => m.Patient).FirstOrDefaultAsync(m => m.ProgramId == programId);
                if (program != null)
                {
                    program.ActStartTime = DateTime.Now;
                    db.Programs.Update(program);
                    db.SaveChanges();
                }

                if (program.Patient != null)
                {
                    PatientItem = new PatientItem().Update(program.Patient);
                }

                var multiItems = await db.ProgramModules.AsNoTracking().Include(m => m.Module).Where(m => m.ProgramId == programId).ToListAsync();

                return (true, null, multiItems);
            });

            if (!status)
            {
                await Crs_DialogEx.MessageBoxShow().GetMessageBoxResultAsync(msg);
                return;
            }

            var items = multiItems.Select(m =>
            {
                var module = m.Module;
                if (!Enum.TryParse<EvaluateTestMode>(module.Name?.Trim(), out var modeResult))
                {
                    return null;
                }

                var evaluateTestItem = new EvaluateTestItem { Mode = modeResult }.Update(module);
                return evaluateTestItem;
            }).Where(m => m != null).ToList();

            ProgramItems = new ObservableCollection<ProgramItem<EvaluateTestMode, EvaluateTestItem>>(items.Select(m => new ProgramItem<EvaluateTestMode, EvaluateTestItem>
            {
                Mode = m.Mode,
                Item = m
            }));

            IGameHost gameHost = null;
            _ = Crs_DialogEx.Show(Crs_Dialog.SubGamePanel, Crs_DialogToken.SubTopContent)
                .UseConfig_ContentStretch()
                .Initialize<IGameHost>(vm => gameHost = vm)
                .GetResultAsync<object>();

            GameHost = gameHost;

            await TryProgramSelectedChanged();
        }

        public bool Result { get; set; }
        public Action CloseAction { get; set; }
    }
}
