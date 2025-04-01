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
    public partial class Explanation of killing mosquitoes : BaseUserControl, IGameBase
    {
        public Explanation of killing mosquitoes()
        {
            InitializeComponent();
        }
    }
    public partial class Explanation of killing mosquitoes : BaseUserControl, IGameBase
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
                    GuideTextBlock.Text = "Now the explanation of the mosquito-killing game will be carried out. Please click Next to enter the explanation with the mouse.";
                    //Picture description

                    //Button below
                    ExplainButtonsGrid.Columns = 2;
                    LastStep.Visibility = Visibility.Collapsed;
                    NextStep.Visibility = Visibility.Visible;
                    Skip.Visibility = Visibility.Visible;
                    Skip.Content = "jump over";
                    break;
                case 2://The second page
                       //Text prompts
                    GuideTextBlock.Text = "Mosquitoes will appear randomly on your screen. Please look at the target mosquito with your eyes to get rid of the mosquito.";
                    //Picture tips
                    GameBody.Visibility = Visibility.Visible;

                    //Button below
                    ExplainButtonsGrid.Columns = 3;
                    LastStep.Visibility = Visibility.Visible;
                    NextStep.Visibility = Visibility.Visible;
                    Skip.Visibility = Visibility.Visible;
                    Skip.Content = "jump over";
                    break;
                case 3://Page 5
                       //Text prompts
                    GuideTextBlock.Text = "After the explanation is completed, please click the Start Game button to play.";

                    //Picture tips

                    //Button below
                    ExplainButtonsGrid.Columns = 2;
                    LastStep.Visibility = Visibility.Visible;
                    NextStep.Visibility = Visibility.Collapsed;
                    Skip.Visibility = Visibility.Visible;
                    Skip.Content = "Start the game";
                    break;
                default:
                    MessageBox.Show("There is a problem with your network, please contact the administrator");
                    break;
            }

        }

    }
}
