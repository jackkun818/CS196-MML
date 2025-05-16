using System;
using System.Collections.Generic;

namespace crs.core.DbModels;

public partial class ProgramModule
{
    public int ProgramId { get; set; }

    public int ModuleId { get; set; }

    public int TableId { get; set; }

    public virtual Module Module { get; set; }

    public virtual Program Program { get; set; }
}
