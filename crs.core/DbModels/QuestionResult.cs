using System;
using System.Collections.Generic;

namespace crs.core.DbModels;

public partial class QuestionResult
{
    public int QuestionResId { get; set; }

    public int ProgramId { get; set; }

    public int QuestionId { get; set; }

    public int? SelectAnswerId { get; set; }

    public DateTime? StartTime { get; set; }

    public DateTime? EndTime { get; set; }

    public string Correct { get; set; }

    public virtual Program Program { get; set; }

    public virtual Question Question { get; set; }

    public virtual Answer SelectAnswer { get; set; }
}
