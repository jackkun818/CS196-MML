using System;
using System.Collections.Generic;

namespace crs.core.DbModels;

public partial class Question
{
    public int QuestionId { get; set; }

    public int ModuleId { get; set; }

    public string Content { get; set; }

    public virtual ICollection<Answer> Answers { get; set; } = new List<Answer>();

    public virtual Module Module { get; set; }

    public virtual ICollection<QuestionResult> QuestionResults { get; set; } = new List<QuestionResult>();
}
