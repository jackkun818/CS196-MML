using HandyControl.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace crs.extension.Controls
{
    public class Crs_Carousel : Carousel
    {
        public override void OnApplyTemplate()
        {
            var _panelPage = GetTemplateChild("PART_PanelPage") as Panel;
            base.OnApplyTemplate();
            _panelPage?.RemoveHandler(ButtonBase.ClickEvent, new RoutedEventHandler(ButtonPages_OnClick));
            _panelPage?.AddHandler(ButtonBase.ClickEvent, new RoutedEventHandler(ButtonPages_OnClick));
        }

        private void ButtonPages_OnClick(object sender, RoutedEventArgs e)
        {
            IsFirstItem = PageIndex <= 0;
            IsLastItem = PageIndex == base.Items.Count - 1;

            this.RaiseEvent(new RoutedEventArgs(PageChangedEvent, this));
        }


        public bool IsFirstItem
        {
            get { return (bool)GetValue(IsFirstItemProperty); }
            set { SetValue(IsFirstItemProperty, value); }
        }

        public static readonly DependencyProperty IsFirstItemProperty =
            DependencyProperty.Register("IsFirstItem", typeof(bool), typeof(Crs_Carousel), new PropertyMetadata(false));


        public bool IsLastItem
        {
            get { return (bool)GetValue(IsLastItemProperty); }
            set { SetValue(IsLastItemProperty, value); }
        }

        public static readonly DependencyProperty IsLastItemProperty =
            DependencyProperty.Register("IsLastItem", typeof(bool), typeof(Crs_Carousel), new PropertyMetadata(null));


        public static readonly RoutedEvent PageChangedEvent =
            EventManager.RegisterRoutedEvent("PageChanged", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(Crs_Carousel));

        public event RoutedEventHandler PageChanged
        {
            add { AddHandler(PageChangedEvent, value); }
            remove { RemoveHandler(PageChangedEvent, value); }
        }
    }
}
