using crs.core;
using crs.core.DbModels;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Threading;

namespace crs.game.Games
{
    /// <summary>
    /// Space digital search.xaml Interaction logic
    /// </summary>
    public partial class Space digital search : BaseUserControl
    {
        private List<int> numbers;
        private int lastClickedNumber;
        private Stopwatch stopwatch;
        private DateTime startTime; // Record the start time displayed on the dial
        private double[] timeIntervals; // Storage time interval
        private int a;
        private int wrongAccount; // Record error count
        private int maxConsecutiveNumber; // Record the maximum value of the longest continuous string of numbers
        private Brush defaultButtonBackground; // Storage button default background color
        private DispatcherTimer gameTimer; // Timer
        private TimeSpan totalGameTime; // Total game time
        private double averagetime; // Added variables to store average reaction time
        private TimeSpan previousTime;  // Record the last correct click time
        private double leftTimes = 0;
        private double rightTimes = 0;
        private double[] reactionTimes; // Change place: Increase statistics to store the reaction time of each number
        private double Zvalue;//23 digital reaction time difference

        private void GameTimer_Tick(object sender, EventArgs e)
        {
            totalGameTime = totalGameTime.Add(TimeSpan.FromSeconds(1)); // One second each time
            int totalSeconds = (int)totalGameTime.TotalSeconds;

            // Calling delegate
            TimeStatisticsAction?.Invoke(totalSeconds, totalSeconds);

        }

        public Space digital search()
        {
            InitializeComponent();
            previousTime = TimeSpan.Zero;   // Initial setting to zero
        }

        private void InitializeNumberGrid()
        {
            numbers = Enumerable.Range(1, 24).ToList();
            Random rand = new Random();
            numbers = numbers.OrderBy(x => rand.Next()).ToList(); // Chaotic order
            foreach (var number in numbers)
            {
                Button button = new Button
                {
                    BorderThickness = new Thickness(0),
                    Content = number,
                    FontWeight = FontWeights.Bold, // Set font bold
                    FontSize = 32,
                    Margin = new Thickness(5),
                    Style = CreateCustomButtonStyle(),
                    Background = defaultButtonBackground // Set the initial background color of the button
                };
                button.Click += NumberButton_Click;
                NumberGrid.Children.Add(button);
            }
            Run textPart1 = new Run("Please find out: ")
            {
                Foreground = new SolidColorBrush(Colors.Black)
            };

            // Create and add orange text
            Run textPart2 = new Run((maxConsecutiveNumber + 1).ToString())
            {
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFB346"))
            };

            // Clear the current one Inlines
            tipblock.Inlines.Clear();

            // Add two parts to TextBlock
            tipblock.Inlines.Add(textPart1);
            tipblock.Inlines.Add(textPart2);

        }

        private async void NumberButton_Click(object sender, RoutedEventArgs e)
        {
            Button clickedButton = sender as Button;
            int clickedNumber = Convert.ToInt32(clickedButton.Content);
            if (clickedNumber < maxConsecutiveNumber + 1)
                return;
            TimeSpan reactionTime;
            if (maxConsecutiveNumber == 0 && clickedNumber == maxConsecutiveNumber + 1)
            {
                TimeSpan timeSinceStart = DateTime.Now - startTime;
                timeIntervals[0] = timeSinceStart.TotalSeconds;
                reactionTimes[0] = timeSinceStart.TotalMilliseconds; //   Change place: Recorded in millisecondsreactionTimesReaction time

                a = 0;
                maxConsecutiveNumber++;
                clickedButton.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFB346"));
                clickedButton.Foreground = Brushes.White; // Set the foreground color to white
                stopwatch.Restart();
            }
            else
            {

                if (clickedNumber == maxConsecutiveNumber + 1)
                {
                    TimeSpan timeInterval = stopwatch.Elapsed;
                    // Storage time interval
                    timeIntervals[maxConsecutiveNumber - 1] = timeInterval.TotalSeconds; // Store to the corresponding index

                    reactionTimes[clickedNumber - 1] = timeInterval.TotalMilliseconds; // Change place: Add reactionTimesArray saves each interval time, recording reaction time in milliseconds

                    maxConsecutiveNumber++;
                    clickedButton.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFB346"));
                    clickedButton.Foreground = Brushes.White; // Set the foreground color to white
                                                              // Debug.WriteLine($"number {clickedNumber} Response time: {reactionTimes[clickedNumber - 1]} millisecond");

                    stopwatch.Restart();

                    if (maxConsecutiveNumber > 1)
                    {
                        a = maxConsecutiveNumber - 1; // renew a The number of time intervals
                    }



                    // Check if all numbers have been clicked
                    if (maxConsecutiveNumber == 24)
                    {
                        string timeIntervalsMessage = "Time interval:\n";
                        string reactionTimesMessage = "Reaction time:\n"; // Change place: Added reaction time output

                        // Traversal timeIntervals Array and build message string
                        for (int i = 0; i < timeIntervals.Length; i++)
                        {
                            timeIntervalsMessage += $"1. {i + 1} Numbers: {timeIntervals[i]} millisecond\n";
                            reactionTimesMessage += $"1. {i + 1} Digital reaction time: {reactionTimes[i]} millisecond\n"; // Change place: Added reaction time output
                            Debug.WriteLine($"1. {i + 1} Digital reaction time: {reactionTimes[i]} millisecond");

                        }

                        // show MessageBox
                        //MessageBox.Show(timeIntervalsMessage, "Time interval statistics");
                        //Space digital search report nwd = new Space digital search report(wrongAccount, timeIntervals);
                        //nwd.Show();
                        stopwatch.Stop();
                        OnGameEnd();
                    }
                }
                else
                {
                    // Increase error count
                    wrongAccount++;
                    clickedButton.Background = Brushes.Black; // Set button background to black

                    // Waiting for 0.Restore color in 5 seconds
                    await Task.Delay(500); // Wait 500 milliseconds
                    clickedButton.Background = defaultButtonBackground; // Restore button background as default color
                    clickedButton.IsEnabled = true; // Re-enable button
                    //Debug.WriteLine($"The number was clicked incorrectly {clickedNumber}, continue to count...");
                }
            }
            Run textPart1 = new Run("Please find out: ")
            {
                Foreground = new SolidColorBrush(Colors.Black)
            };

            // Create and add orange text
            Run textPart2 = new Run((maxConsecutiveNumber + 1).ToString())
            {
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFB346"))
            };

            // Clear the current one Inlines
            tipblock.Inlines.Clear();

            // Add two parts to TextBlock
            tipblock.Inlines.Add(textPart1);
            tipblock.Inlines.Add(textPart2);
            RightStatisticsAction?.Invoke(maxConsecutiveNumber, 24);
            WrongStatisticsAction?.Invoke(wrongAccount, 24);
            // Update status
            lastClickedNumber = clickedNumber;

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            stopwatch.Stop();
            OnGameEnd();
        }
    }
    public partial class Space digital search : BaseUserControl
    {
        protected override async Task OnInitAsync()
        {

            lastClickedNumber = 0; // Initialize to 0, indicating that no click
            timeIntervals = new double[24];
            wrongAccount = 0;
            maxConsecutiveNumber = 0; // Initialize the maximum continuous number string to 0
            defaultButtonBackground = Brushes.White; // Set the default background color
            LevelStatisticsAction?.Invoke(0, 0);
            RightStatisticsAction?.Invoke(0, 10);
            WrongStatisticsAction?.Invoke(0, 10);
            // initialization averagetime
            averagetime = 0.0;
            Zvalue = 0.0;
            totalGameTime = TimeSpan.Zero; // Reset total time
            gameTimer = new DispatcherTimer();
            gameTimer.Interval = TimeSpan.FromSeconds(1); // Updated once every second
            gameTimer.Tick += GameTimer_Tick; // Bind timer event

            reactionTimes = new double[24]; // Initialize an array that stores 24 digital reaction times
        }

        protected override async Task OnStartAsync()
        {

            gameTimer.Start(); // Start the timer
            InitializeNumberGrid();

            stopwatch = new Stopwatch();
            startTime = DateTime.Now; // Record the start time displayed on the dial

            CustomCursor.Visibility = Visibility.Visible;//Call custom mouse cursor
            this.Cursor = Cursors.None; // Hide the default cursor
            CustomCursor.Width = 65; // Adjust to the width you want
            CustomCursor.Height = 65; // Adjust to the height you want
            // Bind the mouse movement event
            this.MouseMove += Game_MouseMove;


            // Calling delegate
            VoiceTipAction?.Invoke("Please click these squares in the order of numbers with the mouse.");
            SynopsisAction?.Invoke("Now you see the numbers in the order of disorder. Please use the mouse to click on the numbers in order. The faster you click these numbers, the better.！");
            RuleAction?.Invoke("Now you see the numbers in the order of disorder. Please use the mouse to click on the numbers in order. The faster you click these numbers, the better.！");//Add code, call function, display the text under the digital person
        }


        private void Game_MouseMove(object sender, MouseEventArgs e)//Bind custom mouse cursor and default mouse cursor
        {
            var position = e.GetPosition(CursorCanvas);
            Canvas.SetLeft(CustomCursor, position.X - CustomCursor.Width / 2);
            Canvas.SetTop(CustomCursor, position.Y - CustomCursor.Height / 2);
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
            stopwatch.Stop();
            gameTimer?.Stop();
            this.Cursor = Cursors.Arrow; // Restore the default cursor
            CustomCursor.Visibility = Visibility.Collapsed; // Hide custom cursor
        }

        protected override async Task OnPauseAsync()
        {
            stopwatch.Stop();
            gameTimer?.Stop();
        }

        protected override async Task OnNextAsync()
        {

            // Calling delegate
            VoiceTipAction?.Invoke("Please click these squares in the order of numbers with the mouse.");
            SynopsisAction?.Invoke("Now you see the numbers in the order of disorder. Please use the mouse to click on the numbers in order. The faster you click these numbers, the better.！");
            RuleAction?.Invoke("Now you see the numbers in the order of disorder. Please use the mouse to click on the numbers in order. The faster you click these numbers, the better.！");
        }//Add code, call function, display the text under the digital person

        protected override async Task OnReportAsync()
        {
            await updateDataAsync();

        }

        protected override IGameBase OnGetExplanationExample()
        {
            return new Explanation of spatial digital search();
        }

        private int GetCorrectNum()
        {
            return 0;
        }
        private int GetWrongNum()
        {
            return wrongAccount;
        }
        private int GetIgnoreNum()
        {
            return 0;
        }
        private double CalculateAccuracy(int correctCount, int wrongCount, int ignoreCount)
        {
            int totalAnswers = correctCount + wrongCount + ignoreCount;
            return totalAnswers > 0 ? Math.Round((double)correctCount / totalAnswers, 2) : 0;
        }

        /*private async Task updateDataAsync(int program_id)
        {
            using (Crs_Db2Context db = new Crs_Db2Context())
            using (var transaction = await db.Database.BeginTransactionAsync())
            {
                try
                {
                    // Get data at the current difficulty level
                    int correctCount = GetCorrectNum();
                    int wrongCount = GetWrongNum();
                    int ignoreCount = GetIgnoreNum();

                    // Calculation accuracy
                    double accuracy = CalculateAccuracy(correctCount, wrongCount, ignoreCount);
                    // create Result Record
                    var newResult = new Result
                    {
                        ProgramId = program_id,
                        Report = "Space digital search capability assessment report",
                        Eval = true,
                        Lv = null,
                        ScheduleId = BaseParameter.ScheduleId ?? null
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
                    ValueName = "Errors",
                    Value = wrongCount,
                    ModuleId = BaseParameter.ModuleId
                }
            };

                    // insert ResultDetail data
                    db.ResultDetails.AddRange(resultDetails);
                    await db.SaveChangesAsync();

                    foreach (var detail in resultDetails)
                    {
                        Debug.WriteLine($"    {detail.ValueName}: {detail.Value}, ModuleId: {detail.ModuleId}");
                    }

                    // Submit transactions
                    await transaction.CommitAsync();
                    Debug.WriteLine("Insert successfully");
                }
                catch (Exception ex)
                {
                    // Roll back transactions
                    await transaction.RollbackAsync();
                    Debug.WriteLine(ex.ToString());
                }
            }
        }*/
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
        private async Task updateDataAsync()
        {
            var baseParameter = BaseParameter;

            var program_id = baseParameter.ProgramId;
            Crs_Db2Context db = baseParameter.Db;

            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    int correctCount = GetCorrectNum();
                    int wrongCount = GetWrongNum();
                    int ignoreCount = GetIgnoreNum();
                    double accuracy = CalculateAccuracy(correctCount, wrongCount, ignoreCount);
                    db.Database.SetCommandTimeout(TimeSpan.FromMinutes(8)); // Adjust as needed
                    double sum = 0;

                    for (int ii = 0; ii < a; ii++)
                    {
                        sum += timeIntervals[ii]; // Accumulated valid time interval
                    }
                    averagetime = (sum / 24) * 1000; ; // Calculate the average reaction time（Unit: milliseconds）
                    double Zvalueworkspeed = (2235 - averagetime) / 530;
                    for (int ii = 0; ii < a - 1; ii++)
                    {
                        Zvalue += Math.Abs((timeIntervals[ii + 1] - timeIntervals[ii]));
                    }
                    Zvalue = (Zvalue * 1000) / 23;
                    for (int ii = 0; ii < 6; ii++)
                    {
                        leftTimes += (timeIntervals[numbers[(4 * ii)] - 1] + timeIntervals[numbers[(4 * ii + 1)] - 1]);
                        rightTimes += (timeIntervals[numbers[(4 * ii + 2)] - 1] + timeIntervals[numbers[(4 * ii + 3)] - 1]);
                    }
                    leftTimes /= 12;
                    rightTimes /= 12;

                    var newResult = new Result
                    {
                        ProgramId = program_id,
                        Report = "Space digital search capability assessment report",
                        Eval = true,
                        Lv = null,
                        ScheduleId = BaseParameter.ScheduleId ?? null
                    };
                    db.Results.Add(newResult);
                    await db.SaveChangesAsync();

                    int result_id = newResult.ResultId;

                    var resultDetails = reactionTimes.Select((value, index) => new ResultDetail
                    {
                        ResultId = result_id,
                        ValueName = "Numbers to be found,time[ms]",
                        Charttype = "Line chart",
                        Value = Math.Round(value, 2),
                        Abscissa = index + 1,
                        ModuleId = BaseParameter.ModuleId,
                    }).ToList();
                    resultDetails.Add(new ResultDetail
                    {
                        ResultId = result_id,
                        ValueName = "ZValue working speed",
                        Value = Math.Round(Zvalueworkspeed, 2), // Average time
                        ModuleId = BaseParameter.ModuleId
                    });
                    resultDetails.Add(new ResultDetail
                    {
                        ResultId = result_id,
                        ValueName = "Average reaction time",
                        Value = Math.Round(averagetime, 2), // Average time
                        ModuleId = BaseParameter.ModuleId
                    });
                    resultDetails.Add(new ResultDetail
                    {
                        ResultId = result_id,
                        ValueName = "Left average reaction time",
                        Value = Math.Round(leftTimes * 1000, 2), // Left average time
                        ModuleId = BaseParameter.ModuleId
                    });
                    resultDetails.Add(new ResultDetail
                    {
                        ResultId = result_id,
                        ValueName = "Right average reaction time",
                        Value = Math.Round(rightTimes * 1000, 2), // Right average time
                        ModuleId = BaseParameter.ModuleId
                    });
                    resultDetails.Add(new ResultDetail
                    {
                        ResultId = result_id,
                        ValueName = "Average of 23 differences(ms)",
                        Value = Math.Round(Zvalue, 2),
                        ModuleId = BaseParameter.ModuleId
                    });
                    resultDetails.Add(new ResultDetail
                    {
                        ResultId = result_id,
                        ValueName = "Errors",
                        Value = wrongCount,
                        ModuleId = BaseParameter.ModuleId
                    });
                    db.ResultDetails.AddRange(resultDetails);
                    db.SaveChanges();

                    foreach (var detail in resultDetails)
                    {
                        Debug.WriteLine($"    {detail.ValueName}: {detail.Value}, ModuleId: {detail.ModuleId}");
                    }

                    transaction.Commit();
                    Debug.WriteLine("Insert successfully");
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    Debug.WriteLine(ex.ToString());
                }
            }
        }

    }

}

