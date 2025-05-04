using crs.core.DbModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace crs.core
{
    public class Crs_Db2Context : Db2Context
    {
        // 数据库迁移脚手架
        // Scaffold-DbContext 'Data Source=rm-7xvoo8188db2fasr42o.sqlserver.rds.aliyuncs.com;Initial Catalog = db2; User ID = user2; Password =666@scut; Integrated Security=True; TrustServerCertificate=True; Trusted_Connection=False;' Microsoft.EntityFrameworkCore.SqlServer -o DbModels -f

        public Crs_Db2Context()
            : base()
        {
        }

        public Crs_Db2Context(DbContextOptions<Db2Context> options)
            : base(options)
        {
        }

        // protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        //     => optionsBuilder.UseSqlServer("Data Source=rm-7xvoo8188db2fasr42o.sqlserver.rds.aliyuncs.com;Initial Catalog = db2; User ID = user2; Password =666@scut; Integrated Security=True; TrustServerCertificate=True; Trusted_Connection=False;")
        //     .LogTo(msg => { Crs_LogHelper.Info(msg); }, LogLevel.Information)
        //     .EnableSensitiveDataLogging()
        //     .EnableDetailedErrors();
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.UseSqlServer("Data Source=localhost;Initial Catalog=db2;Integrated Security=True;TrustServerCertificate=True;")
               .LogTo(msg => { Crs_LogHelper.Info(msg); }, LogLevel.Information)
               .EnableSensitiveDataLogging()
               .EnableDetailedErrors();
    }
}
