using System;
using System.Collections.Generic;

namespace crs.core.DbModels;

public partial class Result
{
    public int ResultId { get; set; }

    public int ProgramId { get; set; }

    public string Report { get; set; }

    public string Remark { get; set; }

    public bool? Eval { get; set; }

    public int? Lv { get; set; }

    public int? ScheduleId { get; set; }

    public bool? IsDisplay { get; set; }

    public virtual Program Program { get; set; }

    public virtual ICollection<ResultDetail> ResultDetails { get; set; } = new List<ResultDetail>();

    public virtual Schedule Schedule { get; set; }
}
