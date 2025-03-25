using System;
using System.Collections.Generic;

namespace crs.core.DbModels;

public partial class ModuleResult
{
    public int ModuleResId { get; set; }

    public int ProgramId { get; set; }

    public int ModuleId { get; set; }

    public string Result { get; set; }

    public DateTime? StartTime { get; set; }

    public DateTime? EndTime { get; set; }

    public virtual Module Module { get; set; }

    public virtual Program Program { get; set; }
}
