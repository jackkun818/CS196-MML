using System;
using System.Collections.Generic;
using System.Windows;

namespace crs.game.Games
{
    public partial class Vocabulary_memory_ability_report : Window
    {
        public Vocabulary_memory_ability_report(int score, int totalTests, int incorrectCount, int skippedCount)
        {
            InitializeComponent();

            // Create a data model instance
            var reportData = new ReportDataVocabulary memory ability
            {
                TotalTests = totalTests,
                Score = score,
                IncorrectCount = incorrectCount,
                SkippedCount = skippedCount,
                Date =DateTime.Now// Set date
            };

            // Bind the data model toDataGrid
            dataGrid.ItemsSource = new List<ReportDataVocabulary memory ability> { reportData };
        }
    }

    public class ReportDataVocabulary memory ability
    {
        public int TotalTests { get; set; }
        public int Score { get; set; }
        public int IncorrectCount { get; set; }
        public int SkippedCount { get; set; }
        public DateTime Date { get; set; } // Added date attribute
    }
}
