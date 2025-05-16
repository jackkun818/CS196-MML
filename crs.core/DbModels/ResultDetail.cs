using System;
using System.Collections.Generic;

namespace crs.core.DbModels;

public partial class ResultDetail
{
    public int ResultId { get; set; }

    public double Value { get; set; }

    public string ValueName { get; set; }

    public int? ModuleId { get; set; }

    public int DetailId { get; set; }

    public int? Lv { get; set; }

    public int? Order { get; set; }

    public int? Maxvalue { get; set; }

    public int? Minvalue { get; set; }

    public string Charttype { get; set; }

    public string Linegraph { get; set; }

    public double? Abscissa { get; set; }

    public string Remark { get; set; }

    public int? EvaluateId { get; set; }

    public virtual Result Result { get; set; }
}
