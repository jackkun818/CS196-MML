using crs.game.Games;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace crs.game
{
    /// <summary>
    /// HostWindow.xaml 的交互逻辑
    /// </summary>
    public partial class HostWindow : Window
    {
        IGameBase gameBase;

        public HostWindow()
        {
            InitializeComponent();
        }

        private async void stopButton_Click(object sender, RoutedEventArgs e)
        {
            if (gameBase == null)
            {
                MessageBox.Show("游戏未加载");
                return;
            }

            await gameBase.StartAsync();
            this.gameContentControl.Content = null;
            gameBase = null;
        }

        private async void nextButton_Click(object sender, RoutedEventArgs e)
        {
            if (gameBase == null)
            {
                MessageBox.Show("游戏未加载");
                return;
            }

            await gameBase.NextAsync();
        }

        private async void pauseButton_Click(object sender, RoutedEventArgs e)
        {
            if (gameBase == null)
            {
                MessageBox.Show("游戏未加载");
                return;
            }

            await gameBase.PauseAsync();
        }

        private async void startButton_Click(object sender, RoutedEventArgs e)
        {
            if (gameBase == null)
            {
                MessageBox.Show("游戏未加载");
                return;
            }

            await gameBase.StartAsync();
        }

        private async void initButton_Click(object sender, RoutedEventArgs e)
        {
            if (gameBase != null)
            {
                MessageBox.Show("游戏已加载");
                return;
            }

            gameBase = new 专注注意力() as IGameBase;
            if (gameBase == null)
            {
                MessageBox.Show("游戏模块未实现IGameBase接口");
                return;
            }

            gameBase.VoiceTipAction = text => this.voiceTextBlock.Text = text;

            gameBase.SynopsisAction = text => this.synopsisTextBlock.Text = text;

            gameBase.LevelStatisticsAction = (current, max) => this.levelTextBlock.Text = $"{current}/{max}";

            gameBase.RightStatisticsAction = (current, max) => this.rightCountTextBlock.Text = $"{current}/{max}";

            gameBase.WrongStatisticsAction = (current, max) => this.wrongCountTextBlock.Text = $"{current}/{max}";

            gameBase.TimeStatisticsAction = (timeSum, time) =>
            {
                this.timeSumTextBlock.Text = timeSum?.ToString();
                this.timeTextBlock.Text = time?.ToString();
            };

            gameBase.GameEndAction = () =>
            {
                this.gameContentControl.Content = null;
                gameBase = null;

                MessageBox.Show("游戏结束");
                return;
            };

            int programId = 1;
            int moduleId = 1;
            await gameBase.InitAsync(programId, moduleId);

            this.gameContentControl.Content = gameBase;
        }
    }
}
