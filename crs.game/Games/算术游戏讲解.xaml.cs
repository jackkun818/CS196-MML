using crs.core.DbModels;
using crs.core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Media;
using System.Windows.Controls.Primitives;
using Spire.Additions.Xps.Schema.Mc;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Rebar;

namespace crs.game.Games
{
    public partial class 算术游戏讲解 : BaseUserControl, IGameBase
    {
        public 算术游戏讲解()
        {
            InitializeComponent();
        }
    }
    public partial class 算术游戏讲解 : BaseUserControl, IGameBase
    {
        private int CurrentPage = 1;

        private void LastStep_Click(object sender, RoutedEventArgs e)
        {//点击了上一步
            CurrentPage--;
            PageSwitchSet();
        }

        private void NextStep_Click(object sender, RoutedEventArgs e)
        {//点击了下一步
            CurrentPage++;
            PageSwitchSet();
        }

        private void Skip_Click(object sender, RoutedEventArgs e)
        {//跳过整个试玩，直接开始本体
            OnGameBegin();
        }

        private void SetVisibilityForAllChildren(UIElement element, Visibility visibility)//把叶子节点的组件的可见性统一设置
        {
            if (element is Panel panel)
            {
                // 如果是 Panel 类型（如 Grid、StackPanel 等），遍历其子元素
                foreach (UIElement child in panel.Children)
                {
                    SetVisibilityForAllChildren(child, visibility); // 递归调用
                }
            }
            else if (element is ContentControl contentControl && contentControl.Content is UIElement content)
            {
                // 如果是 ContentControl 类型（如 Button），检查其内容
                SetVisibilityForAllChildren(content, visibility);
            }
            else
            {
                // 如果是最小控件（叶节点），设置其 Visibility
                element.Visibility = visibility;
            }
        }

        private void PageSwitchSet()//根据page值来显示组件
        {
            SetVisibilityForAllChildren(PicGrid, Visibility.Collapsed);//先把所有的叶子组件都隐藏，只需要显示对应的需要的
            PicGrid.Visibility = Visibility.Visible;
            switch (CurrentPage)
            {
                case 1://第一个页面
                       //文字说明
                    GuideTextBlock.Text = "现在将进行算术游戏的讲解，请用鼠标点击下一步进入讲解。";
                    //图片说明

                    //下方按钮
                    ExplainButtonsGrid.Columns = 2;
                    LastStep.Visibility = Visibility.Collapsed;
                    NextStep.Visibility = Visibility.Visible;
                    Skip.Visibility = Visibility.Visible;
                    break;
                case 2://第二个页面
                       //文字提示
                    GuideTextBlock.Text = "首先您将会看到算数游戏的标题，您可以盯住中间的白色三角形以点击该按钮，从而进入游戏。";
                    //图片提示
                    GameCover.Visibility = Visibility.Visible;

                    //下方按钮
                    ExplainButtonsGrid.Columns = 3;
                    LastStep.Visibility = Visibility.Visible;
                    NextStep.Visibility = Visibility.Visible;
                    Skip.Visibility = Visibility.Visible;
                    break;
                case 3://第三个页面
                       //文字提示
                    GuideTextBlock.Text = "接着在您进入游戏后，您会看到若干数字选项，您需要在上方倒计时结束之前找出?中的数字，并盯住它以选中它。";

                    //图片提示
                    GameBody.Visibility = Visibility.Visible;
                    //下方按钮
                    ExplainButtonsGrid.Columns = 3;
                    LastStep.Visibility = Visibility.Visible;
                    NextStep.Visibility = Visibility.Visible;
                    Skip.Visibility = Visibility.Visible;
                    break;
                case 4://第四个页面
                       //文字提示
                    GuideTextBlock.Text = "当您选错数字或在倒计时结束前未做出选择，游戏背景将会变成红色以提醒您。";

                    //图片提示
                    GameError.Visibility = Visibility.Visible;

                    //下方按钮
                    ExplainButtonsGrid.Columns = 3;
                    LastStep.Visibility = Visibility.Visible;
                    NextStep.Visibility = Visibility.Visible;
                    Skip.Visibility = Visibility.Visible;
                    break;
                case 5://第五个页面
                       //文字提示
                    GuideTextBlock.Text = "讲解完毕，请点击跳过按钮以开始游戏。";

                    //图片提示

                    //下方按钮
                    ExplainButtonsGrid.Columns = 2;
                    LastStep.Visibility = Visibility.Visible;
                    NextStep.Visibility = Visibility.Collapsed;
                    Skip.Visibility = Visibility.Visible;
                    break;
                default:
                    MessageBox.Show("您的网络存在问题，请联系管理员");
                    break;
            }
            
        }

    }
}
