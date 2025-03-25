using System;
using System.Collections.Generic;

namespace crs.core.DbModels;

public partial class User
{
    public string Id { get; set; }

    public string FullName { get; set; }

    public bool? IsActive { get; set; }

    public string RefreshToken { get; set; }

    public DateTime? RefreshTokenExpiryTime { get; set; }

    public string ProfilePictureDataUrl { get; set; }

    public bool? SoftDeleted { get; set; }

    public string UserName { get; set; }

    public string NormalizedUserName { get; set; }

    public string Email { get; set; }

    public string NormalizedEmail { get; set; }

    public bool? EmailConfirmed { get; set; }

    public string PasswordHash { get; set; }

    public string SecurityStamp { get; set; }

    public string ConcurrencyStamp { get; set; }

    public string PhoneNumber { get; set; }

    public bool? PhoneNumberConfirmed { get; set; }

    public bool? TwoFactorEnabled { get; set; }

    public DateTimeOffset? LockoutEnd { get; set; }

    public bool? LockoutEnabled { get; set; }

    public int? AccessFailedCount { get; set; }

    public virtual ICollection<AccountWechatUser> AccountWechatUsers { get; set; } = new List<AccountWechatUser>();

    public virtual Employee Employee { get; set; }

    public virtual Organization Organization { get; set; }

    public virtual PersonalUser PersonalUser { get; set; }

    public virtual SystemAdmin SystemAdmin { get; set; }

    public virtual ICollection<UserClaim> UserClaims { get; set; } = new List<UserClaim>();

    public virtual ICollection<UserLogin> UserLogins { get; set; } = new List<UserLogin>();

    public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();

    public virtual ICollection<UserToken> UserTokens { get; set; } = new List<UserToken>();
}
