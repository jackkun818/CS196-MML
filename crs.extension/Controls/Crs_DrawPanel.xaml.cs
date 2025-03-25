using crs.extension.Models;
using HandyControl.Controls;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.IO;
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
    /// Crs_DrawPanel.xaml 的交互逻辑
    /// </summary>
    public partial class Crs_DrawPanel : UserControl
    {
        static readonly StrokeCollection DefaultStrokeCollection = new StrokeCollection();

        public Crs_DrawPanel()
        {
            InitializeComponent();
            this.Loaded += Crs_DrawPanel_Loaded;
        }

        private void Crs_DrawPanel_Loaded(object sender, RoutedEventArgs e)
        {
            if (this.DataContext is SubjectChildrenItem childrenItem)
            {
                childrenItem.IsUseBitmap = true;
            }
        }

        private void FallbackButton_Click(object sender, RoutedEventArgs e)
        {
            var drawStrokes = DrawStrokes;
            if (drawStrokes != null && drawStrokes.Count > 0)
            {
                drawStrokes.RemoveAt(drawStrokes.Count - 1);
            }
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            var drawStrokes = DrawStrokes;
            drawStrokes?.Clear();
        }

        public StrokeCollection DrawStrokes
        {
            get { return (StrokeCollection)GetValue(DrawStrokesProperty); }
            set { SetValue(DrawStrokesProperty, value); }
        }

        public static readonly DependencyProperty DrawStrokesProperty =
            DependencyProperty.Register("DrawStrokes", typeof(StrokeCollection), typeof(Crs_DrawPanel), new PropertyMetadata(Crs_DrawPanel.DefaultStrokeCollection, DrawStrokesChangedCallback));

        static void DrawStrokesChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Crs_DrawPanel drawPanel)
            {
                var oldItem = e.OldValue as StrokeCollection;
                if (oldItem != null && !oldItem.Equals(Crs_DrawPanel.DefaultStrokeCollection))
                {
                    oldItem.StrokesChanged -= StrokesChanged;
                }

                var newItem = e.NewValue as StrokeCollection;
                if (newItem != null && !newItem.Equals(Crs_DrawPanel.DefaultStrokeCollection))
                {
                    newItem.StrokesChanged += StrokesChanged;
                }

                OnStrokesChanged(false);

                void StrokesChanged(object sender, StrokeCollectionChangedEventArgs e) => OnStrokesChanged();

                void OnStrokesChanged(bool setData = true)
                {
                    var strokes = drawPanel.DrawStrokes;
                    if (strokes != null)
                    {
                        drawPanel.FallbackButton.IsEnabled = strokes.Count > 0;
                        drawPanel.ClearButton.IsEnabled = strokes.Count > 0;
                    }

                    if (setData && drawPanel.DataContext is SubjectChildrenItem childrenItem)
                    {
                        if (strokes != null && strokes.Count > 0)
                        {
                            using var stream = Crs_ControlToolkit.SaveControlAsStream(drawPanel.DrawSimplePanel);
                            childrenItem.BitmapData = stream.ToArray();
                        }
                        else 
                        {
                            childrenItem.BitmapData = null;
                        }
                    }
                }
            }
        }


        public FrameworkElement BackgroundElement
        {
            get { return (FrameworkElement)GetValue(BackgroundElementProperty); }
            set { SetValue(BackgroundElementProperty, value); }
        }

        public static readonly DependencyProperty BackgroundElementProperty =
            DependencyProperty.Register("BackgroundElement", typeof(FrameworkElement), typeof(Crs_DrawPanel), new PropertyMetadata(null));
    }
}
