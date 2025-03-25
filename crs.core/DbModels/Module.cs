using System;
using System.Collections.Generic;

namespace crs.core.DbModels;

public partial class Module
{
    public int ModuleId { get; set; }

    public string Name { get; set; }

    public string Type { get; set; }

    public string Description { get; set; }

    public string TrainType { get; set; }

    public string Instruction { get; set; }

    public virtual ICollection<ModuleParTemp> ModuleParTemps { get; set; } = new List<ModuleParTemp>();

    public virtual ICollection<ModulePar> ModulePars { get; set; } = new List<ModulePar>();

    public virtual ICollection<ModuleResult> ModuleResults { get; set; } = new List<ModuleResult>();

    public virtual ICollection<ProgramModule> ProgramModules { get; set; } = new List<ProgramModule>();

    public virtual ICollection<Question> Questions { get; set; } = new List<Question>();
}
