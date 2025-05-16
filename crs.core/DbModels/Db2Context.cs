using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace crs.core.DbModels;

public partial class Db2Context : DbContext
{
    public Db2Context()
    {
    }

    public Db2Context(DbContextOptions<Db2Context> options)
        : base(options)
    {
    }

    public virtual DbSet<AccountWechatUser> AccountWechatUsers { get; set; }

    public virtual DbSet<Answer> Answers { get; set; }

    public virtual DbSet<ChargingItem> ChargingItems { get; set; }

    public virtual DbSet<Edition> Editions { get; set; }

    public virtual DbSet<Employee> Employees { get; set; }

    public virtual DbSet<Equipment> Equipment { get; set; }

    public virtual DbSet<EvaluateResult> EvaluateResults { get; set; }

    public virtual DbSet<MedicalAdvice> MedicalAdvices { get; set; }

    public virtual DbSet<MedicalAdviecsOrder> MedicalAdviecsOrders { get; set; }

    public virtual DbSet<MedicalRecord> MedicalRecords { get; set; }

    public virtual DbSet<MegaHuman> MegaHumans { get; set; }

    public virtual DbSet<Module> Modules { get; set; } 

    public virtual DbSet<ModulePar> ModulePars { get; set; }

    public virtual DbSet<ModuleParTemp> ModuleParTemps { get; set; }

    public virtual DbSet<ModuleResult> ModuleResults { get; set; }

    public virtual DbSet<Organization> Organizations { get; set; }

    public virtual DbSet<OrganizationPatient> OrganizationPatients { get; set; }

    public virtual DbSet<Patient> Patients { get; set; }

    public virtual DbSet<PatientUser> PatientUsers { get; set; }

    public virtual DbSet<PersonalUser> PersonalUsers { get; set; }

    public virtual DbSet<Program> Programs { get; set; }

    public virtual DbSet<ProgramModule> ProgramModules { get; set; }

    public virtual DbSet<ProgramModulePar> ProgramModulePars { get; set; }

    public virtual DbSet<Question> Questions { get; set; }

    public virtual DbSet<QuestionResult> QuestionResults { get; set; }

    public virtual DbSet<QuestionType> QuestionTypes { get; set; }

    public virtual DbSet<Result> Results { get; set; }

    public virtual DbSet<ResultDetail> ResultDetails { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<RoleClaim> RoleClaims { get; set; }

    public virtual DbSet<Schedule> Schedules { get; set; }

    public virtual DbSet<SystemAdmin> SystemAdmins { get; set; }

    public virtual DbSet<Therapist> Therapists { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserClaim> UserClaims { get; set; }

    public virtual DbSet<UserLogin> UserLogins { get; set; }

    public virtual DbSet<UserRole> UserRoles { get; set; }

    public virtual DbSet<UserToken> UserTokens { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Data Source=localhost\\SQLDEVELOPER;Initial Catalog=db2;Integrated Security=True;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AccountWechatUser>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__account___3214EC07951E4758");

            entity.ToTable("account_wechat_user");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.AccountId).HasMaxLength(450);
            entity.Property(e => e.OpenId).HasMaxLength(450);
            entity.Property(e => e.UnionId).HasMaxLength(450);

            entity.HasOne(d => d.Account).WithMany(p => p.AccountWechatUsers)
                .HasForeignKey(d => d.AccountId)
                .HasConstraintName("FK_account_wechat_user_user");
        });

        modelBuilder.Entity<Answer>(entity =>
        {
            entity.ToTable("answer");

            entity.Property(e => e.AnswerId).HasColumnName("answer_id");
            entity.Property(e => e.Content)
                .IsRequired()
                .HasColumnType("xml")
                .HasColumnName("content");
            entity.Property(e => e.Correct).HasColumnName("correct");
            entity.Property(e => e.QuestionId).HasColumnName("question_id");

            entity.HasOne(d => d.Question).WithMany(p => p.Answers)
                .HasForeignKey(d => d.QuestionId)
                .HasConstraintName("FK_answer_question");
        });

        modelBuilder.Entity<ChargingItem>(entity =>
        {
            entity.ToTable("charging_item");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.CreatedBy).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(200);
            entity.Property(e => e.LastModifiedBy).IsRequired();
            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(50);
            entity.Property(e => e.OrganizationId).HasMaxLength(450);
            entity.Property(e => e.Price).HasColumnType("decimal(18, 2)");
        });

        modelBuilder.Entity<Edition>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("edition");

            entity.Property(e => e.EditTime)
                .HasColumnType("datetime")
                .HasColumnName("editTime");
            entity.Property(e => e.ProgramId).HasColumnName("program_id");
            entity.Property(e => e.TherapistId).HasColumnName("therapist_id");

            entity.HasOne(d => d.Program).WithMany()
                .HasForeignKey(d => d.ProgramId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_edition_program");

            entity.HasOne(d => d.Therapist).WithMany()
                .HasForeignKey(d => d.TherapistId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_edition_therapist");
        });

        modelBuilder.Entity<Employee>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__employee__3214EC0732198210");

            entity.ToTable("employee");

            entity.Property(e => e.OrganizationId).HasMaxLength(450);

            entity.HasOne(d => d.IdNavigation).WithOne(p => p.Employee)
                .HasForeignKey<Employee>(d => d.Id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_employee_user");

            entity.HasOne(d => d.Organization).WithMany(p => p.Employees)
                .HasForeignKey(d => d.OrganizationId)
                .HasConstraintName("FK_employee_organization");
        });

        modelBuilder.Entity<Equipment>(entity =>
        {
            entity.ToTable("equipment");

            entity.Property(e => e.EquipmentId).HasColumnName("equipment_id");
            entity.Property(e => e.CreateTime)
                .HasColumnType("datetime")
                .HasColumnName("createTime");
            entity.Property(e => e.Name)
                .IsRequired()
                .HasColumnName("name");
            entity.Property(e => e.Status).HasColumnName("status");
            entity.Property(e => e.Type)
                .IsRequired()
                .HasColumnName("type");
            entity.Property(e => e.Version)
                .IsRequired()
                .IsUnicode(false)
                .HasColumnName("version");
        });

        modelBuilder.Entity<EvaluateResult>(entity =>
        {
            entity.HasKey(e => e.EvaluateId).HasName("PK__evaluateResult");

            entity.ToTable("evaluateResult");

            entity.Property(e => e.EvaluateId).HasColumnName("evaluate_id");
            entity.Property(e => e.Data).HasColumnName("data");
            entity.Property(e => e.Type)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("type");
        });

        modelBuilder.Entity<MedicalAdvice>(entity =>
        {
            entity.ToTable("medical_advice");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.ExecuteDates).IsRequired();
            entity.Property(e => e.OrganizationId).IsRequired();
            entity.Property(e => e.TrainingModes).IsRequired();

            entity.HasOne(d => d.MedicalAdvicesOrder).WithMany(p => p.MedicalAdvices)
                .HasForeignKey(d => d.MedicalAdvicesOrderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_medical_advice_medical_adviecs_order");
        });

        modelBuilder.Entity<MedicalAdviecsOrder>(entity =>
        {
            entity.ToTable("medical_adviecs_order");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Organizationld).IsRequired();

            entity.HasOne(d => d.MedicalRecord).WithMany(p => p.MedicalAdviecsOrders)
                .HasForeignKey(d => d.MedicalRecordId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_medical_adviecs_order_medical_record");
        });

        modelBuilder.Entity<MedicalRecord>(entity =>
        {
            entity.ToTable("medical_record");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Organizationld).IsRequired();
            entity.Property(e => e.PersonalMarriageFamilyHistories).HasColumnName("Personal_Marriage_Family_Histories");
        });

        modelBuilder.Entity<MegaHuman>(entity =>
        {
            entity.ToTable("megaHuman");

            entity.Property(e => e.MegaHumanId).HasColumnName("megaHuman_id");
            entity.Property(e => e.AppearPar)
                .IsRequired()
                .HasColumnType("xml")
                .HasColumnName("appearPar");
            entity.Property(e => e.CreateTime)
                .HasColumnType("datetime")
                .HasColumnName("createTime");
            entity.Property(e => e.LastEditTime)
                .HasColumnType("datetime")
                .HasColumnName("lastEditTime");
            entity.Property(e => e.Name)
                .IsRequired()
                .HasColumnName("name");
            entity.Property(e => e.SoundPar)
                .IsRequired()
                .HasColumnType("xml")
                .HasColumnName("soundPar");
        });

        modelBuilder.Entity<Module>(entity =>
        {
            entity.ToTable("module");

            entity.Property(e => e.ModuleId).HasColumnName("module_id");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.Instruction).HasColumnName("instruction");
            entity.Property(e => e.Name)
                .IsRequired()
                .HasColumnName("name");
            entity.Property(e => e.TrainType)
                .HasMaxLength(10)
                .IsFixedLength()
                .HasColumnName("trainType");
            entity.Property(e => e.Type)
                .IsRequired()
                .HasColumnName("type");
        });

        modelBuilder.Entity<ModulePar>(entity =>
        {
            entity.ToTable("modulePar");

            entity.Property(e => e.ModuleParId).HasColumnName("modulePar_id");
            entity.Property(e => e.DefaultValue).HasColumnName("defaultValue");
            entity.Property(e => e.FeedbackType)
                .HasMaxLength(50)
                .HasColumnName("feedbackType");
            entity.Property(e => e.Interval).HasColumnName("interval");
            entity.Property(e => e.MaxValue).HasColumnName("maxValue");
            entity.Property(e => e.MinValue).HasColumnName("minValue");
            entity.Property(e => e.ModuleId).HasColumnName("module_id");
            entity.Property(e => e.Name)
                .IsRequired()
                .HasColumnName("name");
            entity.Property(e => e.Unit)
                .HasMaxLength(10)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("unit");

            entity.HasOne(d => d.Module).WithMany(p => p.ModulePars)
                .HasForeignKey(d => d.ModuleId)
                .HasConstraintName("FK_modulePar_module");
        });

        modelBuilder.Entity<ModuleParTemp>(entity =>
        {
            entity.HasKey(e => e.ModuleParId).HasName("PK__modulePa__C48BF82C9ADA201A");

            entity.ToTable("moduleParTemp");

            entity.Property(e => e.ModuleParId).HasColumnName("modulePar_id");
            entity.Property(e => e.DefaultValue).HasColumnName("defaultValue");
            entity.Property(e => e.FeedbackType)
                .HasMaxLength(50)
                .HasColumnName("feedbackType");
            entity.Property(e => e.Interval).HasColumnName("interval");
            entity.Property(e => e.MaxValue).HasColumnName("maxValue");
            entity.Property(e => e.MinValue).HasColumnName("minValue");
            entity.Property(e => e.ModuleId).HasColumnName("module_id");
            entity.Property(e => e.Name)
                .IsRequired()
                .HasColumnName("name");
            entity.Property(e => e.Unit)
                .HasMaxLength(10)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("unit");

            entity.HasOne(d => d.Module).WithMany(p => p.ModuleParTemps)
                .HasForeignKey(d => d.ModuleId)
                .HasConstraintName("FK__modulePar__modul__345EC57D");
        });

        modelBuilder.Entity<ModuleResult>(entity =>
        {
            entity.HasKey(e => e.ModuleResId);

            entity.ToTable("moduleResult");

            entity.Property(e => e.ModuleResId).HasColumnName("moduleRes_id");
            entity.Property(e => e.EndTime)
                .HasColumnType("datetime")
                .HasColumnName("endTime");
            entity.Property(e => e.ModuleId).HasColumnName("module_id");
            entity.Property(e => e.ProgramId).HasColumnName("program_id");
            entity.Property(e => e.Result)
                .IsRequired()
                .HasColumnType("xml")
                .HasColumnName("result");
            entity.Property(e => e.StartTime)
                .HasColumnType("datetime")
                .HasColumnName("startTime");

            entity.HasOne(d => d.Module).WithMany(p => p.ModuleResults)
                .HasForeignKey(d => d.ModuleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_moduleResult_module");

            entity.HasOne(d => d.Program).WithMany(p => p.ModuleResults)
                .HasForeignKey(d => d.ProgramId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_moduleResult_program");
        });

        modelBuilder.Entity<Organization>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__organiza__3214EC0778A6E34B");

            entity.ToTable("organization");

            entity.Property(e => e.Address).HasMaxLength(200);
            entity.Property(e => e.City).HasMaxLength(50);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Province).HasMaxLength(50);

            entity.HasOne(d => d.IdNavigation).WithOne(p => p.Organization)
                .HasForeignKey<Organization>(d => d.Id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_organization_user");
        });

        modelBuilder.Entity<OrganizationPatient>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__organiza__3214EC071FF8E220");

            entity.ToTable("organization_patient");

            entity.Property(e => e.Career).HasMaxLength(50);
            entity.Property(e => e.CreatedOn).HasColumnType("datetime");
            entity.Property(e => e.DoctorId).HasMaxLength(450);
            entity.Property(e => e.LastModifiedOn).HasColumnType("datetime");
            entity.Property(e => e.OrganizationId).HasMaxLength(450);
            entity.Property(e => e.Province).HasMaxLength(50);

            entity.HasOne(d => d.Doctor).WithMany(p => p.OrganizationPatients)
                .HasForeignKey(d => d.DoctorId)
                .HasConstraintName("FK_organization_patient_employee");

            entity.HasOne(d => d.LatestMedRecord).WithMany(p => p.OrganizationPatients)
                .HasForeignKey(d => d.LatestMedRecordId)
                .HasConstraintName("FK_organization_patient_medical_record");

            entity.HasOne(d => d.Organization).WithMany(p => p.OrganizationPatients)
                .HasForeignKey(d => d.OrganizationId)
                .HasConstraintName("FK_organization_patient_organization");
        });

        modelBuilder.Entity<Patient>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__patient___3214EC07B17A4EA6");

            entity.ToTable("patient");

            entity.Property(e => e.PhoneNumber).HasMaxLength(450);
            entity.Property(e => e.UserId).HasMaxLength(450);

            entity.HasOne(d => d.User).WithMany(p => p.Patients)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_patient_user_personal_user");
        });

        modelBuilder.Entity<PatientUser>(entity =>
        {
            entity.HasKey(e => e.PatientId).HasName("PK_patient");

            entity.ToTable("patient_user");

            entity.Property(e => e.PatientId)
                .ValueGeneratedNever()
                .HasColumnName("patient_id");
            entity.Property(e => e.MegaHumanId).HasColumnName("megaHuman_id");

            entity.HasOne(d => d.MegaHuman).WithMany(p => p.PatientUsers)
                .HasForeignKey(d => d.MegaHumanId)
                .HasConstraintName("FK_patient_megaHuman");

            entity.HasOne(d => d.Patient).WithOne(p => p.PatientUser)
                .HasForeignKey<PatientUser>(d => d.PatientId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_patient_patient_user");
        });

        modelBuilder.Entity<PersonalUser>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__personal__3214EC07CFFA9C3F");

            entity.ToTable("personal_user");

            entity.HasOne(d => d.IdNavigation).WithOne(p => p.PersonalUser)
                .HasForeignKey<PersonalUser>(d => d.Id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_personal_user_user");
        });

        modelBuilder.Entity<Program>(entity =>
        {
            entity.HasKey(e => e.ProgramId).HasName("PK_program_new");

            entity.ToTable("program");

            entity.Property(e => e.ProgramId).HasColumnName("program_id");
            entity.Property(e => e.ActEndTime).HasColumnType("datetime");
            entity.Property(e => e.ActStartTime).HasColumnType("datetime");
            entity.Property(e => e.CreateTime).HasColumnType("datetime");
            entity.Property(e => e.EndTime).HasColumnType("datetime");
            entity.Property(e => e.EquipmentId).HasColumnName("equipment_id");
            entity.Property(e => e.Eval).HasColumnName("eval");
            entity.Property(e => e.PatientId).HasColumnName("patient_id");
            entity.Property(e => e.ProcessTherapistId).HasColumnName("processTherapist_id");
            entity.Property(e => e.Remark).HasColumnName("remark");
            entity.Property(e => e.ScheduleId).HasColumnName("schedule_id");
            entity.Property(e => e.SoftDel).HasColumnName("softDel");
            entity.Property(e => e.StartTime).HasColumnType("datetime");

            entity.HasOne(d => d.Equipment).WithMany(p => p.Programs)
                .HasForeignKey(d => d.EquipmentId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_program_equipment");

            entity.HasOne(d => d.Patient).WithMany(p => p.Programs)
                .HasForeignKey(d => d.PatientId)
                .HasConstraintName("FK_program_organization_patient");

            entity.HasOne(d => d.ProcessTherapist).WithMany(p => p.Programs)
                .HasForeignKey(d => d.ProcessTherapistId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_program_therapist");
        });

        modelBuilder.Entity<ProgramModule>(entity =>
        {
            entity.HasKey(e => e.TableId);

            entity.ToTable("programModule");

            entity.Property(e => e.TableId).HasColumnName("table_id");
            entity.Property(e => e.ModuleId).HasColumnName("module_id");
            entity.Property(e => e.ProgramId).HasColumnName("program_id");

            entity.HasOne(d => d.Module).WithMany(p => p.ProgramModules)
                .HasForeignKey(d => d.ModuleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_programModule_module");

            entity.HasOne(d => d.Program).WithMany(p => p.ProgramModules)
                .HasForeignKey(d => d.ProgramId)
                .HasConstraintName("FK_programModule_program");
        });

        modelBuilder.Entity<ProgramModulePar>(entity =>
        {
            entity.HasKey(e => e.TableId);

            entity.ToTable("programModulePar");

            entity.Property(e => e.TableId).HasColumnName("table_id");
            entity.Property(e => e.ModuleParId).HasColumnName("modulePar_id");
            entity.Property(e => e.ProgramId).HasColumnName("program_id");
            entity.Property(e => e.Value).HasColumnName("value");

            entity.HasOne(d => d.ModulePar).WithMany(p => p.ProgramModulePars)
                .HasForeignKey(d => d.ModuleParId)
                .HasConstraintName("FK_programModulePar_modulePar");
        });

        modelBuilder.Entity<Question>(entity =>
        {
            entity.ToTable("question");

            entity.Property(e => e.QuestionId).HasColumnName("question_id");
            entity.Property(e => e.Content)
                .IsRequired()
                .HasColumnType("xml")
                .HasColumnName("content");
            entity.Property(e => e.ModuleId).HasColumnName("module_id");

            entity.HasOne(d => d.Module).WithMany(p => p.Questions)
                .HasForeignKey(d => d.ModuleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_question_module");
        });

        modelBuilder.Entity<QuestionResult>(entity =>
        {
            entity.HasKey(e => e.QuestionResId);

            entity.ToTable("questionResult");

            entity.Property(e => e.QuestionResId).HasColumnName("questionRes_id");
            entity.Property(e => e.Correct)
                .IsRequired()
                .HasMaxLength(10)
                .IsFixedLength()
                .HasColumnName("correct");
            entity.Property(e => e.EndTime)
                .HasColumnType("datetime")
                .HasColumnName("endTime");
            entity.Property(e => e.ProgramId).HasColumnName("program_id");
            entity.Property(e => e.QuestionId).HasColumnName("question_id");
            entity.Property(e => e.SelectAnswerId).HasColumnName("selectAnswer_id");
            entity.Property(e => e.StartTime)
                .HasColumnType("datetime")
                .HasColumnName("startTime");

            entity.HasOne(d => d.Program).WithMany(p => p.QuestionResults)
                .HasForeignKey(d => d.ProgramId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_questionResult_program");

            entity.HasOne(d => d.Question).WithMany(p => p.QuestionResults)
                .HasForeignKey(d => d.QuestionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_questionResult_question");

            entity.HasOne(d => d.SelectAnswer).WithMany(p => p.QuestionResults)
                .HasForeignKey(d => d.SelectAnswerId)
                .HasConstraintName("FK_questionResult_answer");
        });

        modelBuilder.Entity<QuestionType>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("questionType");

            entity.Property(e => e.ModuleParId).HasColumnName("modulePar_id");
            entity.Property(e => e.QuestionId).HasColumnName("question_id");
            entity.Property(e => e.Value).HasColumnName("value");

            entity.HasOne(d => d.ModulePar).WithMany()
                .HasForeignKey(d => d.ModuleParId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_questionType_modulePar");

            entity.HasOne(d => d.Question).WithMany()
                .HasForeignKey(d => d.QuestionId)
                .HasConstraintName("FK_questionType_question");
        });

        modelBuilder.Entity<Result>(entity =>
        {
            entity.ToTable("result");

            entity.Property(e => e.ResultId).HasColumnName("result_id");
            entity.Property(e => e.Eval).HasColumnName("eval");
            entity.Property(e => e.IsDisplay).HasColumnName("isDisplay");
            entity.Property(e => e.Lv).HasColumnName("lv");
            entity.Property(e => e.ProgramId).HasColumnName("program_id");
            entity.Property(e => e.Remark).HasColumnName("remark");
            entity.Property(e => e.Report).HasColumnName("report");
            entity.Property(e => e.ScheduleId).HasColumnName("schedule_id");

            entity.HasOne(d => d.Program).WithMany(p => p.Results)
                .HasForeignKey(d => d.ProgramId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_result_program");

            entity.HasOne(d => d.Schedule).WithMany(p => p.Results)
                .HasForeignKey(d => d.ScheduleId)
                .HasConstraintName("FK_result_schedule");
        });

        modelBuilder.Entity<ResultDetail>(entity =>
        {
            entity.HasKey(e => e.DetailId);

            entity.ToTable("resultDetail");

            entity.Property(e => e.DetailId).HasColumnName("detail_id");
            entity.Property(e => e.Abscissa).HasColumnName("abscissa");
            entity.Property(e => e.Charttype).HasColumnName("charttype");
            entity.Property(e => e.EvaluateId).HasColumnName("evaluate_id");
            entity.Property(e => e.Linegraph)
                .IsUnicode(false)
                .HasColumnName("linegraph");
            entity.Property(e => e.Lv).HasColumnName("lv");
            entity.Property(e => e.Maxvalue).HasColumnName("maxvalue");
            entity.Property(e => e.Minvalue).HasColumnName("minvalue");
            entity.Property(e => e.ModuleId)
                .HasDefaultValueSql("(NULL)")
                .HasColumnName("module_id");
            entity.Property(e => e.Order).HasColumnName("order");
            entity.Property(e => e.ResultId).HasColumnName("result_id");
            entity.Property(e => e.Value).HasColumnName("value");
            entity.Property(e => e.ValueName)
                .IsRequired()
                .HasMaxLength(50)
                .HasColumnName("valueName");

            entity.HasOne(d => d.Result).WithMany(p => p.ResultDetails)
                .HasForeignKey(d => d.ResultId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_result_resultDetail");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__role__3214EC07481D30DF");

            entity.ToTable("role");

            entity.Property(e => e.Description).HasMaxLength(256);
            entity.Property(e => e.Name).HasMaxLength(256);
            entity.Property(e => e.NormalizedName).HasMaxLength(256);
        });

        modelBuilder.Entity<RoleClaim>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__role_cla__3214EC07A59B23E5");

            entity.ToTable("role_claim");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.RoleId).HasMaxLength(450);

            entity.HasOne(d => d.Role).WithMany(p => p.RoleClaims)
                .HasForeignKey(d => d.RoleId)
                .HasConstraintName("FK_role_claim_role");
        });

        modelBuilder.Entity<Schedule>(entity =>
        {
            entity.ToTable("schedule");

            entity.Property(e => e.ScheduleId).HasColumnName("schedule_id");
            entity.Property(e => e.CreateTime)
                .HasColumnType("datetime")
                .HasColumnName("createTime");
            entity.Property(e => e.EndTime).HasColumnType("datetime");
            entity.Property(e => e.PatientId).HasColumnName("patient_id");
            entity.Property(e => e.ProgramId).HasColumnName("program_id");
            entity.Property(e => e.Remark).HasColumnName("remark");
            entity.Property(e => e.SoftDel).HasColumnName("softDel");
            entity.Property(e => e.StartTime).HasColumnType("datetime");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("status");
            entity.Property(e => e.TherapistId).HasColumnName("therapist_id");

            entity.HasOne(d => d.Patient).WithMany(p => p.Schedules)
                .HasForeignKey(d => d.PatientId)
                .HasConstraintName("FK_schedule_patient");

            entity.HasOne(d => d.Program).WithMany(p => p.Schedules)
                .HasForeignKey(d => d.ProgramId)
                .HasConstraintName("FK_schedule_program");

            entity.HasOne(d => d.Therapist).WithMany(p => p.Schedules)
                .HasForeignKey(d => d.TherapistId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_schedule_therapist");
        });

        modelBuilder.Entity<SystemAdmin>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__SystemAd__3214EC07500E5767");

            entity.HasOne(d => d.IdNavigation).WithOne(p => p.SystemAdmin)
                .HasForeignKey<SystemAdmin>(d => d.Id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SystemAdmins_user");
        });

        modelBuilder.Entity<Therapist>(entity =>
        {
            entity.ToTable("therapist");

            entity.Property(e => e.TherapistId).HasColumnName("therapist_id");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__user__3214EC078DE18A63");

            entity.ToTable("user");

            entity.Property(e => e.Email).HasMaxLength(256);
            entity.Property(e => e.FullName).HasMaxLength(50);
            entity.Property(e => e.NormalizedEmail).HasMaxLength(256);
            entity.Property(e => e.NormalizedUserName).HasMaxLength(256);
            entity.Property(e => e.ProfilePictureDataUrl).HasColumnType("text");
            entity.Property(e => e.UserName).HasMaxLength(256);
        });

        modelBuilder.Entity<UserClaim>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__user_cla__3214EC07219A807A");

            entity.ToTable("user_claim");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.UserId).HasMaxLength(450);

            entity.HasOne(d => d.User).WithMany(p => p.UserClaims)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_user_claim_user");
        });

        modelBuilder.Entity<UserLogin>(entity =>
        {
            entity.HasKey(e => new { e.LoginProvider, e.ProviderKey }).HasName("PK__user_log__2B2C5B52F9584207");

            entity.ToTable("user_login");

            entity.Property(e => e.LoginProvider).HasMaxLength(128);
            entity.Property(e => e.ProviderKey).HasMaxLength(128);
            entity.Property(e => e.UserId).HasMaxLength(450);

            entity.HasOne(d => d.User).WithMany(p => p.UserLogins)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_user_login_user");
        });

        modelBuilder.Entity<UserRole>(entity =>
        {
            entity.HasKey(e => new { e.UserId, e.RoleId }).HasName("PK__user_rol__AF2760ADE2D56685");

            entity.ToTable("user_role");

            entity.HasOne(d => d.Role).WithMany(p => p.UserRoles)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_user_role_role1");

            entity.HasOne(d => d.RoleNavigation).WithMany(p => p.UserRoles)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_user_role_role");
        });

        modelBuilder.Entity<UserToken>(entity =>
        {
            entity.HasKey(e => new { e.UserId, e.LoginProvider, e.Name }).HasName("PK__user_tok__8CC49841281AC6E4");

            entity.ToTable("user_token");

            entity.Property(e => e.LoginProvider).HasMaxLength(128);
            entity.Property(e => e.Name).HasMaxLength(128);

            entity.HasOne(d => d.User).WithMany(p => p.UserTokens)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_user_token_user");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
