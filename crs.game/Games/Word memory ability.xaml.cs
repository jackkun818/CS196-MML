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
using System.Windows.Threading;

namespace crs.game.Games
{
    /// <summary>
    /// Word memory ability.xaml Interaction logic
    /// </summary>
    public partial class Word memory ability : BaseUserControl
    {
        public Action StopAction { get; set; }
        public Action<object> ProgressAction { get; set; }
        private DispatcherTimer gameTimer; // Timer
        private TimeSpan totalGameTime; // Total game time

        private List<string> wordsToMemorize = new List<string> { "apple fruit", "fragrant banana", "orange child", "West melon", "Grass Raspberry", "steam car", "since OK car", "Mo Hope car", "fly machine", "fire car", "lead Pen", "steel Pen", "oak Skin", "ruler child", "Pen remember Books" };
        private List<string> testWords = new List<string>
        { 
            // Fruits
            "pear", "peach child", "Portugal Grapes", "spinach radish", "pomelo child",
            "cherry peach", "Coconut child", "blue Raspberry", "black Raspberry", "lemon lemon",
            "acid orange", "Number eggplant", "red date", "Lili branch", "Mang fruit",
            "sweet melon", "Mountain bamboo", "none flower fruit", "orange child", "pomegranate lotus",
    
            // Transportation
            "wheel Boat", "electricity move car", "straight Lift machine", "Small Type fly machine", "foot tread car",
            "slip plate car", "male pay car", "goods car", "Seating car", "Journey tour car",
            "three wheel car", "Competition Mo Hope", "rail road pay Pass", "light rail", "high Fast train",
            "View Light -bar Senior", "machine move Boat", "Skin Sketch Boat", "Competition Boat", "since drive car",
    
            // Stationery
            "Book", "remember Number Pen", "Book Write plate", "Paper open", "arts Parts folder",
            "order Book machine", "Hope sign Paper", "Pen cylinder", "Paint change liquid", "painting Pen",
            "water color Pen", "painting Paper", "Pen remember Books electricity brain", "painting plate", "count Calculate Device",
            "Book sign", "Fast Write Books", "folder child", "Book Law Pen", "painting book"
        };
        private HashSet<string> memorizedWords = new HashSet<string>();
        List<string> result = new List<string>();
        private int score = 0;
        private int totalTests = 72; // Total number of tests
        private int currentTestCount = 0; // Current test times
        private int incorrectCount = 0; // Errors
        private int skippedCount = 0; // Number of skipped times

        private int[] incorrectcount;//Newly added line chart data: cumulative error count

        public Word memory ability()
        {
            InitializeComponent();
        }
        private void GameTimer_Tick(object sender, EventArgs e)
        {
            totalGameTime = totalGameTime.Add(TimeSpan.FromSeconds(1)); // One second each time
            // Get the total number of seconds
            int totalSeconds = (int)totalGameTime.TotalSeconds;

            // Calling delegate
            TimeStatisticsAction?.Invoke(totalSeconds, totalSeconds);

        }

        private void StartMemorizationPhase()
        {
            Random random = new Random();

            var selectedWords = wordsToMemorize.OrderBy(x => random.Next()).Take(5).ToList();
            foreach (var word in selectedWords)
            {
                memorizedWords.Add(word); // Save each word into the set
            }

            var availableTestWords = new List<string>(testWords);

            for (int i = 0; i < 6; i++)
            {
                var group = new List<string>();

                group.AddRange(selectedWords);

                var randomTestWords = availableTestWords.OrderBy(x => random.Next()).Take(7).ToList();
                group.AddRange(randomTestWords);

                foreach (var word in randomTestWords)
                {
                    availableTestWords.Remove(word);
                }

                group = group.OrderBy(x => random.Next()).ToList();

                result.AddRange(group);
            }
            WordTextBlock.Foreground = new SolidColorBrush(Colors.Black);
            EnterTestingPhase();
        }



        private void EnterTestingPhase()
        {
            if (currentTestCount >= totalTests)
            {
                ShowResults();
                return;
            }

            Random random = new Random();
            WordTextBlock.Text = result[currentTestCount]; // use currentTestCount As an index

            RightStatisticsAction?.Invoke(score, 10);
            WrongStatisticsAction?.Invoke((incorrectCount + skippedCount), 10);
        }


        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            string currentWord = WordTextBlock.Text;
            if (memorizedWords.Contains(currentWord) && currentTestCount >= 12)
            {
                score++;
            }
            else
            {
                incorrectCount++;
                if (currentTestCount >= 0)
                {
                    for (int i = (currentTestCount) / 12; i < incorrectcount.Length; i++)
                    {
                        incorrectcount[i]++;
                    }
                }
            }
            currentTestCount++; // Increase the number of tests and enter the next round
            RightStatisticsAction?.Invoke(score, 10);
            WrongStatisticsAction?.Invoke((incorrectCount + skippedCount), 10);
            EnterTestingPhase();
        }

        private void SkipButton_Click(object sender, RoutedEventArgs e)
        {
            string currentWord = WordTextBlock.Text;
            if (memorizedWords.Contains(currentWord) && currentTestCount >= 12)
            {
                skippedCount++;
                for (int i = (currentTestCount) / 12; i < incorrectcount.Length; i++)
                {
                    incorrectcount[i]++;
                }
            }
            currentTestCount++; // Increase the number of tests and enter the next round
            RightStatisticsAction?.Invoke(score, 10);
            WrongStatisticsAction?.Invoke((incorrectCount + skippedCount), 10);
            EnterTestingPhase();
        }

        private async void ShowResults()
        {
            // Trigger event
            //Vocabulary memory ability report nwd=new Word memory ability report(score,totalTests,incorrectCount,skippedCount);
            //nwd.Show();
            OnGameEnd();
            Button_Click(null, null);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            gameTimer?.Stop();
        }
        protected override void OnHostWindow_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Left)
            {
                OKButton_Click(sender, e);
            }
            if (e.Key == Key.Right)
            {
                SkipButton_Click(sender, e);
            }
        }
    }
    public partial class Word memory ability : BaseUserControl
    {
        protected override async Task OnInitAsync()
        {
            currentTestCount = 0; // Current test times
            incorrectCount = 0; // Errors
            skippedCount = 0; // Number of skipped times
            RightStatisticsAction?.Invoke(0, 10);
            WrongStatisticsAction?.Invoke(0, 10);

        }

        protected override async Task OnStartAsync()
        {
            totalGameTime = TimeSpan.Zero; // Reset total time
            gameTimer = new DispatcherTimer();
            gameTimer.Interval = TimeSpan.FromSeconds(1); // Updated once every second
            gameTimer.Tick += GameTimer_Tick; // Bind timer event
            gameTimer.Start(); // Start the timer
            incorrectcount = new int[6];
            for (int i = 0; i < incorrectcount.Length; i++)
            {
                incorrectcount[i] = 0;
            }
            StartMemorizationPhase();
            // Calling delegate
            VoiceTipAction?.Invoke("Please find the repeated words from the words that appear on the screen.");
            SynopsisAction?.Invoke("There will be some words repeated in this question,Please find the repeated words. When the repeated word appears, please click“yes”Button, otherwise click“no”Button");
            RuleAction?.Invoke("There will be some words repeated in this question,Please find the repeated words. When the repeated word appears, please click“yes”Button, otherwise click“no”Button");//Add code, call function, display the text under the digital person

        }

        protected override async Task OnStopAsync()
        {
            gameTimer?.Stop();
        }

        protected override async Task OnPauseAsync()
        {
            gameTimer?.Stop();
        }

        protected override async Task OnNextAsync()
        {

            // Calling delegate
            VoiceTipAction?.Invoke("Please find the repeated words from the words that appear on the screen.");
            SynopsisAction?.Invoke("There will be some words repeated in this question,Please find the repeated words. When the repeated word appears, please click“yes”Button, otherwise click“no”Button");
            RuleAction?.Invoke("There will be some words repeated in this question,Please find the repeated words. When the repeated word appears, please click“yes”Button, otherwise click“no”Button");//Add code, call function, display the text under the digital person
        }

        protected override async Task OnReportAsync()
        {
            await updateDataAsync();
        }

        protected override IGameBase OnGetExplanationExample()
        {
            return new Explanation of word memory ability();
        }

        private int GetCorrectNum()
        {
            return score;
        }
        private int GetWrongNum()
        {
            return incorrectCount + skippedCount;//Calculate the number of omissions into the number of errors
        }
        private int GetIgnoreNum()
        {
            return skippedCount;
        }
        //The accuracy rate is fixed with 25 as the denominator
        private double CalculateAccuracy(int correctCount)
        {
            const int totalRequiredCorrect = 25; // answer 25 That's it 100%
            return correctCount >= totalRequiredCorrect ? 1.0 : Math.Round((double)correctCount / totalRequiredCorrect, 2);
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
                        int diasCount = (correctCount - wrongCount);//Add deviation parameters
                        double totalMilliseconds = totalGameTime.TotalMilliseconds;
                        double time = Math.Round((double)totalMilliseconds / currentTestCount, 0);
                        // Calculation accuracy
                        double accuracy = CalculateAccuracy(correctCount);
                        double ZcorrectCount = Math.Round((double)(correctCount - 21.7) / 3, 2);//IncreasezValue correct parameters
                        double ZwrongCount = Math.Round((double)(wrongCount - 4) / 20.00, 2);//IncreasezValue Error Parameters
                        double ZdiasCount = Math.Round((double)((correctCount - wrongCount) - 21.5) / 3.4, 2);//IncreaseZValue language learning ability parameters

                        // create Result Record
                        var newResult = new Result
                        {
                            ProgramId = program_id, // program_id
                            Report = "Vocabulary memory ability assessment report",
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
                                    ValueName = "correct",
                                    Order = 0,
                                    Value = correctCount,
                                    ModuleId = BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    ValueName = "mistake",
                                    Order = 1,
                                    Value = wrongCount,
                                    ModuleId = BaseParameter.ModuleId
                                },
                                 new ResultDetail
                                {
                                    ResultId = result_id,
                                    ValueName = "deviation",
                                    Order = 2,
                                    Value = diasCount, // Stored as a percentage
                                    ModuleId = BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    ValueName = "Correct rate",
                                    Order = 3,
                                    Value = Math.Round(accuracy * 100, 2), // Stored as a percentage
                                    ModuleId = BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    ValueName = "Average reaction time(ms)",
                                    Order = 4,
                                    Value = Math.Round(time, 2), // Stored as a percentage
                                    ModuleId = BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    ValueName = "ZThe value is correct",
                                    Order = 5,
                                    Value = Math.Round(ZcorrectCount, 2),
                                    ModuleId = BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    ValueName = "ZError value",
                                    Order = 6,
                                    Value = Math.Round(ZwrongCount, 2),
                                    ModuleId = BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    ValueName = "ZValue language learning ability",
                                    Order = 7,
                                    Value = Math.Round(ZdiasCount, 2),
                                    ModuleId = BaseParameter.ModuleId
                                }
                            };

                        resultDetails.AddRange(incorrectcount.Select((value, index) => new ResultDetail
                        {
                            ResultId = result_id,
                            ValueName = "Task order(Group),Error number(indivual)",
                            Value = value,
                            Abscissa = index + 2,
                            Charttype = "Line chart",
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