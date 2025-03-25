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
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace crs.extension.Controls
{
    [ContentProperty("LeftContent")]
    public class Crs_SubjectReport : Control
    {
        static Crs_SubjectReport()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Crs_SubjectReport), new FrameworkPropertyMetadata(typeof(Crs_SubjectReport)));
        }

        public UIElement LeftContent
        {
            get { return (UIElement)GetValue(LeftContentProperty); }
            set { SetValue(LeftContentProperty, value); }
        }

        public static readonly DependencyProperty LeftContentProperty =
            DependencyProperty.Register("LeftContent", typeof(UIElement), typeof(Crs_SubjectReport), new PropertyMetadata(null));


        public UIElement RightContent
        {
            get { return (UIElement)GetValue(RightContentProperty); }
            set { SetValue(RightContentProperty, value); }
        }

        public static readonly DependencyProperty RightContentProperty =
            DependencyProperty.Register("RightContent", typeof(UIElement), typeof(Crs_SubjectReport), new PropertyMetadata(null));
    }
}
