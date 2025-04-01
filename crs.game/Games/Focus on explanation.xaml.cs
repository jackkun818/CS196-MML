using System;
using System.Collections.Generic;
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

namespace crs.game.Games
{
    /// <summary>
    /// Focus on explanation.xaml Interaction logic
    /// </summary>
    public partial class Focus on explanation : BaseUserControl
    {
        private int left = 1;

        public Focus on explanation()
        {
            InitializeComponent();
            TipBlock.Text = null;// "Please find the same picture as the right one from the three pictures on the left, through←→Press the key to control the movement of the selected box. After the selected box is moved to the target picture, pressenterkey.";
            Image1.Source = new BitmapImage(new Uri("Focus on attention/1/1.png", UriKind.Relative));
            Image2.Source = new BitmapImage(new Uri("Focus on attention/1/2.png", UriKind.Relative));
            Image3.Source = new BitmapImage(new Uri("Focus on attention/1/3.png", UriKind.Relative));
            Image4.Source = new BitmapImage(new Uri("Focus on attention/1/1.png", UriKind.Relative));
            Image5.Source = new BitmapImage(new Uri("Focus on attention/1/1.png", UriKind.Relative));
            Image6.Source = new BitmapImage(new Uri("Focus on attention/1/2.png", UriKind.Relative));
            TargetImage.Source = new BitmapImage(new Uri("Focus on attention/1/3.png", UriKind.Relative));

            this.Loaded += Focus on explanation_Loaded;
        }

        private void Focus on explanation_Loaded(object sender, RoutedEventArgs e)
        {
            nextButton_Click(null, null);
        }

        protected override void OnHostWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                bool isCorrect = (left == 3);//The rightmost answer is the correct answer

                if (isCorrect)
                {
                    TipBlock.FontSize = 40;
                    TipBlock.Text = "Congratulations on getting right！";
                    TipBlock.Foreground = new SolidColorBrush(Colors.Green);
                    OkButton.Visibility = Visibility.Visible;
                }
                else
                {
                    TipBlock.FontSize = 40;
                    TipBlock.Text = "Sorry to answer wrong！";
                    TipBlock.Foreground = new SolidColorBrush(Colors.Red);
                    OkButton.Visibility = Visibility.Collapsed;
                }
            }
            else
            {
                if (left > 1 && e.Key == Key.Left)
                    left -= 1;
                if (left < 3 && e.Key == Key.Right)
                    left += 1;
            }
            switch (left)
            {
                case 1:
                    Border1.BorderThickness = new Thickness(2); // set up Border1 The border thickness is -2
                    Border2.BorderThickness = new Thickness(0);   // set up Border2 The border thickness is 0
                    Border3.BorderThickness = new Thickness(0);   // set up Border3 The border thickness is 0
                    break;
                case 2:
                    Border1.BorderThickness = new Thickness(0);   // set up Border1 The border thickness is 0
                    Border2.BorderThickness = new Thickness(2);  // set up Border2 The border thickness is -2
                    Border3.BorderThickness = new Thickness(0);   // set up Border3 The border thickness is 0
                    break;
                case 3:
                    Border1.BorderThickness = new Thickness(0);   // set up Border1 The border thickness is 0
                    Border2.BorderThickness = new Thickness(0);   // set up Border2 The border thickness is 0
                    Border3.BorderThickness = new Thickness(2);  // set up Border3 The border thickness is -2
                    break;
            }


        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            OnGameBegin();
        }

        int currentPage = -1;

        private void lastButton_Click(object sender, RoutedEventArgs e)
        {
            currentPage--;
            PageSwitch();
        }

        private void nextButton_Click(object sender, RoutedEventArgs e)
        {
            currentPage++;
            PageSwitch();
        }

        private void ignoreButton_Click(object sender, RoutedEventArgs e)
        {
            OnGameBegin();
        }

        async void PageSwitch()
        {
            switch (currentPage)
            {
                case 0:
                    {
                        page_panel.Visibility = Visibility.Visible;
						first_game.Visibility=Visibility.Collapsed;

						page_0.Visibility = Visibility.Visible;
                        page_1.Visibility = Visibility.Collapsed;

                        lastButton.IsEnabled = false;
                        nextButton.Content = "Next step";
						lastButton.Visibility = Visibility.Collapsed;
                        nextButton.Margin = new Thickness(329, 850, 0, 0);
						ignoreButton.Margin= new Thickness(770, 850,0, 0);

						await OnVoicePlayAsync(page_0_message.Text);
                    }
                    break;
                case 1:
                    {
                        page_panel.Visibility = Visibility.Visible;
						first_game.Visibility = Visibility.Collapsed;

						page_0.Visibility = Visibility.Collapsed;
                        page_1.Visibility = Visibility.Visible;

                        lastButton.IsEnabled = true;
                        nextButton.Content = "Trial";
						lastButton.Visibility = Visibility.Visible;
						nextButton.Margin = new Thickness(550, 850, 0, 0);
						ignoreButton.Margin = new Thickness(911, 850, 0, 0);

						await OnVoicePlayAsync(page_1_message.Text);
                    }
                    break;
                case 2:
                    {
                        page_panel.Visibility = Visibility.Collapsed;
						first_game.Visibility = Visibility.Visible;
                        //LJN, instructing the delegate in the explanation module
                        SetTitleVisibleAction?.Invoke(true);
                        RuleAction?.Invoke("A target picture will appear on the right side of the screen, and three pictures that are slightly different from the target picture will appear on the left side. Please select one of the pictures through the left and right keys of the keyboard to make it the same as the target picture on the right, and press theOKKey confirms selection.");//Add code, call function, display the text under the digital person
                        //LJN
                    }
                    break;
            }
        }
    }
}
