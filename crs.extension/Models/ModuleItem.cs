using crs.core.DbModels;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;

namespace crs.extension.Models
{
    public class ModuleItem : BindableBase
    {
        private string name;
        public string Name
        {
            get { return name; }
            set { SetProperty(ref name, value); }
        }

        public Module Data { get; private set; }

        public ModuleItem Update(Module data)
        {
            Data = data;

            Name = data.Name?.Trim();

            return this;
        }
    }
}
