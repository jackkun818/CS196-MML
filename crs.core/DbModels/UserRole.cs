using System;
using System.Collections.Generic;

namespace crs.core.DbModels;

public partial class UserRole
{
    public string UserId { get; set; }

    public string RoleId { get; set; }

    public virtual Role Role { get; set; }

    public virtual User RoleNavigation { get; set; }
}
