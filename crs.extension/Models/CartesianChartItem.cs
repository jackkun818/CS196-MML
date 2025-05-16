using LiveChartsCore;
using LiveChartsCore.Kernel.Sketches;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;

namespace crs.extension.Models
{
    public class CartesianChartItem : BindableBase
    {
        private ICartesianAxis[] xAxes;
        public ICartesianAxis[] XAxes
        {
            get { return xAxes; }
            set { SetProperty(ref xAxes, value); }
        }

        private ICartesianAxis[] yAxes;
        public ICartesianAxis[] YAxes
        {
            get { return yAxes; }
            set { SetProperty(ref yAxes, value); }
        }

        private ISeries[] series;
        public ISeries[] Series
        {
            get { return series; }
            set { SetProperty(ref series, value); }
        }
    }
}
