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
    public partial class Arithmetic game explanation : BaseUserControl, IGameBase
    {
        public Arithmetic game explanation()
        {
            InitializeComponent();
        }
    }
    public partial class Arithmetic game explanation : BaseUserControl, IGameBase
    {
        private int CurrentPage = 1;

        private void LastStep_Click(object sender, RoutedEventArgs e)
        {//Clicked on the previous step
            CurrentPage--;
            PageSwitchSet();
        }

        private void NextStep_Click(object sender, RoutedEventArgs e)
        {//Click Next
            CurrentPage++;
            PageSwitchSet();
        }

        private void Skip_Click(object sender, RoutedEventArgs e)
        {//Skip the entire trial and start the body directly
            OnGameBegin();
        }

        private void SetVisibilityForAllChildren(UIElement element, Visibility visibility)//Set the visibility of the components of the leaf nodes uniformly
        {
            if (element is Panel panel)
            {
                // in the case of Panel type（like Grid、StackPanel wait）, iterate over its child elements
                foreach (UIElement child in panel.Children)
                {
                    SetVisibilityForAllChildren(child, visibility); // Recursive call
                }
            }
            else if (element is ContentControl contentControl && contentControl.Content is UIElement content)
            {
                // in the case of ContentControl type（like Button）, check its content
                SetVisibilityForAllChildren(content, visibility);
            }
            else
            {
                // If it is the smallest control（Leaf node）, set it Visibility
                element.Visibility = visibility;
            }
        }

        private void PageSwitchSet()//according topageValue to display components
        {
            SetVisibilityForAllChildren(PicGrid, Visibility.Collapsed);//First hide all leaf components, just display the corresponding required ones
            PicGrid.Visibility = Visibility.Visible;
            switch (CurrentPage)
            {
                case 1://First page
                       //Text description
                    GuideTextBlock.Text = "Now the explanation of the arithmetic game will be performed. Please click Next with the mouse to enter the explanation.";
                    //Picture description

                    //Button below
                    ExplainButtonsGrid.Columns = 2;
                    LastStep.Visibility = Visibility.Collapsed;
                    NextStep.Visibility = Visibility.Visible;
                    Skip.Visibility = Visibility.Visible;
                    break;
                case 2://The second page
                       //Text prompts
                    GuideTextBlock.Text = "First you will see the title of the arithmetic game, you can stare at the white triangle in the middle to click the button to enter the game.";
                    //Picture tips
                    GameCover.Visibility = Visibility.Visible;

                    //Button below
                    ExplainButtonsGrid.Columns = 3;
                    LastStep.Visibility = Visibility.Visible;
                    NextStep.Visibility = Visibility.Visible;
                    Skip.Visibility = Visibility.Visible;
                    break;
                case 3://The third page
                       //Text prompts
                    GuideTextBlock.Text = "Then after you enter the game, you will see several numerical options that you need to find before the countdown above ends?and stare at it to select it.";

                    //Picture tips
                    GameBody.Visibility = Visibility.Visible;
                    //Button below
                    ExplainButtonsGrid.Columns = 3;
                    LastStep.Visibility = Visibility.Visible;
                    NextStep.Visibility = Visibility.Visible;
                    Skip.Visibility = Visibility.Visible;
                    break;
                case 4://The fourth page
                       //Text prompts
                    GuideTextBlock.Text = "When you select the wrong number or do not make a choice before the countdown ends, the game background will turn red to remind you.";

                    //Picture tips
                    GameError.Visibility = Visibility.Visible;

                    //Button below
                    ExplainButtonsGrid.Columns = 3;
                    LastStep.Visibility = Visibility.Visible;
                    NextStep.Visibility = Visibility.Visible;
                    Skip.Visibility = Visibility.Visible;
                    break;
                case 5://Page 5
                       //Text prompts
                    GuideTextBlock.Text = "After the explanation is finished, please click the skip button to start the game.";

                    //Picture tips

                    //Button below
                    ExplainButtonsGrid.Columns = 2;
                    LastStep.Visibility = Visibility.Visible;
                    NextStep.Visibility = Visibility.Collapsed;
                    Skip.Visibility = Visibility.Visible;
                    break;
                default:
                    MessageBox.Show("There is a problem with your network, please contact the administrator");
                    break;
            }
            
        }

    }
}
