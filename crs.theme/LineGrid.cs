using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace crs.theme
{
    public class LineGrid : Grid
    {
        private readonly Pen _pen;

        public LineGrid()
        {
            _pen = new Pen(new SolidColorBrush(Colors.Black), 1);
            _pen.Freeze();
        }

        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);

            if (this.RowDefinitions.Count == 0)
            {
                dc.DrawLine(_pen, new Point(0, 0), new Point(this.ActualWidth, 0));
            }

            foreach (RowDefinition item in this.RowDefinitions)
            {
                dc.DrawLine(_pen, new Point(0, item.Offset), new Point(this.ActualWidth, item.Offset));
            }
            dc.DrawLine(_pen, new Point(0, this.ActualHeight), new Point(this.ActualWidth, this.ActualHeight));

            if (this.ColumnDefinitions.Count == 0)
            {
                dc.DrawLine(_pen, new Point(0, 0), new Point(0, this.ActualHeight));
            }

            foreach (ColumnDefinition item in this.ColumnDefinitions)
            {
                dc.DrawLine(_pen, new Point(item.Offset, 0), new Point(item.Offset, this.ActualHeight));
            }
            dc.DrawLine(_pen, new Point(this.ActualWidth, 0), new Point(this.ActualWidth, this.ActualHeight));
        }
    }
}
