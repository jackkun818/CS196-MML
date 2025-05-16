using System;
using System.Collections.Generic;

namespace crs.core.DbModels;

public partial class SystemAdmin
{
    public string Id { get; set; }

    public virtual User IdNavigation { get; set; }
}
