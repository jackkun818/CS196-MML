using System;
using System.Collections.Generic;

namespace crs.core.DbModels;

public partial class AccountWechatUser
{
    public int Id { get; set; }

    public string AccountId { get; set; }

    public string OpenId { get; set; }

    public string UnionId { get; set; }

    public string NickName { get; set; }

    public virtual User Account { get; set; }
}
