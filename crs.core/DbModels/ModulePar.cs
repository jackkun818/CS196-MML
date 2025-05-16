using System;
using System.Collections.Generic;

namespace crs.core.DbModels;

public partial class ModulePar
{
    public int ModuleParId { get; set; }

    public int ModuleId { get; set; }

    public string Name { get; set; }

    public double DefaultValue { get; set; }

    public string FeedbackType { get; set; }

    public double? MaxValue { get; set; }

    public double? MinValue { get; set; }

    public double? Interval { get; set; }

    public string Unit { get; set; }

    public virtual Module Module { get; set; }

    public virtual ICollection<ProgramModulePar> ProgramModulePars { get; set; } = new List<ProgramModulePar>();
}
