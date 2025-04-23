using crs.core.DbModels;
using crs.core;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Windows.Input;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using System.Runtime.InteropServices;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;

namespace crs.game.Games
{
    /// <summary>
    /// Broadness of memory.xaml Interaction logic
    /// </summary>
    public partial class Broadness_of_memory : BaseUserControl
    {

        /*20241110 New Requirements
         Now the highest level needs to be added to 9, that is, the maximum memory breadth reaches 9 squares that need to be remembered. The company now requires adding upgrades and downgrades. There are two questions at each level, and you need to do two consecutive tasks to reach the next level.
        If you do wrongly in the two questions in this level, whether the previous or the next one is done, you must return to the previous level, that is, if you do wrong, you will immediately be downgraded.
        The company's current requirement for the end is to do two questions wrong in a row, which means that the evaluation will be directly concluded by mistake in two questions in a row.
        */
        private const int GridSize = 5;
        private const int MAX_BLOCKS = 9; // Maximum number of blocks
        private const double DELAY = 1.0; // Time interval between two adjacent blocks
        private const int TOTAL_ROUNDS_PER_BLOCK = 2; // Total number of rounds for each block
        private List<Button> buttons = new List<Button>();
        private List<int> sequence = new List<int>();
        private List<int> selectedIndices = new List<int>(); // Record the index of the block selected by the player
        private int currentBlockCount; // The number of blocks currently displayed
        private int currentRound = 1; // Current round
        private Stopwatch stopwatch = new Stopwatch();
        private DispatcherTimer countdownTimer; // Countdown timer
        private List<DispatcherTimer> sequenceTimers = new List<DispatcherTimer>(); // Timer to store display blocks
        private Dictionary<int, int> errorCounts; // Record the number of errors per block
        // private Dictionary<int, int> correctCounts; // Record the correct number of times each block
        private bool isShowingSequence; // Are blocks being displayed
        private DispatcherTimer gameTimer; // Timer
        private TimeSpan totalGameTime; // Total game time

        //LJN,Add some bar chart statistics
        private string DateNow = DateTime.Now.ToString("yyyy/M/d"); // Get the current date
        private int[] CorrectNums = new int[MAX_BLOCKS-1];//The corresponding correct number of different memory breadths, such as[15,4,2,1]When the memory breadth 2 is answered correctly, 15 are answered correctly, and when the memory breadth 3 is answered correctly, that is,CorrectNums[0]It corresponds to the correct number of memory breadth 2.CorrectNums[1]Corresponding to the correct number of memory breadth 3；MAX_BLOCKS-1 is because the memory breadth starts from 2, whenMAX_BLOCKSWhen it is 4, there are 2 in total 3 4. The calculation unit is the number of questions
        private int MemorySpan = 2;//The maximum memory breadth is, the default starts from 2
        private int IfEverWrong = 0;//Used to record the number of consecutive errors, and end the evaluation directly with two consecutive errors.
        private int[] PositionErrorNums = new int[MAX_BLOCKS - 1];//The position error means that you clicked the memory display blocks to calculate the position error, and the calculation unit is the number of questions.
        private int[] OrrderErrorNums = new int[MAX_BLOCKS - 1];//The order is wrong, the order is that the squares you point are all the squares that he displays the key points, but the order is wrong, the calculation unit is the number of questions
        public Broadness_of_memory()
        {
            InitializeComponent();
        }

        private void InitializeGrid()
        {
            GameGrid.Children.Clear();
            buttons.Clear(); // Clear the button list at the same time
            for (int i = 0; i < GridSize; i++)
            {
                for (int j = 0; j < GridSize; j++)
                {
                    Button button = new Button
                    {
                        //LJN,Modify the color
                        Background = Brushes.White, // Set the initial background color to gray
                        //LJN,Modify spacing 2->5
                        Margin = new Thickness(5),
                        FontSize = 24, // Set the default font size
                        Content = "", // The content of the initialization button is empty
                       //LJN,Apply custom styles
                        Style = CreateCustomButtonStyle() // Apply custom styles
                    };
                    button.Click += Button_Click;
                    Grid.SetRow(button, i);
                    Grid.SetColumn(button, j);
                    GameGrid.Children.Add(button);
                    buttons.Add(button);
                }
            }
        }
        //Set all blocks to gray,Clear button content
        private void ResetGridGray()
        {
            foreach (var item in buttons)
            {
                //LJN,Modified to white
                item.Background = Brushes.White;
                item.Content = "";
            }
        }

        private void StartGame()
        {
            sequence.Clear();
            ResetGridGray();
            isShowingSequence = true;
            StatusTextBlock.Text = "Ready to start..."; // Set up text to start
            StartCountdown();
            //LJN,Used to display pictures
            CreateImage();
            OnGameStart();
        }

        private void GameTimer_Tick(object sender, EventArgs e)
        {
            totalGameTime = totalGameTime.Add(TimeSpan.FromSeconds(1)); // One second each time// Get the total number of seconds
            int totalSeconds = (int)totalGameTime.TotalSeconds;

            // Calling delegate
            TimeStatisticsAction?.Invoke(totalSeconds, totalSeconds);

        }

        private void StartCountdown()
        {
            countdownTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            int countdownTime = 5; // 5 seconds countdown
            countdownTimer.Tick += (s, args) =>
            {
                if (countdownTime > 0)
                {
                    //LJN,Before countdown starts, make the text appear
                    StatusTextBlock.Visibility = Visibility.Visible;
                    StatusTextBlock.Text = $"Countdown: {countdownTime}Second, Current display {currentBlockCount} Squares, 1.{currentRound}wheel";
                    countdownTime--;
                }
                else
                {
                    countdownTimer.Stop();
                    ShowNextRound();
                }
            };
            countdownTimer.Start();
        }

        private void ShowNextRound()
        {
            int numberToShow = currentBlockCount;
            sequence.Clear();
            Random rand = new Random();
            HashSet<int> shownIndices = new HashSet<int>();

            for (int i = 0; i < numberToShow; i++)
            {
                int index;
                do
                {
                    index = rand.Next(buttons.Count);
                } while (shownIndices.Contains(index));

                shownIndices.Add(index);
                sequence.Add(index);
            }
            stopwatch.Restart();
            ShowSequence(0);
        }
        private void OnGameStart()//Call custom mouse cursor function
        {
            this.Cursor = Cursors.None; // Hide the default cursor
            CustomCursor.Visibility = Visibility.Visible; // Show custom cursor
            MouseMove += Window_MouseMove; // subscription MouseMove event
            CustomCursor.Width = 65; // Adjust to the width you want
            CustomCursor.Height = 65; // Adjust to the height you want
        }
     
        private void ShowSequence(int index)
        {
            if (index < sequence.Count)
            {
                int buttonIndex = sequence[index];
                buttons[buttonIndex].Background = Brushes.Red;

                DispatcherTimer timer = new DispatcherTimer
                {
                    Interval = TimeSpan.FromSeconds(DELAY)
                };
                timer.Tick += (s, args) =>
                {
                    //LJN,With the countdown,buttonGo back to white instead of gray
                    buttons[buttonIndex].Background = Brushes.White; // Hide blocks
                    timer.Stop();
                    sequenceTimers.Remove(timer); // Remove the timer
                    ShowSequence(index + 1); // Show the next block
                };
                timer.Start();
                sequenceTimers.Add(timer); // Add to list
            }
            else
            {
                // All blocks are displayed and prompt the user
                isShowingSequence = false;
                StatusTextBlock.Text = "Now please press the corresponding blocks in turn";
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (isShowingSequence)
            {
                return; // If the block is being displayed, return directly and do not handle the click event
            }

            Button clickedButton = sender as Button;
            int clickedIndex = buttons.IndexOf(clickedButton);

            // Uncheck if the button has been selected
            if (selectedIndices.Contains(clickedIndex))
            {
                selectedIndices.Remove(clickedIndex);
                //LJN,Restore to white
                clickedButton.Background = Brushes.White; // Revert to gray
                clickedButton.Content = ""; // Clear content
            }
            else
            {
                // Add selected blocks
                selectedIndices.Add(clickedIndex);
                //LJN,Turns orange when the square is selected
                clickedButton.Background = Brushes.Orange; // Turns red
                clickedButton.Content = (selectedIndices.Count).ToString(); // Shows the current number of blocks clicked
                clickedButton.Foreground = Brushes.White; // Set the font color to white
                clickedButton.FontSize = 36; // Increase font size
            }

            // Determine the number of selected blocks
            if (selectedIndices.Count == sequence.Count)
            {
                int errors = 0; // Calculate the number of errors
                isShowingSequence = true;
                // Clean up selected blocks and provide feedback
                for (int i = 0; i < sequence.Count; i++)
                {
                    if (sequence[i] == selectedIndices[i])
                    {
                        buttons[selectedIndices[i]].Background = Brushes.Green; // Correct options
                        buttons[selectedIndices[i]].Content = "✔"; // Show correct mark

                    }
                    else
                    {
                        buttons[selectedIndices[i]].Background = Brushes.Red; // Wrong option
                        buttons[selectedIndices[i]].Content = "✖"; // Show error mark
                        errorCounts[currentBlockCount]++;
                        errors++;                      
                    }
                }
                if(errors!=0)//It means there is an error to determine what type of error the problem is.
                {
                    if (new HashSet<int>(sequence).SetEquals(selectedIndices))
                    {//Determine error information and compare two arrays
                        //If the elements contained in the two arrays are exactly the same, it means that the order is wrong
                        OrrderErrorNums[currentBlockCount - 2] += 1;
                    }
                    else
                    {//Otherwise it's just a location error
                        PositionErrorNums[currentBlockCount - 2] += 1;
                    }
                }

                // Show feedback information
                string feedbackText;
                SolidColorBrush feedbackColor;

                if (errors == 0)
                {
                    feedbackText = "correct！";
                    feedbackColor = Brushes.Green; // Correctly green
                    //LJN,Show pictures, hide text
                    CorrectImage.Visibility = Visibility.Visible;
                    StatusTextBlock.Visibility = Visibility.Collapsed;
                    //LJN,Record the correct number of numbers at each breadth
                    CorrectNums[currentBlockCount - 2] += 1;
                }
                else
                {
                    feedbackText = "mistake！";
                    feedbackColor = Brushes.Red; // Error is red
                    //LJN,Show pictures, hide text
                    ErrorImage.Visibility = Visibility.Visible;
                    StatusTextBlock.Visibility = Visibility.Collapsed;
                }

                // Update status text
                StatusTextBlock.Text = feedbackText;
                StatusTextBlock.Foreground = feedbackColor; // Set font color

                selectedIndices.Clear(); // Clear the selected index

                // Create a timer to restore state text color
                DispatcherTimer resetTimer = new DispatcherTimer
                {
                    Interval = TimeSpan.FromSeconds(3) // Recover in 3 seconds
                };
                resetTimer.Tick += (s, args) =>
                {
                    foreach (var button in buttons)
                    {
                        //LJN,Return to white
                        button.Background = Brushes.White; // Revert to gray
                        button.Content = ""; // Clear content
                        button.Foreground = Brushes.Black; // Restore font color to black
                    }
                    StatusTextBlock.Foreground = Brushes.Black; // Restore the font color to black
                    resetTimer.Stop(); // Stop the timer
                    resetTimer = null; // Clear timer reference
                    StatusTextBlock.Text = "";
                    //LJN,Hide the image feedback
                    ErrorImage.Visibility = Visibility.Collapsed;
                    CorrectImage.Visibility = Visibility.Collapsed;

                    // Continue the game logic
                    currentRound++; // Increase the current round
                    isShowingSequence = true;
                    if(errors!=0)//If this question has been wrong, then immediately return to the next level
                    {
                        currentRound = 1; // Reset the current round

                        IfEverWrong++;//This is used to record whether it is done continuously or not.
                        if(IfEverWrong>=2)
                        {//Two mistakes in a row, end the game directly
                         // The game ends, displays error statistics
                         //Memory Broadness Report nwd = new Memory Broadness Report(errorCounts);
                         //nwd.Show();
                            StopAllTimers();
                            //LJN,Before the game ends, update the value of memory breadth
                            MemorySpan = FindMaxMemorySpan();



                            OnGameEnd();
                        }
                        currentBlockCount--;                  //Block count fallback

                        if (currentBlockCount<2) currentBlockCount = 2;//Make minimum limit
                        StartCountdown(); // Go to the next round
                    }
                    else if (currentRound <= TOTAL_ROUNDS_PER_BLOCK)//The current round is less than the required round, and no error has occurred yet
                    {
                        if (IfEverWrong > 0) IfEverWrong = 0;//The normal answer is correct and the next round should be zero.
                        StartCountdown(); // Go to the next round
                    }
                    else
                    {
                        currentRound = 1; // Reset the current round
                        currentBlockCount++; // Increase the number of blocks
                        if (currentBlockCount <= MAX_BLOCKS)
                        {
                            if (IfEverWrong > 0) IfEverWrong = 0;//The normal answer is correct and the next round should be zero.
                            StartCountdown(); // Conduct the next round of countdown
                        }
                        else
                        {
                            // The game ends, displays error statistics
                            //Memory Broadness Report nwd = new Memory Broadness Report(errorCounts);
                            //nwd.Show();
                            StopAllTimers();

                            //LJN,Before the game ends, update the value of memory breadth
                            MemorySpan = FindMaxMemorySpan();

                            OnGameEnd();

                        }
                    }
                };
                resetTimer.Start(); // Start the timer
            }
        }

        private void StopAllTimers()
        {
            // Stop countdown timer
            countdownTimer?.Stop();
            gameTimer?.Stop();
            // Stop the timer for all display blocks
            foreach (var timer in sequenceTimers)
            {
                timer.Stop();
            }
            sequenceTimers.Clear(); // Clear the timer list
        }

        //LJN,Add some functions and styles
        private Style CreateCustomButtonStyle()
        {//Pack a style
            // Create button style
            Style buttonStyle = new Style(typeof(Button));

            // Set background to white
            buttonStyle.Setters.Add(new Setter(Button.BackgroundProperty, Brushes.White));

            // Set shadow effect
            ////LJN,Add shadow effect
            //Effect = new DropShadowEffect
            //{
            //    Color = Colors.Gray,      // Shadow color
            //    BlurRadius = 10,          // Fuzzy radius
            //    ShadowDepth = 5,          // Shadow depth
            //    Direction = 315,          // Shadow direction, angle
            //    Opacity = 0.5             // Shadow Transparency
            //},
            buttonStyle.Setters.Add(new Setter(Button.EffectProperty, new DropShadowEffect
            {
                Color = Colors.Gray,
                BlurRadius = 10,
                ShadowDepth = 5,
                Direction = 315,
                Opacity = 0.5
            }));

            // Custom templates to remove default visual changes when mouse over
            ControlTemplate template = new ControlTemplate(typeof(Button));
            FrameworkElementFactory border = new FrameworkElementFactory(typeof(Border));
            border.SetValue(Border.BackgroundProperty, new TemplateBindingExtension(Button.BackgroundProperty));
            border.SetValue(Border.BorderBrushProperty, new TemplateBindingExtension(Button.BorderBrushProperty));
            border.SetValue(Border.BorderThicknessProperty, new TemplateBindingExtension(Button.BorderThicknessProperty));

            ////LJN,Cancel the length and width setting, keep filling
            //HorizontalAlignment = HorizontalAlignment.Stretch,  // Set horizontal fill
            //VerticalAlignment = VerticalAlignment.Stretch,   // Set vertical fill
            FrameworkElementFactory contentPresenter = new FrameworkElementFactory(typeof(ContentPresenter));
            contentPresenter.SetValue(ContentPresenter.HorizontalAlignmentProperty, HorizontalAlignment.Center);
            contentPresenter.SetValue(ContentPresenter.VerticalAlignmentProperty, VerticalAlignment.Center);

            border.AppendChild(contentPresenter);
            template.VisualTree = border;

            ////LJN,Make the mouse not change when it moves abovebuttonThe color of
            //FocusVisualStyle = null
            // Triggers to ensure that the background remains unchanged while the mouse is hovered
            System.Windows.Trigger isMouseOverTrigger = new System.Windows.Trigger { Property = Button.IsMouseOverProperty, Value = true };
            isMouseOverTrigger.Setters.Add(new Setter(Button.BackgroundProperty, Brushes.White));
            template.Triggers.Add(isMouseOverTrigger);

            // Set the template to style
            buttonStyle.Setters.Add(new Setter(Button.TemplateProperty, template));

            return buttonStyle;
        }
        //LJN,Add some functions and styles
        private void CreateImage()
        {//Create two images from localJIYIFolder reading
            //First get the full path to the image
            string currentDirectory = AppDomain.CurrentDomain.BaseDirectory;            // Get the root directory of the current project(Right nowbinTable of contents)
            //@ Symbols are used to define verbatim strings（verbatim string）, make special characters in the string（Such as backslash \）Explained as is and no longer required to escape.

            // Construct the relative path of the image
            string correctRelativePath = @"Games\pic\JIYI\Correct.png";
            string errorRelativePath = @"Games\pic\JIYI\Error.png";

            // use Path.Combine To splice the absolute path
            string correctImagePath = System.IO.Path.Combine(currentDirectory, correctRelativePath);
            string errorImagePath = System.IO.Path.Combine(currentDirectory, errorRelativePath);
            CorrectImage.Source = new BitmapImage(new Uri(correctImagePath, UriKind.RelativeOrAbsolute));
            ErrorImage.Source = new BitmapImage(new Uri(errorImagePath, UriKind.RelativeOrAbsolute));
            ErrorImage.Visibility = Visibility.Collapsed;
            CorrectImage.Visibility = Visibility.Collapsed;
        }

        private int FindMaxMemorySpan()
        {
            int maxIndex = -1;
            for (int i = 0; i < CorrectNums.Length; i++)
            {
                if (CorrectNums[i] > 0) // Determine whether there is any result
                {
                    maxIndex = i; // Update the maximum index
                }
            }

            if (maxIndex == -1)
            {
                // If no value is greater than 0, returnnullIndicates that there is no valid result
                return 2;
            }

            // The memory breadth starts from 2, so the index needs to be added to 2.
            return maxIndex + 2;
        }
    }
    public partial class Broadness_of_memory : BaseUserControl
    {

        protected override async Task OnInitAsync()
        {
            currentBlockCount = 2; // Start with 2 blocks
            InitializeGrid();
            errorCounts = new Dictionary<int, int>(); // Initialization error dictionary

            for (int i = 2; i <= MAX_BLOCKS; i++)
            {
                errorCounts[i] = 0; // Initialize to 0
            }
            // Calling delegate
            LevelStatisticsAction?.Invoke(currentBlockCount, MAX_BLOCKS);
            RightStatisticsAction?.Invoke(0, 10);
            WrongStatisticsAction?.Invoke(0, 10);

            totalGameTime = TimeSpan.Zero; // Reset total time
            gameTimer = new DispatcherTimer();
            gameTimer.Interval = TimeSpan.FromSeconds(1); // Updated once every second
            gameTimer.Tick += GameTimer_Tick; // Bind timer event
        }

        protected override async Task OnStartAsync()
        {

            gameTimer.Start(); // Start the timer
            StartGame();
            // Calling delegate
            VoiceTipAction?.Invoke("Please click the squares in order after the block is hidden.");
            SynopsisAction?.Invoke("Now you see 5×5 squares, and then the squares will be displayed in order. Please remember them. After the square is hidden, click the squares in sequence.");
            RuleAction?.Invoke("Now you see 5×5 squares, and then the squares will be displayed in order. Please remember them. After the square is hidden, click the squares in sequence.");//Add code, call function, display the text under the digital person
        }
        private void Window_MouseMove(object sender, MouseEventArgs e)//Bind custom mouse cursor and default mouse cursor
        {
            Point position = e.GetPosition(this);
            Canvas.SetLeft(CustomCursor, position.X - (CustomCursor.Width / 2));
            Canvas.SetTop(CustomCursor, position.Y - (CustomCursor.Height / 2));
            // Get Canvas The boundary of
            double canvasLeft = Canvas.GetLeft(CustomCursor);
            double canvasTop = Canvas.GetTop(CustomCursor);
            double canvasWidth = this.ActualWidth;
            double canvasHeight = this.ActualHeight;

            // Get CustomCursor width and height
            double cursorWidth = CustomCursor.Width;
            double cursorHeight = CustomCursor.Height;

            // if CustomCursor More than Canvas boundaries, cut
            if (canvasLeft + cursorWidth > canvasWidth)
            {
                Canvas.SetLeft(CustomCursor, canvasWidth - cursorWidth); // Limit to the right boundary
            }
            if (canvasTop + cursorHeight > canvasHeight)
            {
                Canvas.SetTop(CustomCursor, canvasHeight - cursorHeight); // Limit to the lower boundary
            }
            if (canvasLeft < 0)
            {
                Canvas.SetLeft(CustomCursor, 0); // Limit to the left border
            }
            if (canvasTop < 0)
            {
                Canvas.SetTop(CustomCursor, 0); // Limit to the upper boundary
            }

            // if CustomCursor Exceeded Canvas Range, cut out the excess
            Rect clipRect = new Rect(0, 0, canvasWidth, canvasHeight);
            CustomCursor.Clip = new RectangleGeometry(clipRect);  // Crop areas using rectangles

        }


        protected override async Task OnStopAsync()
        {
            this.Cursor = Cursors.Arrow; // Restore the default cursor
            CustomCursor.Visibility = Visibility.Collapsed; // Hide custom cursor
            MouseMove -= Window_MouseMove; // Unsubscribe MouseMove event

            MemorySpan = FindMaxMemorySpan();


            StopAllTimers();
        }

        protected override async Task OnPauseAsync()
        {
            StopAllTimers();
        }

        protected override async Task OnNextAsync()
        {
            // Calling delegate
            VoiceTipAction?.Invoke("Please click the squares in order after the block is hidden.");
            SynopsisAction?.Invoke("Now you see 5×5 squares, and then the squares will be displayed in order. Please remember them. After the square is hidden, click the squares in sequence.");
            RuleAction?.Invoke("Now you see 5×5 squares, and then the squares will be displayed in order. Please remember them. After the square is hidden, click the squares in sequence.");//Add code, call function, display the text under the digital person
        }

        protected override async Task OnReportAsync()
        {
            await updateDataAsync();
        }

        protected override IGameBase OnGetExplanationExample()
        {
            return new Explanation_of_memory_breadth();
        }

        private int GetCorrectNum()
        {
            // 18 ？？？

            //return 2* currentBlockCount-errorCounts[currentBlockCount];
            //return correctCounts.Values.Sum();
            return CorrectNums.Sum();
        }
        private int GetWrongNum()
        {
            return errorCounts.Values.Sum();
        }
        private int GetIgnoreNum()
        {
            return 0;
        }

        private int GetPosErrorNum()
        {
            return PositionErrorNums.Sum();
        }

        private int GetOrderErrorNum()
        {
            return OrrderErrorNums.Sum();
        }


        private double CalculateAccuracy(int correctCount, int wrongCount, int ignoreCount)
        {
            int totalAnswers = correctCount + wrongCount + ignoreCount;
            return totalAnswers > 0 ? Math.Round((double)correctCount / totalAnswers, 2) : 0;
        }

        private async Task updateDataAsync()
        {
            var baseParameter = BaseParameter;

            var program_id = baseParameter.ProgramId;
            Crs_Db2Context db = baseParameter.Db;

            using (var transaction = await db.Database.BeginTransactionAsync())
            {
                try
                {
                    await Task.Run(async () =>
                    {
                        // Get data at the current difficulty level
                        int correctCount = GetCorrectNum();
                        int wrongCount = GetWrongNum();
                        int ignoreCount = GetIgnoreNum();
                        int positionErrorCount = GetPosErrorNum();
                        int orderErrorCount = GetOrderErrorNum();
                        double Zvalue = (MemorySpan - 3.1) / 0.8;
                        // Calculation accuracy
                        double accuracy = CalculateAccuracy(correctCount, wrongCount, ignoreCount);
                        // create Result Record
                        var newResult = new Result
                        {
                            ProgramId = program_id, // program_id
                            Report = "Logical reasoning ability assessment report",
                            Eval = true,
                            Lv = null, // Current difficulty level
                            ScheduleId = BaseParameter.ScheduleId ?? null // Assumption Schedule_id, can be replaced with actual
                        };
                        db.Results.Add(newResult);
                        await db.SaveChangesAsync();
                        // get result_id
                        int result_id = newResult.ResultId;
                        // create ResultDetail Object List
                        var resultDetails = new List<ResultDetail>
                            {
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    Order = 1,
                                    ValueName = "Minimum level",
                                    Value = 2,
                                    ModuleId = BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    Order = 2,
                                    ValueName = "Maximum level",
                                    Value =  MemorySpan ,
                                    ModuleId = BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    Order = 3,
                                    ValueName = "correct",
                                    Value = correctCount , // Stored as a percentage
                                    ModuleId = BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    ValueName = "Position error",
                                    Value = positionErrorCount,
                                    ModuleId = BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    ValueName = "Incorrect order",
                                    Value = orderErrorCount,
                                    ModuleId = BaseParameter.ModuleId
                                } ,
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    ValueName = "ZValue memory breadth",
                                    Value = Math.Round(Zvalue, 2),
                                    ModuleId = BaseParameter.ModuleId
                                }
                            };

                        resultDetails.AddRange(CorrectNums.Select((value, index) => new ResultDetail
                        {
                            ResultId = result_id,
                            ValueName = "Broadness of memory,correct",
                            Value = value,
                            Abscissa = index + 2,
                            Charttype = "Bar chart",
                            ModuleId = BaseParameter.ModuleId
                        }).ToList());

                        // insert ResultDetail data
                        db.ResultDetails.AddRange(resultDetails);
                        await db.SaveChangesAsync();
                        // Output each ResultDetail Object data
                        /*Debug.WriteLine($"Difficulty level {lv}:");*/
                        foreach (var detail in resultDetails)
                        {
                            Debug.WriteLine($"    {detail.ValueName}: {detail.Value}, ModuleId: {detail.ModuleId}");
                        }

                        // Submit transactions
                        await transaction.CommitAsync();
                        Debug.WriteLine("Insert successfully");
                    });
                }
                catch (Exception ex)
                {
                    // Roll back transactions
                    await transaction.RollbackAsync();
                    Debug.WriteLine(ex.ToString());
                }
            }

        }
    }
}
