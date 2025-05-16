using crs.extension.Models;
using HandyControl.Controls;
using System;
using System.Collections.Concurrent;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace crs.extension.Controls
{
    /// <summary>
    /// Crs_Rate.xaml 的交互逻辑
    /// </summary>
    public partial class Crs_Rate : UserControl
    {
        readonly Dictionary<int, Rate> rateDict = new Dictionary<int, Rate>();
        readonly int maxShowViewCount = 10;
        readonly int defaultRateCount = 1;

        public Crs_Rate()
        {
            InitializeComponent();
            this.Unloaded += Crs_Rate_Unloaded;

            var rateItem = new Rate { AllowHalf = true, IsReadOnly = true };
            Crs_Content = rateItem;
            defaultRateCount = rateItem.Count;
            rateDict.TryAdd(defaultRateCount, rateItem);
        }

        private void Crs_Rate_Unloaded(object sender, RoutedEventArgs e)
        {
            Crs_Content = null;
            rateDict.Clear();
        }

        public FrameworkElement Crs_Content
        {
            get { return (FrameworkElement)GetValue(Crs_ContentProperty); }
            set { SetValue(Crs_ContentProperty, value); }
        }

        public static readonly DependencyProperty Crs_ContentProperty =
            DependencyProperty.Register("Crs_Content", typeof(FrameworkElement), typeof(Crs_Rate), new PropertyMetadata(null));


        public StatisticsItem Crs_Source
        {
            get { return (StatisticsItem)GetValue(Crs_SourceProperty); }
            set { SetValue(Crs_SourceProperty, value); }
        }

        public static readonly DependencyProperty Crs_SourceProperty =
            DependencyProperty.Register("Crs_Source", typeof(StatisticsItem), typeof(Crs_Rate), new PropertyMetadata(null, Crs_SourceChangedCallback));

        static void Crs_SourceChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not Crs_Rate control)
            {
                return;
            }

            var item = e.NewValue as StatisticsItem;
            var count = item?.Count ?? control.defaultRateCount;
            var value = (double?)item?.Value ?? 0;

            //  按等级比例填充星星
            value = (double)value * Math.Min(count, control.maxShowViewCount) / count;
            count = Math.Min(count, control.maxShowViewCount);

            //  value =>最接近的整数或半整数
            value = (((value * 2) - (int)(value * 2)) < 0.5 ? (int)(value * 2) : (int)(value * 2 + 1)) / 2.0;

            Rate rateItem;
            if (control.rateDict.TryGetValue(count, out rateItem))
            {
                rateItem.Foreground = control.Foreground;
                rateItem.Value = value;
                if (control.Crs_Content != rateItem)
                {
                    control.Crs_Content = rateItem;
                }
                return;
            }

            rateItem = new Rate
            {
                Count = count,
                Value = value,
                AllowHalf = true,
                IsReadOnly = true,
                Foreground = control.Foreground
            };

            control.rateDict.TryAdd(count, rateItem);
            control.Crs_Content = rateItem;
        }
    }
}
