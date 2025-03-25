using System;
using System.Collections.Generic;

namespace crs.core.DbModels;

public partial class MegaHuman
{
    public int MegaHumanId { get; set; }

    public string Name { get; set; }

    public DateTime CreateTime { get; set; }

    public DateTime? LastEditTime { get; set; }

    public string SoundPar { get; set; }

    public string AppearPar { get; set; }

    public virtual ICollection<PatientUser> PatientUsers { get; set; } = new List<PatientUser>();
}
