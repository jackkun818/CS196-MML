using crs.core.DbModels;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using static crs.extension.Crs_Enum;

namespace crs.extension.Models
{
    public class DigitalHumanItem : BindableBase
    {
        private string name;
        public string Name
        {
            get { return name; }
            set { SetProperty(ref name, value); }
        }

        private Bitmap image;
        public Bitmap Image
        {
            get { return image; }
            set { SetProperty(ref image, value); }
        }

        public MegaHuman Data { get; private set; }

        public DigitalHumanItem Update(MegaHuman data, Bitmap image = null)
        {
            Data = data;

            Name = data.Name;
            Image = image;

            return this;
        }
    }
}
