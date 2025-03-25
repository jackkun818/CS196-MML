using System;
using System.Collections.Generic;

namespace crs.core.DbModels;

public partial class ProgramModulePar
{
    public int ProgramId { get; set; }

    public int ModuleParId { get; set; }

    public double? Value { get; set; }

    public int TableId { get; set; }

    public virtual ModulePar ModulePar { get; set; }
}
