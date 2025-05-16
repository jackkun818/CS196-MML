using System;
using System.Collections.Generic;

namespace crs.core.DbModels;

public partial class Answer
{
    public int AnswerId { get; set; }

    public int QuestionId { get; set; }

    public string Content { get; set; }

    public bool Correct { get; set; }

    public virtual Question Question { get; set; }

    public virtual ICollection<QuestionResult> QuestionResults { get; set; } = new List<QuestionResult>();
}
