using crs.core.DbModels;
using crs.core;
using crs.theme.Extensions;
using HandyControl.Tools.Extension;
using Prism.Commands;
using Prism.Events;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Linq;
using static crs.extension.Crs_Enum;
using static crs.extension.Crs_Interface;
using crs.extension.Models;
using crs.extension;
using crs.dialog.Views;
using crs.game;
using System.Windows;
using System.Threading.Tasks;
using static crs.dialog.Views.SubGamePanel;
using System.Windows.Forms;
using System.ComponentModel;

namespace crs.dialog.ViewModels
{
    public class SubGamePanelViewModel : BindableBase, IDialogResultable<object>, IGameHost
    {
        readonly IRegionManager regionManager;
        readonly IContainerProvider containerProvider;
        readonly IEventAggregator eventAggregator;
        readonly Crs_Db2Context db;

        public SubGamePanelViewModel() { }
        public SubGamePanelViewModel(IRegionManager regionManager, IContainerProvider containerProvider, IEventAggregator eventAggregator, Crs_Db2Context db)
        {
            this.regionManager = regionManager;
            this.containerProvider = containerProvider;
            this.eventAggregator = eventAggregator;
            this.db = db;
        }

        #region Property
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

        private Enum modeType;
        public Enum ModeType
        {
            get { return modeType; }
            set { SetProperty(ref modeType, value); }
        }

        private FrameworkElement gameDemoContent;
        public FrameworkElement GameDemoContent
        {
            get { return gameDemoContent; }
            set { SetProperty(ref gameDemoContent, value); }
        }

        private string gameDemoMessage;
        public string GameDemoMessage
        {
            get { return gameDemoMessage; }
            set { SetProperty(ref gameDemoMessage, value); }
        }

        private IGameBase gameContent;
        public IGameBase GameContent
        {
            get { return gameContent; }
            set { SetProperty(ref gameContent, value); }
        }

        private DateTime? currentCountdownTime;
        public DateTime? CurrentCountdownTime
        {
            get { return currentCountdownTime; }
            set { SetProperty(ref currentCountdownTime, value); }
        }

        private IHumanInterface instance;
        public IHumanInterface Instance
        {
            get { return instance; }
            set { SetProperty(ref instance, value); }
        }

        private bool isTitleVisible = false;
        public bool IsTitleVisible
        {
            get { return isTitleVisible; }
            set
            {
                SetProperty(ref isTitleVisible, value);
                OnPropertyChanged(nameof(IsTitleVisible));
            }
        }

        private string ruleContent;
        public string RuleContent
        {
            get { return ruleContent; }
            set { SetProperty(ref ruleContent, value); }
        }

        private Visibility dockPanelVisibility;
        public Visibility DockPanelVisibility
        {
            get { return dockPanelVisibility; }
            set { SetProperty(ref dockPanelVisibility, value); }
        }
        private Thickness dockPanelMargin;
        public Thickness DockPanelMargin
        {
            get { return dockPanelMargin; }
            set { SetProperty(ref dockPanelMargin, value); }
        }
        private Thickness gameBorderMargin;
        public Thickness GameBorderMargin
        {
            get { return gameBorderMargin; }
            set { SetProperty(ref gameBorderMargin, value); }
        }
        #endregion

        public bool Init(DigitalHumanItem humanItem, PatientItem patientItem, Enum modeType)
        {
            DigitalHumanItem = humanItem;
            PatientItem = patientItem;
            ModeType = modeType;
            return true;
        }

        public bool ShowDemoInfo(FrameworkElement element, string message)
        {
            GameDemoContent = element;
            GameDemoMessage = message;
            /*DKY
            2025.2.27 new requirements: arithmetic games, puzzles, mosquito shooting full screen display
            */
            if (message != null)
            {
                if (message.Equals("Arithmetic game") || message.Equals("puzzle") || message.Equals("Kill mosquitoes"))
                {
                    DockPanelVisibility = Visibility.Collapsed;
                    DockPanelMargin = new Thickness(0);
                    GameBorderMargin = new Thickness(0);
                }
                else
                {
                    DockPanelVisibility = Visibility.Visible;
                    DockPanelMargin = new Thickness(45);
                    GameBorderMargin = new Thickness(0, 0, 45, 0);
                }
            }
            return true;
        }

        public bool Show(IGameBase gameContent)
        {
            GameContent = gameContent;
            /*LJN
            20241105 New requirements: Only when you really start playing games will you show “Question Rules”These four words are not displayed when explaining and trying them
            Solution:
            When you start drawing content in the game area, just put it in a while“Question Rules”Four words are displayed
            */
            IsTitleVisible = true;
            //Here the manual assignment istrueIt is to ensure that the game can be displayed when it is actually played
            return true;
        }

        public bool Remove(IGameBase gameContent = null)
        {
            if (gameContent == null || gameContent == GameContent)
            {
                IsTitleVisible = false;
                //Here the manual assignment istrueIt is to ensure that it can be hidden at the end of the game
                GameContent = null;
                return true;
            }
            return false;
        }

        public bool ShowTime(int? totalCountdownTime, int? currentCountdownTime)
        {
            CurrentCountdownTime = DateTime.MinValue.AddSeconds(Math.Max(currentCountdownTime ?? 0, 0));
            return true;
        }

        public async Task VoicePlayAsync(string message)
        {
            var instance = Instance;
            await (instance?.SendMessageAsync(message) ?? Task.CompletedTask);
        }

        public void SetRuleContent(string content)
        {
            RuleContent = content;
        }

        //LJN
        public void SetTitleVisible(bool Visiblity)
        {
            IsTitleVisible = Visiblity;
        }
        //LJN
        public async void Close()
        {
            await VoicePlayAsync(null);
            CloseAction?.Invoke();
        }

        public object Result { get; set; }
        public Action CloseAction { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
