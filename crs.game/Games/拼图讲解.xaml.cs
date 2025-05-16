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
    public partial class 拼图讲解 : BaseUserControl, IGameBase
    {
        public 拼图讲解()
        {
            InitializeComponent();
        }
    }
    public partial class 拼图讲解 : BaseUserControl, IGameBase
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
                    GuideTextBlock.Text = "现在将进行拼图游戏的讲解，请用鼠标点击下一步进入讲解。";
                    //图片说明

                    //下方按钮
                    ExplainButtonsGrid.Columns = 2;
                    LastStep.Visibility = Visibility.Collapsed;
                    NextStep.Visibility = Visibility.Visible;
                    Skip.Visibility = Visibility.Visible;
                    Skip.Content = "跳过";
                    break;
                case 2://第二个页面
                       //文字提示
                    GuideTextBlock.Text = "首先您的屏幕右侧会出现一幅被打乱的拼图，左侧是它还原后的样子。";
                    //图片提示
                    GameBody.Visibility = Visibility.Visible;

                    //下方按钮
                    ExplainButtonsGrid.Columns = 3;
                    LastStep.Visibility = Visibility.Visible;
                    NextStep.Visibility = Visibility.Visible;
                    Skip.Visibility = Visibility.Visible;
                    Skip.Content = "跳过";
                    break;
                case 3://第五个页面
                       //文字提示
                    GuideTextBlock.Text = "您需要用眼睛观察各个拼图碎片的图案，通过注视并拖动碎片到左侧合适的位置，将拼图完整还原。";

                    //图片提示
                    GameBody.Visibility = Visibility.Visible;
                    //下方按钮
                    ExplainButtonsGrid.Columns = 3;
                    LastStep.Visibility = Visibility.Visible;
                    NextStep.Visibility = Visibility.Visible;
                    Skip.Visibility = Visibility.Visible;
                    Skip.Content = "跳过";
                    break;
                case 4://第五个页面
                       //文字提示
                    GuideTextBlock.Text = "讲解完毕，请点击开始游戏按钮以进行游戏。";

                    //图片提示

                    //下方按钮
                    ExplainButtonsGrid.Columns = 2;
                    LastStep.Visibility = Visibility.Visible;
                    NextStep.Visibility = Visibility.Collapsed;
                    Skip.Visibility = Visibility.Visible;
                    Skip.Content = "开始游戏";
                    break;
                default:
                    MessageBox.Show("您的网络存在问题，请联系管理员");
                    break;
            }

        }

    }
}
