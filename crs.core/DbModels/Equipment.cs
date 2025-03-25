using System;
using System.Collections.Generic;

namespace crs.core.DbModels;

public partial class Equipment
{
    public int EquipmentId { get; set; }

    public DateTime CreateTime { get; set; }

    public string Type { get; set; }

    public string Name { get; set; }

    public string Status { get; set; }

    public string Version { get; set; }

    public virtual ICollection<Program> Programs { get; set; } = new List<Program>();
}
