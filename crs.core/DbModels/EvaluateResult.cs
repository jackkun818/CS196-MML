using System;
using System.Collections.Generic;

namespace crs.core.DbModels;

public partial class EvaluateResult
{
    public string Type { get; set; }

    public byte[] Data { get; set; }

    public int EvaluateId { get; set; }
}
