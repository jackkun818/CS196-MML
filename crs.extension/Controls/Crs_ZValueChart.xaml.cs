using crs.extension.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace crs.extension.Controls
{
    /// <summary>
    /// Crs_ZValueChart.xaml 的交互逻辑
    /// </summary>
    public partial class Crs_ZValueChart : UserControl
    {
        static readonly ObservableCollection<KeyValueItem<string, double>> DefaultZValueChartItems
            = new ObservableCollection<KeyValueItem<string, double>>();
        public Crs_ZValueChart()
        {
            InitializeComponent();
        }


        public ObservableCollection<KeyValueItem<string, double>> ZValueChartItems
        {
            get { return (ObservableCollection<KeyValueItem<string, double>>)GetValue(ZValueChartItemsProperty); }
            set { SetValue(ZValueChartItemsProperty, value); }
        }

        public static readonly DependencyProperty ZValueChartItemsProperty =
            DependencyProperty.Register("ZValueChartItems", typeof(ObservableCollection<KeyValueItem<string, double>>), typeof(Crs_ZValueChart), new PropertyMetadata(Crs_ZValueChart.DefaultZValueChartItems, ZValueChartItemsChangedCallback));

        static void ZValueChartItemsChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Crs_ZValueChart zValueChart)
            {
                var oldItem = e.OldValue as ObservableCollection<KeyValueItem<string, double>>;
                if (oldItem != null && !oldItem.Equals(Crs_ZValueChart.DefaultZValueChartItems))
                {
                    oldItem.CollectionChanged -= ZValuesChanged;
                }

                var newItem = e.NewValue as ObservableCollection<KeyValueItem<string, double>>;
                if (newItem != null && !newItem.Equals(Crs_ZValueChart.DefaultZValueChartItems))
                {
                    newItem.CollectionChanged += ZValuesChanged;
                }

                OnZValuesChanged(false);

                void ZValuesChanged(object sender, NotifyCollectionChangedEventArgs e) => OnZValuesChanged();

                void OnZValuesChanged(bool setData = true)
                {
                    var zValues = zValueChart.ZValueChartItems;
                    if (zValues != null)
                    {
                        if (zValues.Count == 0)
                        {
                            zValueChart.Z_Standard.Visibility = Visibility.Collapsed;
                        }
                        else
                        {
                            zValueChart.Z_Standard.Visibility = Visibility.Visible;
                        }
                    }

                    //foreach (var item in zValueChart.itemsControl.Items)
                    //{
                    //    // 将item转换为FrameworkElement，以便可以使用FindName
                    //    FrameworkElement container = zValueChart.itemsControl.ItemContainerGenerator.ContainerFromItem(item) as FrameworkElement;
                    //    if (container != null)
                    //    {
                    //        // 使用FindName查找zValueTextBlock和zValueBar
                    //        TextBlock zValueTextBlock = container.FindName("zValueTextBlock") as TextBlock;
                    //        Rectangle zValueBar = container.FindName("zValueBar") as Rectangle;
                    //        var zValue = zValues.FirstOrDefault(m => m.Key.Equals(zValueTextBlock.Text));
                    //        if (zValue.Value > 0)
                    //        {
                    //            if (zValueTextBlock != null && zValueBar != null)
                    //            {
                    //                Grid parentGrid = zValueTextBlock.Parent as Grid;
                    //                if (parentGrid != null)
                    //                {
                    //                    Grid.SetColumn(zValueTextBlock, 4);
                    //                }
                    //                double newWidth = zValue.Value switch
                    //                {
                    //                    <= 3.0 => zValue.Value * 160,
                    //                    > 3.0 => 480,
                    //                    _ => 0
                    //                };
                    //                zValueTextBlock.HorizontalAlignment = HorizontalAlignment.Left;
                    //                zValueTextBlock.TextAlignment= TextAlignment.Right;
                    //                zValueTextBlock.Width = newWidth;
                    //                zValueBar.Width = newWidth;
                    //            }
                    //        }
                    //        else if (zValue.Value < 0)
                    //        {
                    //            if (zValueTextBlock != null && zValueBar != null)
                    //            {
                    //                Grid parentGrid = zValueTextBlock.Parent as Grid;
                    //                if (parentGrid != null)
                    //                {
                    //                    Grid.SetColumn(zValueTextBlock, 3);
                    //                }
                    //                double newWidth = zValue.Value switch
                    //                {
                    //                    >= -4.0 => zValue.Value * -160,
                    //                    < -4.0 => 640,
                    //                    _ => 0
                    //                };
                    //                zValueTextBlock.HorizontalAlignment = HorizontalAlignment.Right;
                    //                zValueTextBlock.TextAlignment = TextAlignment.Left;
                    //                zValueTextBlock.Width = newWidth;
                    //                zValueBar.Width = newWidth;
                    //            }
                    //        }
                    //        else
                    //        {
                    //            if (zValueTextBlock != null && zValueBar != null)
                    //            {
                    //                Grid parentGrid = zValueTextBlock.Parent as Grid;
                    //                if (parentGrid != null)
                    //                {
                    //                    Grid.SetColumn(zValueTextBlock, 4);
                    //                }
                    //                double newWidth = 0;
                    //                zValueTextBlock.HorizontalAlignment = HorizontalAlignment.Left;
                    //                zValueTextBlock.Width = newWidth;
                    //                zValueBar.Width = newWidth;
                    //            }
                    //        }
                    //    }



                    //}
                }
            }

        }
    }
    public class ZValueChartDoubleToWidthConverter1 : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double)
            {
                double val = (double)value;
                if (val < 0)
                {
                    return val switch
                    {
                        >= -4.0 => val * -160,
                        < -4.0 => 640,
                        _ => 0
                    };
                }
                else
                {
                    return val switch
                    {
                        <= 3.0 => val * 160,
                        > 3.0 => 480,
                        _ => 0
                    };
                }
            }
            return 0;
        }


        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

    }
    public class ZValueChartDoubleToWidthConverter2 : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double)
            {
                double val = (double)value;
                if (val < 0)
                {
                    return val switch
                    {
                        >= -4.0 => val * -160 * 590.0 / 640.0 + 50,
                        < -4.0 => 640,
                        _ => 2
                    };
                }
                else
                {
                    return val switch
                    {
                        <= 3.0 => val * 160 * 430.0 / 480.0 + 50,
                        > 3.0 => 480,
                        _ => 2
                    };
                }
            }
            return 0;
        }


        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

    }

    public class ZValueChartDoubleToHorizontalAlignmentConverter1 : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double)
            {
                double val = (double)value;
                return val > 0 ? HorizontalAlignment.Left : HorizontalAlignment.Right;
            }
            return HorizontalAlignment.Stretch; // 默认值
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class ZValueChartDoubleToHorizontalAlignmentConverter2 : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double)
            {
                double val = (double)value;
                return val < 0 ? HorizontalAlignment.Left : HorizontalAlignment.Right;
            }
            return HorizontalAlignment.Stretch; // 默认值
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }


    public class ZValueChartDoubleToMarginConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double)
            {
                double val = (double)value;

                if (val > 0)
                {
                    return new Thickness(990, 0, 0, 0);
                }
                else
                {
                    return new Thickness(0, 0, 480, 0);
                }
            }

            return new Thickness(0, 0, 0, 0);

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
