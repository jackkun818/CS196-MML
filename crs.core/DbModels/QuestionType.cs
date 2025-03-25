using System;
using System.Collections.Generic;

namespace crs.core.DbModels;

public partial class QuestionType
{
    public int ModuleParId { get; set; }

    public int QuestionId { get; set; }

    public double Value { get; set; }

    public virtual ModulePar ModulePar { get; set; }

    public virtual Question Question { get; set; }
}
