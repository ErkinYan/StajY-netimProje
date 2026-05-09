namespace StajYonetim.Identity.Models.DashboardViewModels;

using StajYonetim.Identity.Core.Models;

public class DashboardHomeViewModel
{
    public List<School> Schools { get; set; } = new();

    public AcademicYear? ActiveYear { get; set; }

    public int SchoolCount { get; set; }

    public int PrincipalCount { get; set; }

    public int StudentCount { get; set; }

    public int BusinessCount { get; set; }
}
