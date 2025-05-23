﻿using crs.core.DbModels;
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
            2025.2.27新加需求：算术游戏、拼图、打蚊子全屏显示
            */
            if (message != null)
            {
                if (message.Equals("算术游戏") || message.Equals("拼图") || message.Equals("打蚊子"))
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
            20241105新加需求：真正开始玩游戏了才显示 "题目规则"这四个字，讲解试玩时不显示
            解决方案：
            在游戏区域开始绘制内容时，顺手把"题目规则"四个字显示出来
            */
            IsTitleVisible = true;
            //这里手动赋值为true是确保在游戏真正玩的时候能显示出来
            return true;
        }

        public bool Remove(IGameBase gameContent = null)
        {
            if (gameContent == null || gameContent == GameContent)
            {
                IsTitleVisible = false;
                //这里手动赋值为true是确保在游戏结束的时候能隐藏掉
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
            // 语音播放功能已移除
        }

        public void SetRuleContent(string content)
        {
            RuleContent = content;
        }

        public void SetTitleVisible(bool Visiblity)
        {
            IsTitleVisible = Visiblity;
        }

        public async void Close()
        {
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
